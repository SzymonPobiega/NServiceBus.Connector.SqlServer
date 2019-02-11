using System;
using System.Threading.Tasks;
using NServiceBus.Router;

class NullForwardPublishRule : ChainTerminator<ForwardPublishContext>
{
    protected override Task<bool> Terminate(ForwardPublishContext context)
    {
        throw new Exception("Not supported");
    }
}