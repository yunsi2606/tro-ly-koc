using Quartz;
using Microsoft.Extensions.Logging;
using TroLiKOC.Modules.Subscription.Contracts;
using TroLiKOC.Modules.Wallet.Contracts;

namespace TroLiKOC.JobOrchestrator.Jobs;

[DisallowConcurrentExecution]
public class SubscriptionRenewalJob : IJob
{
    private readonly ISubscriptionModule _subscriptionModule;
    private readonly IWalletModule _walletModule;
    private readonly ILogger<SubscriptionRenewalJob> _logger;

    public SubscriptionRenewalJob(
        ISubscriptionModule subscriptionModule,
        IWalletModule walletModule,
        ILogger<SubscriptionRenewalJob> logger)
    {
        _subscriptionModule = subscriptionModule;
        _walletModule = walletModule;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting subscription renewal check at {Time}", DateTime.UtcNow);
        
        // Get subscriptions expiring today with auto-renew enabled
        var expiringSubscriptions = await _subscriptionModule
            .GetExpiringSubscriptionsAsync(DateTime.UtcNow.Date, autoRenewOnly: true);

        foreach (var subscription in expiringSubscriptions)
        {
            try
            {
                var tier = await _subscriptionModule.GetTierAsync(subscription.TierId);
                if (tier == null) continue;

                var wallet = await _walletModule.GetByUserIdAsync(subscription.UserId);
                if (wallet == null) continue;

                if (wallet.Balance >= tier.MonthlyPrice)
                {
                    // Deduct from wallet
                    await _walletModule.DeductAsync(
                        subscription.UserId,
                        tier.MonthlyPrice,
                        $"Auto-renewal: {tier.Name}");

                    // Extend subscription
                    await _subscriptionModule.ExtendAsync(
                        subscription.Id,
                        TimeSpan.FromDays(30));

                    _logger.LogInformation(
                        "Renewed subscription {SubId} for user {UserId}",
                        subscription.Id, subscription.UserId);
                }
                else
                {
                    // Insufficient balance - mark for expiry
                    await _subscriptionModule.MarkInsufficientBalanceAsync(subscription.Id);
                    
                    _logger.LogWarning(
                        "Insufficient balance for renewal: User {UserId}, Required {Amount}",
                        subscription.UserId, tier.MonthlyPrice);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Failed to process renewal for subscription {SubId}", 
                    subscription.Id);
            }
        }

        _logger.LogInformation(
            "Completed renewal check. Processed {Count} subscriptions",
            expiringSubscriptions.Count);
    }
}
