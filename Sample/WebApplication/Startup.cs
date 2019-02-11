using System.Data.SqlClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NServiceBus;
using NServiceBus.Connector.SqlServer;
using NServiceBus.Router;

public class Startup
{
    const string ConnectionString = "data source=(local); initial catalog=connector; integrated security=SSPI";

    public Startup(IHostingEnvironment env)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddEnvironmentVariables();
        Configuration = builder.Build();
    }

    public IConfigurationRoot Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        #region ConnectorConfiguration
        
        var storage = new SqlSubscriptionStorage(() => new SqlConnection(ConnectionString), "WebApplication", new SqlDialect.MsSqlServer(), null);
        storage.Install().GetAwaiter().GetResult();

        var connectorConfig = new ConnectorConfiguration<MsmqTransport>(
            name: "WebApplication",
            sqlConnectionString: ConnectionString,
            customizeConnectedTransport: extensions =>
            {

            }, 
            customizeConnectedInterface: configuration =>
            {
                //Required because connected transport (MSMQ) does not support pub/sub
                configuration.EnableMessageDrivenPublishSubscribe(storage);
            });

        connectorConfig.AutoCreateQueues();

        #endregion

        #region Routing

        connectorConfig.RouteToEndpoint(
            messageType: typeof(MyMessage),
            endpointName: "Samples.ASPNETCore.Endpoint");

        #endregion

        #region ConnectorStart

        connector = connectorConfig.CreateConnector();
        connector.Start().GetAwaiter().GetResult();

        #endregion

        #region ServiceRegistration

        services.UseSqlServer(ConnectionString);
        services.UseOneTransactionPerHttpCall();
        services.UseNServiceBusConnector(connector);

        #endregion

        services.AddMvc();
        services.AddLogging(loggingBuilder => loggingBuilder.AddDebug());
    }


    public void Configure(IApplicationBuilder app, IApplicationLifetime applicationLifetime)
    {
        applicationLifetime.ApplicationStopping.Register(OnShutdown);

        app.UseMvc(routeBuilder => routeBuilder.MapRoute(name: "default",
            template: "{controller=SendMessage}/{action=Get}"));
    }

    void OnShutdown()
    {
        connector?.Stop().GetAwaiter().GetResult();
    }

    IConnector connector;
}