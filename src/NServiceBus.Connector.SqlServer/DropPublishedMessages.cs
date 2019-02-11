using System.Threading.Tasks;
using NServiceBus.Router;

class DropPublishedMessages : ChainTerminator<PublishPreroutingContext>
{
    protected override Task<bool> Terminate(PublishPreroutingContext context)
    {
        return Task.FromResult(true);
    }
}