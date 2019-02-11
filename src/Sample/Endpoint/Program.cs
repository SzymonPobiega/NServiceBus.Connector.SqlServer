using System;
using System.Threading.Tasks;
using NServiceBus;

static class Program
{
    static async Task Main()
    {
        Console.Title = "Samples.ASPNETCore.Endpoint";
        var endpointConfiguration = new EndpointConfiguration("Samples.ASPNETCore.Endpoint");
        endpointConfiguration.UsePersistence<InMemoryPersistence>();
        endpointConfiguration.EnableInstallers();
        endpointConfiguration.SendFailedMessagesTo("error");

        var transport = endpointConfiguration.UseTransport<MsmqTransport>();

        #region RouterConnection

        transport.Routing().RegisterPublisher(typeof(MyEvent), "WebApplication");
        #endregion

        var endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);
        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
        await endpointInstance.Stop()
            .ConfigureAwait(false);
    }
}