using System.Collections.Generic;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Extensibility;
using NServiceBus.Features;
using NServiceBus.Routing;
using NServiceBus.Transport;
using NServiceBus.Unicast.Subscriptions;
using NServiceBus.Unicast.Subscriptions.MessageDrivenSubscriptions;

class RouterSubscriptionPersistence : Feature
{
    internal RouterSubscriptionPersistence()
    {
        DependsOn<MessageDrivenSubscriptions>();
    }

    /// <summary>
    /// See <see cref="Feature.Setup" />.
    /// </summary>
    protected override void Setup(FeatureConfigurationContext context)
    {
        //Router has same endpoint name as send only endpoint
        var routerEndpoint = context.Settings.EndpointName();

        var routerAddress = context.Settings.GetTransportAddress(LogicalAddress.CreateRemoteAddress(new EndpointInstance(routerEndpoint)));
        context.Container.ConfigureComponent<ISubscriptionStorage>(b => new SubscriptionStorage(routerAddress, routerEndpoint), DependencyLifecycle.SingleInstance);
    }

    class SubscriptionStorage : ISubscriptionStorage
    {
        IEnumerable<Subscriber> subscribers;

        public SubscriptionStorage(string routerAddress, string routerEndpoint)
        {
            subscribers = new[] {new Subscriber(routerAddress, routerEndpoint) };
        }

        public Task Subscribe(Subscriber subscriber, MessageType messageType, ContextBag context)
        {
            return Task.CompletedTask;
        }

        public Task Unsubscribe(Subscriber subscriber, MessageType messageType, ContextBag context)
        {
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Subscriber>> GetSubscriberAddressesForMessage(IEnumerable<MessageType> messageTypes, ContextBag context)
        {
            return Task.FromResult(subscribers);
        }
    }
}