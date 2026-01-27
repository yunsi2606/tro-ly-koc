using TroLiKOC.Modules.Jobs.Domain.Enums;

namespace TroLiKOC.Modules.Jobs.Contracts;

public record RenderJobDto(
    Guid Id,
    Guid UserId,
    JobType JobType,
    JobStatus Status,
    string Priority,
    string? OutputUrl,
    string? ErrorMessage,
    int? ProcessingTimeMs,
    DateTime CreatedAt,
    DateTime? QueuedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt);

public record CreateJobRequest(
    JobType JobType,
    string Priority,
    string? SourceImageUrl,
    string? AudioUrl,
    string? GarmentImageUrl,
    string? SkeletonVideoUrl,
    string? TargetFaceUrl,
    string? OutputResolution
)
{
    // Helper to serialize inputs back to JSON for internal storage
    public string InputPayload => System.Text.Json.JsonSerializer.Serialize(new
    {
        SourceImageUrl,
        AudioUrl,
        GarmentImageUrl,
        SkeletonVideoUrl,
        TargetFaceUrl,
        OutputResolution
    });
}
