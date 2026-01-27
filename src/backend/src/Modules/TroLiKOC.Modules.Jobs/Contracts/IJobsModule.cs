using TroLiKOC.Modules.Jobs.Domain.Enums;

namespace TroLiKOC.Modules.Jobs.Contracts;

public interface IJobsModule
{
    Task<RenderJobDto> CreateJobAsync(Guid userId, JobType jobType, string priority, string inputPayload);
    Task<RenderJobDto?> GetJobAsync(Guid jobId);
    Task<IReadOnlyList<RenderJobDto>> GetUserJobsAsync(Guid userId, int page, int size);
    Task<IReadOnlyList<RenderJobDto>> GetJobsByStatusAsync(JobStatus status, int limit);
    Task MarkJobQueuedAsync(Guid jobId);
    Task MarkJobStartedAsync(Guid jobId);
    Task CompleteJobAsync(Guid jobId, string outputUrl, string outputKey, int processingTimeMs);
    Task FailJobAsync(Guid jobId, string error);
}
