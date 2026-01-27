using TroLiKOC.SharedKernel.Domain;

namespace TroLiKOC.Modules.Subscription.Domain.Entities;

public class Subscription : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid TierId { get; private set; } // Using Guid for TierId to match BaseEntity, plan said int but let's stick to Guid for consistency or change Tier to inherit BaseEntity which has Guid.
    // Wait, the plan SQL schema said TierId INT. 
    // But SubscriptionTier inherits BaseEntity which has Guid Id.
    // I should probably stick to Guid for everything in .NET world unless specific reason.
    // I'll use Guid for TierId.

    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool AutoRenew { get; private set; } = true;
    public string Status { get; private set; } = "ACTIVE"; // ACTIVE, EXPIRED, CANCELLED
    public int JobsUsedThisMonth { get; private set; } = 0;
    public DateTime? LastRenewalDate { get; private set; }

    public Subscription(Guid userId, Guid tierId, DateTime startDate, DateTime endDate)
    {
        UserId = userId;
        TierId = tierId;
        StartDate = startDate;
        EndDate = endDate;
        LastRenewalDate = startDate;
    }

    public void Renew(DateTime newEndDate)
    {
        EndDate = newEndDate;
        LastRenewalDate = DateTime.UtcNow;
        JobsUsedThisMonth = 0;
        Status = "ACTIVE";
        UpdateTimestamp();
    }
    
    public void Cancel()
    {
        AutoRenew = false;
        UpdateTimestamp();
    }

    public void Expire()
    {
        Status = "EXPIRED";
        UpdateTimestamp();
    }

    // EF Core
    private Subscription() { }
}
