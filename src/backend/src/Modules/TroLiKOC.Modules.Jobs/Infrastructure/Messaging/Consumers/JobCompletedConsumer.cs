using MassTransit;
using Microsoft.Extensions.Logging;
using TroLiKOC.Modules.Jobs.Contracts;
using TroLiKOC.Modules.Jobs.Contracts.Messages;

namespace TroLiKOC.Modules.Jobs.Infrastructure.Messaging.Consumers;

public class JobCompletedConsumer : IConsumer<JobCompletedEvent>
{
    private readonly IJobsModule _jobsModule;
    private readonly ILogger<JobCompletedConsumer> _logger;

    public JobCompletedConsumer(IJobsModule jobsModule, ILogger<JobCompletedConsumer> logger)
    {
        _jobsModule = jobsModule;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<JobCompletedEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "Nhận kết quả xử lý Job {JobId}: Trạng thái={Status}, Thời gian={TimeMs}ms",
            message.JobId, message.Status, message.ProcessingTimeMs);

        if (message.Status == "COMPLETED" && message.OutputUrl != null)
        {
            await _jobsModule.CompleteJobAsync(
                message.JobId,
                message.OutputUrl,
                message.OutputUrl, // OutputKey would normally be extracted from URL
                message.ProcessingTimeMs);

            _logger.LogInformation("Đã hoàn thành Job {JobId}", message.JobId);
        }
        else if (message.Status == "FAILED")
        {
            await _jobsModule.FailJobAsync(message.JobId, message.Error ?? "Lỗi không xác định");
            
            _logger.LogWarning("Job {JobId} thất bại: {Error}", message.JobId, message.Error);
        }
    }
}
