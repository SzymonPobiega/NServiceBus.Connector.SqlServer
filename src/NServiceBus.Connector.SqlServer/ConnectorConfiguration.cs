using System;
using NServiceBus.Router;
using NServiceBus.Transport;

namespace NServiceBus.Connector.SqlServer
{
    /// <summary>
    /// Configures the NServiceBus SQL Server connector that allows sending messages through SQL Server transport to
    /// a different connected transport.
    /// </summary>
    /// <typeparam name="T">The connected transport type.</typeparam>
    public class ConnectorConfiguration<T>
        where T : TransportDefinition, new()
    {
        RouterConfiguration routerConfig;
        EndpointConfiguration endpointConfig;
        RouterConnectionSettings routingSettings;

        /// <summary>
        /// Creates a new instance of connector configuration.
        /// </summary>
        /// <param name="name">The name of the connector.</param>
        /// <param name="sqlConnectionString">The connection string to SQL Server instance.</param>
        /// <param name="customizeConnectedTransport">The callback to customize the connected transport.</param>
        /// <param name="customizeConnectedInterface">The callback to customize the connected transport interface.</param>
        public ConnectorConfiguration(string name, string sqlConnectionString, 
            Action<TransportExtensions<T>> customizeConnectedTransport,
            Action<InterfaceConfiguration<T>> customizeConnectedInterface = null)
        {
            routerConfig = new RouterConfiguration(name);
            var sqlInterface = routerConfig.AddInterface<SqlServerTransport>("SQL", t =>
            {
                t.Transactions(TransportTransactionMode.SendsAtomicWithReceive);
                t.ConnectionString(sqlConnectionString);
            });

            //We don't forward the publishes as the connector is by nature send-only
            sqlInterface.AddRule(c => new NullForwardPublishRule());

            //We don't forward the subscribe and unsubscribe because the connector is hard-coded to forward all publishes to the routing endpoint

            var externalInterface = routerConfig.AddInterface("External", customizeConnectedTransport);
            customizeConnectedInterface?.Invoke(externalInterface);

            externalInterface.AddRule(c => new DropPublishedMessages());
            externalInterface.AddRule(c => new DropSubscribeMessages());
            externalInterface.AddRule(c => new DropUnsubscribeMessages());

            var staticRouting = routerConfig.UseStaticRoutingProtocol();
            staticRouting.AddForwardRoute("SQL", "External");

            endpointConfig = new EndpointConfiguration(name);
            endpointConfig.SendOnly();
            endpointConfig.UsePersistence<RouterBasedPersistence>();

            var endpointTransport = endpointConfig.UseTransport<SqlServerTransport>();
            endpointTransport.ConnectionString(sqlConnectionString);
            routingSettings = endpointTransport.Routing().ConnectToRouter(name);
        }

        /// <summary>
        /// Instructs the connector to route messages of this type to a designated endpoint.
        /// </summary>
        /// <param name="messageType">Message type.</param>
        /// <param name="endpointName">Name of the destination endpoint.</param>
        public void RouteToEndpoint(Type messageType, string endpointName)
        {
            routingSettings.RouteToEndpoint(messageType, endpointName);
        }
        
        /// <summary>
        /// Configures the connector to automatically create the queues in SQL Server and external transport when starting up.
        /// </summary>
        /// <param name="identity">Identity to use when creating the queue.</param>
        public void AutoCreateQueues(string identity = null)
        {
            routerConfig.AutoCreateQueues(identity);
        }

        /// <summary>
        /// Defines the message conventions to use for this connector.
        /// </summary>
        public ConventionsBuilder Conventions()
        {
            return endpointConfig.Conventions();
        }

        /// <summary>
        /// Creates an instance of the SQL connector.
        /// </summary>
        public IConnector CreateConnector()
        {
            return new global::Connector(routerConfig, endpointConfig);
        }
    }
}