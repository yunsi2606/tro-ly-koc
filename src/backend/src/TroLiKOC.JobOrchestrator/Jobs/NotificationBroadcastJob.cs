using Quartz;
using Microsoft.Extensions.Logging;

namespace TroLiKOC.JobOrchestrator.Jobs;

[DisallowConcurrentExecution]
public class NotificationBroadcastJob : IJob
{
    private readonly ILogger<NotificationBroadcastJob> _logger;

    public NotificationBroadcastJob(ILogger<NotificationBroadcastJob> logger)
    {
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        // _logger.LogInformation("Checking for notifications to broadcast...");
        // Logic to poll database for pending notifications and send via SignalR
        
        await Task.CompletedTask;
    }
}
