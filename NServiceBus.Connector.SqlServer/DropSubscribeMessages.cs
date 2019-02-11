using System.Threading.Tasks;
using NServiceBus.Router;

class DropSubscribeMessages : ChainTerminator<SubscribePreroutingContext>
{
    protected override Task<bool> Terminate(SubscribePreroutingContext context)
    {
        return Task.FromResult(true);
    }
}