using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Connector.SqlServer;
using NServiceBus.Router;

class Connector : IConnector
{
    EndpointConfiguration endpointConfig;
    IRouter router;
    IEndpointInstance sendOnlyEndpoint;

    public Connector(RouterConfiguration routerConfig, EndpointConfiguration endpointConfig)
    {
        this.endpointConfig = endpointConfig;
        router = Router.Create(routerConfig);
    }

    public async Task Start()
    {
        if (sendOnlyEndpoint != null)
        {
            throw new InvalidOperationException("Cannot start a connector that has already been started.");
        }
        await router.Start().ConfigureAwait(false);
        sendOnlyEndpoint = await Endpoint.Start(endpointConfig).ConfigureAwait(false);
    }

    public IMessageSession GetSession(SqlConnection connection, SqlTransaction transaction)
    {
        if (sendOnlyEndpoint == null)
        {
            throw new InvalidOperationException("Cannot create a session before starting the connector.");
        }

        return new ConnectorMessageSession(sendOnlyEndpoint, connection, transaction);
    }

    public async Task Stop()
    {
        if (sendOnlyEndpoint == null)
        {
            throw new InvalidOperationException("Cannot stop a connector that has not been started yet.");
        }
        await sendOnlyEndpoint.Stop().ConfigureAwait(false);
        await router.Stop().ConfigureAwait(false);
    }
}