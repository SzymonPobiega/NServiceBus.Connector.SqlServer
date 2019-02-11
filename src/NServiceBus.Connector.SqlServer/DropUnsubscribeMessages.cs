using System.Threading.Tasks;
using NServiceBus.Router;

class DropUnsubscribeMessages : ChainTerminator<UnsubscribePreroutingContext>
{
    protected override Task<bool> Terminate(UnsubscribePreroutingContext context)
    {
        return Task.FromResult(true);
    }
}