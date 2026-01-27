using TroLiKOC.Modules.Jobs.Domain.Enums;
using TroLiKOC.SharedKernel.Domain;

namespace TroLiKOC.Modules.Jobs.Domain.Entities;

public class RenderJob : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    public JobType JobType { get; private set; }
    public JobStatus Status { get; private set; } = JobStatus.Pending;
    public string Priority { get; private set; } = "normal";
    public string InputPayload { get; private set; } // JSON
    public string? OutputUrl { get; private set; }
    public string? OutputKey { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int? ProcessingTimeMs { get; private set; }
    public DateTime? QueuedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    internal RenderJob(Guid userId, JobType jobType, string priority, string inputPayload)
    {
        UserId = userId;
        JobType = jobType;
        Priority = priority;
        InputPayload = inputPayload;
    }

    public void MarkQueued()
    {
        Status = JobStatus.Queued;
        QueuedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void MarkStarted()
    {
        Status = JobStatus.Processing;
        StartedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void Complete(string outputUrl, string outputKey, int processingTimeMs)
    {
        Status = JobStatus.Completed;
        OutputUrl = outputUrl;
        OutputKey = outputKey;
        ProcessingTimeMs = processingTimeMs;
        CompletedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void Fail(string error)
    {
        Status = JobStatus.Failed;
        ErrorMessage = error;
        CompletedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    // EF Core
    private RenderJob() { }
}
