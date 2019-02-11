using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

public class MyMessageHandler :
    IHandleMessages<MyMessage>
{
    static ILog log = LogManager.GetLogger<MyMessageHandler>();

    #region MessageHandler
    public Task Handle(MyMessage message, IMessageHandlerContext context)
    {
        log.Info("MyMessage received at endpoint");
        return Task.CompletedTask;
    }
    #endregion
}