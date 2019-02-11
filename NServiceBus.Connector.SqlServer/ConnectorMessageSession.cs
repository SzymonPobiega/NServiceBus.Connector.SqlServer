using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Extensibility;
using NServiceBus.Transport;

class ConnectorMessageSession : IMessageSession
{
    IEndpointInstance sendOnlyEndpoint;
    TransportTransaction transportTransaction;

    public ConnectorMessageSession(IEndpointInstance sendOnlyEndpoint, SqlConnection connection, SqlTransaction transaction)
    {
        this.sendOnlyEndpoint = sendOnlyEndpoint;
        transportTransaction = new TransportTransaction();
        transportTransaction.Set(connection);
        transportTransaction.Set(transaction);
    }

    public Task Send(object message, SendOptions options)
    {
        options.GetExtensions().Set(transportTransaction);
        return sendOnlyEndpoint.Send(message, options);
    }

    public Task Send<T>(Action<T> messageConstructor, SendOptions options)
    {
        options.GetExtensions().Set(transportTransaction);
        return sendOnlyEndpoint.Send(messageConstructor, options);
    }

    public Task Publish(object message, PublishOptions options)
    {
        options.GetExtensions().Set(transportTransaction);
        return sendOnlyEndpoint.Publish(message, options);
    }

    public Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions)
    {
        publishOptions.GetExtensions().Set(transportTransaction);
        return sendOnlyEndpoint.Publish(messageConstructor, publishOptions);
    }

    public Task Subscribe(Type eventType, SubscribeOptions options)
    {
        throw new NotSupportedException("Subscribing is not possible via a connector.");
    }

    public Task Unsubscribe(Type eventType, UnsubscribeOptions options)
    {
        throw new NotSupportedException("Subscribing is not possible via a connector.");
    }
}
