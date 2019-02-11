using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

public class MyEventHandler :
    IHandleMessages<MyEvent>
{
    static ILog log = LogManager.GetLogger<MyEventHandler>();

    #region EventHandler
    public Task Handle(MyEvent message, IMessageHandlerContext context)
    {
        log.Info("MyEvent received at endpoint");
        return Task.CompletedTask;
    }
    #endregion
}