using Microsoft.AspNetCore.SignalR;
using TroLiKOC.API.Hubs;
using TroLiKOC.Modules.Jobs.Contracts;
using TroLiKOC.Modules.Jobs.Contracts.Messages;

namespace TroLiKOC.API.Services;

public class SignalRJobNotifier : IJobNotifier
{
    private readonly IHubContext<JobHub> _hubContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SignalRJobNotifier> _logger;

    public SignalRJobNotifier(
        IHubContext<JobHub> hubContext,
        IConfiguration configuration,
        ILogger<SignalRJobNotifier> logger)
    {
        _hubContext = hubContext;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task NotifyJobCompletedAsync(Guid userId, JobCompletedEvent jobResult)
    {
        // Fix Internal MinIO URL
        // If the URL contains internal hostname "minio:9000", allow replacing it with public URL
        var outputUrl = jobResult.OutputUrl;
        
        var publicStorageUrl = _configuration["MinIO:PublicEndpoint"]; 
        
        if (!string.IsNullOrEmpty(outputUrl) && !string.IsNullOrEmpty(publicStorageUrl))
        {
            // Simple string replacement for internal minio
            if (outputUrl.Contains("minio:9000"))
            {
                outputUrl = outputUrl.Replace("http://minio:9000", publicStorageUrl);
            }
        }
        
        await _hubContext.Clients.Group(userId.ToString()).SendAsync("JobCompleted", new
        {
            jobResult.JobId,
            jobResult.Status,
            OutputUrl = outputUrl,
            jobResult.Error,
            jobResult.ProcessingTimeMs,
            jobResult.CompletedAt
        });
        
        _logger.LogInformation("Sent JobCompleted signal to User {UserId} with URL {Url}", userId, outputUrl);
    }

    public async Task NotifyJobFailedAsync(Guid userId, Guid jobId, string error)
    {
        await _hubContext.Clients.Group(userId.ToString()).SendAsync("JobFailed", new
        {
            JobId = jobId,
            Status = "FAILED",
            Error = error
        });
    }
}
