# NServiceBus.Connector.SqlServer

A package that allows atomic sending and database updates in a send-only endpoint in a transport-agnostic way

## Why?

[The Outbox](https://docs.particular.net/nservicebus/outbox/) is one of the most useful and powerful features of NServiceBus. It guarantees that sending messages and updating database is done atomically -- either both are completed or neither is. It frees the developer from thinking about partial failure scenarios such as when messages have been sent but DB update failed or vice versa. 

Unfortunately the Outbox works only in the context of a message handler. More specifically, it does not help if the code you are writing is in a MVC controller that needs to both modify the data and send/publish a message. 

Say hello to the Connector!


## How do I use it?

Here's how the controller code looks like when using plain ADO.NET data access

```
public SendMessageController(IMessageSession messageSession, SqlConnection conn, SqlTransaction trans)
{
    this.messageSession = messageSession;
    this.conn = conn;
    this.trans = trans;
}

[HttpGet]
public async Task<string> Get()
{
    await messageSession.Send(new MyMessage())
        .ConfigureAwait(false);

    await messageSession.Publish(new MyEvent())
        .ConfigureAwait(false);

    using (var command = new SqlCommand("insert into Widgets default values;", conn, trans))
    {
        await command.ExecuteNonQueryAsync()
            .ConfigureAwait(false);
    }

    return "Message sent to endpoint";
}
```

As you can see, the controller is not aware of the fact that it uses connector. The message session and the data access context (the connection and the transaction) are passed to it via dependency injection (DI).

The configuration part is only a little bit more complex. Here's part of the `setup.cs`:

```
//The system uses RabbitMQ transport
var connectorConfig = new ConnectorConfiguration<RabbitMQTransport>(
    name: "WebApplication",
    sqlConnectionString: ConnectionString,
    customizeConnectedTransport: extensions =>
    {
        extensions.ConnectionString("host=localhost");
    });

connectorConfig.AutoCreateQueues();

//Configure where to send messages
connectorConfig.RouteToEndpoint(
    messageType: typeof(MyMessage),
    endpointName: "Samples.ASPNETCore.Endpoint");

//Start the connector
connector = connectorConfig.CreateConnector();
connector.Start().GetAwaiter().GetResult();

//Configure the DB connection/transaction management
services.UseSqlServer(ConnectionString);
services.UseOneTransactionPerHttpCall();

//Configure injection of the message session managed by the connector
serviceCollection.AddScoped(provider =>
            connector.GetSession(provider.GetService<SqlConnection>(), provider.GetService<SqlTransaction>()));
```

## How it works?

The connector bundles together an NServiceBus send-only endpoint that uses SQL Server transport and a [NServiceBus.Router](https://github.com/szymonpobiega/nservicebus.router) instance that routes messages between the SQL Server transport and the connected transport (the transport that the rest of the system uses).

The message session injected into the controller has access to the per-request connection and transaction and passes them to the SQL Server transport dispatcher. This way when the transaction commits, it persists both messages and data manipulations.

Then the Router SQL Server interface picks up the message and forwards it to the connected interface which, in turn, forwards it to the destination.

The connector supports both sends and publishes but if the connected transport does not support Pub/Sub natively, the connector requires the user to enable the message-driven Pub/Sub in the connected transport (see sample). In that case the subscription information is stored by the router.
