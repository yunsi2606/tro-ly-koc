using Quartz;
using Microsoft.Extensions.Logging;
using TroLiKOC.Modules.Subscription.Contracts;

namespace TroLiKOC.JobOrchestrator.Jobs;

[DisallowConcurrentExecution]
public class SubscriptionExpiryJob : IJob
{
    private readonly ISubscriptionModule _subscriptionModule;
    private readonly ILogger<SubscriptionExpiryJob> _logger;

    public SubscriptionExpiryJob(
        ISubscriptionModule subscriptionModule,
        ILogger<SubscriptionExpiryJob> logger)
    {
        _subscriptionModule = subscriptionModule;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting subscription expiry cleanup at {Time}", DateTime.UtcNow);
        
        // This job could be used to send "Your subscription has expired" emails
        // or ensure status is correctly updated if renewal job missed it.
        // For now, it's just a placeholder for logic that runs AFTER renewal attempts.
        
        // Example: Get expired subscriptions that are still marked ACTIVE but define strict expiry
        // Logic implemented in Service usually.
        
        _logger.LogInformation("Completed subscription expiry cleanup");
        await Task.CompletedTask;
    }
}
