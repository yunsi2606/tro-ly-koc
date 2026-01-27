namespace TroLiKOC.Modules.Subscription.Contracts;

public record SubscriptionTierDto(
    Guid Id,
    string Name,
    decimal MonthlyPrice,
    int MaxJobsPerMonth,
    string MaxResolution,
    bool HasWatermark,
    string QueuePriority,
    bool SupportsLoRA,
    bool SupportsVoiceCloning,
    bool IsActive);

public record SubscriptionDto(
    Guid Id,
    Guid UserId,
    Guid TierId,
    DateTime StartDate,
    DateTime EndDate,
    bool AutoRenew,
    string Status,
    int JobsUsedThisMonth,
    DateTime? LastRenewalDate);
