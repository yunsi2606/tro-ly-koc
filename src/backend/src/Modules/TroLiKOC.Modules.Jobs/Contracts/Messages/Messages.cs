namespace TroLiKOC.Modules.Jobs.Contracts.Messages;

public record TalkingHeadRequest
{
    public Guid JobId { get; init; }
    public Guid UserId { get; init; }
    public string SourceImageUrl { get; init; } = default!;
    public string AudioUrl { get; init; } = default!;
    public string Priority { get; init; } = "normal";
    public string OutputResolution { get; init; } = "720p";
    public bool AddWatermark { get; init; } = true;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

public record VirtualTryOnRequest
{
    public Guid JobId { get; init; }
    public Guid UserId { get; init; }
    public string ModelImageUrl { get; init; } = default!;
    public string GarmentImageUrl { get; init; } = default!;
    public string Priority { get; init; } = "normal";
    public string OutputResolution { get; init; } = "720p";
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

public record ImageToVideoRequest
{
    public Guid JobId { get; init; }
    public Guid UserId { get; init; }
    public string SourceImageUrl { get; init; } = default!;
    public string Priority { get; init; } = "normal";
    public string OutputResolution { get; init; } = "720p";
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

public record MotionTransferRequest
{
    public Guid JobId { get; init; }
    public Guid UserId { get; init; }
    public string SourceImageUrl { get; init; } = default!;
    public string SkeletonVideoUrl { get; init; } = default!;
    public string Priority { get; init; } = "normal";
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

public record FaceSwapRequest
{
    public Guid JobId { get; init; }
    public Guid UserId { get; init; }
    public string SourceVideoUrl { get; init; } = default!;
    public string TargetFaceUrl { get; init; } = default!;
    public string Priority { get; init; } = "normal";
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

public record JobCompletedEvent
{
    public Guid JobId { get; init; }
    public string Status { get; init; } = default!;
    public string? OutputUrl { get; init; }
    public string? Error { get; init; }
    public int ProcessingTimeMs { get; init; }
    public DateTime CompletedAt { get; init; }
}
