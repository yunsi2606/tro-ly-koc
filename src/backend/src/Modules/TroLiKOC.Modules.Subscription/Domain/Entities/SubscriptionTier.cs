using TroLiKOC.SharedKernel.Domain;

namespace TroLiKOC.Modules.Subscription.Domain.Entities;

public class SubscriptionTier : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; }
    public decimal MonthlyPrice { get; private set; }
    public int MaxJobsPerMonth { get; private set; }
    public string MaxResolution { get; private set; } // 720p, 1080p, 4K
    public bool HasWatermark { get; private set; }
    public string QueuePriority { get; private set; } // low, normal, high, realtime
    public bool SupportsLoRA { get; private set; }
    public bool SupportsVoiceCloning { get; private set; }
    public bool IsActive { get; private set; } = true;

    internal SubscriptionTier(string name, decimal monthlyPrice, int maxJobsPerMonth, string maxResolution, bool hasWatermark, string queuePriority, bool supportsLoRA, bool supportsVoiceCloning)
    {
        Name = name;
        MonthlyPrice = monthlyPrice;
        MaxJobsPerMonth = maxJobsPerMonth;
        MaxResolution = maxResolution;
        HasWatermark = hasWatermark;
        QueuePriority = queuePriority;
        SupportsLoRA = supportsLoRA;
        SupportsVoiceCloning = supportsVoiceCloning;
    }

    // EF Core
    private SubscriptionTier() { }
}
