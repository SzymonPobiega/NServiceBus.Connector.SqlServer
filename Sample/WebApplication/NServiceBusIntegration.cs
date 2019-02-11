using System.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus.Connector.SqlServer;

public static class NServiceBusIntegration
{
    public static void UseNServiceBusConnector(this IServiceCollection serviceCollection, IConnector connector)
    {
        serviceCollection.AddScoped(provider =>
            connector.GetSession(provider.GetService<SqlConnection>(), provider.GetService<SqlTransaction>()));
    }
}