using Microsoft.EntityFrameworkCore;
using TroLiKOC.Modules.Subscription.Contracts;
using TroLiKOC.Modules.Subscription.Infrastructure;

namespace TroLiKOC.Modules.Subscription.Application.Services;

public class SubscriptionService : ISubscriptionModule
{
    private readonly SubscriptionDbContext _dbContext;

    public SubscriptionService(SubscriptionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SubscriptionTierDto?> GetTierAsync(Guid tierId)
    {
        var tier = await _dbContext.SubscriptionTiers.FindAsync(tierId);
        if (tier == null) return null;

        return new SubscriptionTierDto(
            tier.Id, tier.Name, tier.MonthlyPrice, tier.MaxJobsPerMonth, tier.MaxResolution, 
            tier.HasWatermark, tier.QueuePriority, tier.SupportsLoRA, tier.SupportsVoiceCloning, tier.IsActive);
    }

    public async Task<SubscriptionDto?> GetActiveSubscriptionAsync(Guid userId)
    {
        var sub = await _dbContext.Subscriptions
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "ACTIVE");

        if (sub == null) return null;

        return ToDto(sub);
    }

    public async Task<List<SubscriptionDto>> GetExpiringSubscriptionsAsync(DateTime date, bool autoRenewOnly)
    {
        // Get subscriptions expiring exactly on this date (or before, if we want to catch missed ones, but daily check implies Date match)
        // Let's check for Expiry Date <= date AND Status == ACTIVE
        // Ideally, renewal happens slightly before expiry or on the day.
        
        var query = _dbContext.Subscriptions
            .Where(s => s.EndDate.Date <= date.Date && s.Status == "ACTIVE");

        if (autoRenewOnly)
        {
            query = query.Where(s => s.AutoRenew);
        }

        var subs = await query.ToListAsync();
        return subs.Select(ToDto).ToList();
    }

    public async Task ExtendAsync(Guid subscriptionId, TimeSpan duration)
    {
        var sub = await _dbContext.Subscriptions.FindAsync(subscriptionId);
        if (sub == null) return;

        sub.Renew(sub.EndDate.Add(duration));
        await _dbContext.SaveChangesAsync();
    }

    public async Task MarkInsufficientBalanceAsync(Guid subscriptionId)
    {
        // Maybe we don't expire it immediately, but for now let's just log or maybe have a "GracePeriod" status?
        // Plan says "mark for expiry". I implemented 'Expire' method but that sets status to EXPIRED.
        // Let's assume we expire it if payment fails.
        var sub = await _dbContext.Subscriptions.FindAsync(subscriptionId);
        if (sub != null)
        {
            sub.Expire();
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task InitializeSubscriptionAsync(Guid userId, Guid tierId)
    {
        // Cancel existing active subscription
        var existing = await _dbContext.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "ACTIVE");
        
        if (existing != null)
        {
            existing.Cancel();
        }

        var subscription = new Domain.Entities.Subscription(
            userId, 
            tierId, 
            DateTime.UtcNow, 
            DateTime.UtcNow.AddDays(30));

        _dbContext.Subscriptions.Add(subscription);
        await _dbContext.SaveChangesAsync();
    }

    public async Task CancelSubscriptionAsync(Guid userId)
    {
        var sub = await _dbContext.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "ACTIVE");
        
        if (sub != null)
        {
            sub.Cancel();
            await _dbContext.SaveChangesAsync();
        }
    }

    private static SubscriptionDto ToDto(Domain.Entities.Subscription sub)
    {
        return new SubscriptionDto(
            sub.Id, sub.UserId, sub.TierId, sub.StartDate, sub.EndDate, sub.AutoRenew, 
            sub.Status, sub.JobsUsedThisMonth, sub.LastRenewalDate);
    }
}
