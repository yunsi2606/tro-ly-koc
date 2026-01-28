using MassTransit;
using Microsoft.Extensions.Logging;
using TroLiKOC.Modules.Jobs.Contracts;
using TroLiKOC.Modules.Jobs.Contracts.Messages;

namespace TroLiKOC.Modules.Jobs.Infrastructure.Messaging.Consumers;

public class JobCompletedConsumer : IConsumer<JobCompletedEvent>
{
    private readonly IJobsModule _jobsModule;
    private readonly IJobNotifier _jobNotifier;
    private readonly ILogger<JobCompletedConsumer> _logger;

    public JobCompletedConsumer(
        IJobsModule jobsModule,
        IJobNotifier jobNotifier,
        ILogger<JobCompletedConsumer> logger)
    {
        _jobsModule = jobsModule;
        _jobNotifier = jobNotifier;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<JobCompletedEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "Nhận kết quả xử lý Job {JobId}: Trạng thái={Status}, Thời gian={TimeMs}ms",
            message.JobId, message.Status, message.ProcessingTimeMs);

        // We need to get the job to find the UserId to notify
        var job = await _jobsModule.GetJobAsync(message.JobId);
        if (job == null)
        {
            _logger.LogError("Không tìm thấy Job {JobId} trong database", message.JobId);
            return;
        }

        if (message.Status == "COMPLETED" && message.OutputUrl != null)
        {
            await _jobsModule.CompleteJobAsync(
                message.JobId,
                message.OutputUrl,
                message.OutputUrl, 
                message.ProcessingTimeMs);

            // Notify User via SignalR (through interface)
            await _jobNotifier.NotifyJobCompletedAsync(job.UserId, message);

            _logger.LogInformation("Đã hoàn thành Job {JobId} và thông báo tới User {UserId}", message.JobId, job.UserId);
        }
        else if (message.Status == "FAILED")
        {
            await _jobsModule.FailJobAsync(message.JobId, message.Error ?? "Lỗi không xác định");
            
            // Notify User
            await _jobNotifier.NotifyJobFailedAsync(job.UserId, message.JobId, message.Error ?? "Lỗi không xác định");

            _logger.LogWarning("Job {JobId} thất bại: {Error}", message.JobId, message.Error);
        }
    }
}
