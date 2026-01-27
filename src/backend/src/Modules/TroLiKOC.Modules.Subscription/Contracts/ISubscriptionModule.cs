namespace TroLiKOC.Modules.Subscription.Contracts;

public interface ISubscriptionModule
{
    Task<SubscriptionTierDto?> GetTierAsync(Guid tierId);
    Task<SubscriptionDto?> GetActiveSubscriptionAsync(Guid userId);
    Task<List<SubscriptionDto>> GetExpiringSubscriptionsAsync(DateTime date, bool autoRenewOnly);
    Task ExtendAsync(Guid subscriptionId, TimeSpan duration);
    Task MarkInsufficientBalanceAsync(Guid subscriptionId);
    Task InitializeSubscriptionAsync(Guid userId, Guid tierId);
    Task CancelSubscriptionAsync(Guid userId);
}
