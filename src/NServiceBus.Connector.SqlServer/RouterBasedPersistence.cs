using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Persistence;

class RouterBasedPersistence : PersistenceDefinition
{
    internal RouterBasedPersistence()
    {
        Supports<StorageType.Subscriptions>(s => s.EnableFeatureByDefault<RouterSubscriptionPersistence>());
    }
}