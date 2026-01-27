using MediatR;
using Microsoft.EntityFrameworkCore;
using TroLiKOC.Modules.Subscription.Domain.Entities;
using TroLiKOC.SharedKernel.Infrastructure;

namespace TroLiKOC.Modules.Subscription.Infrastructure;

public class SubscriptionDbContext : BaseDbContext
{
    public SubscriptionDbContext(DbContextOptions<SubscriptionDbContext> options, IMediator mediator) : base(options, mediator)
    {
    }

    public DbSet<SubscriptionTier> SubscriptionTiers { get; set; }
    public DbSet<Domain.Entities.Subscription> Subscriptions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.HasDefaultSchema("subscription");

        builder.Entity<SubscriptionTier>(b =>
        {
            b.ToTable("SubscriptionTiers");
            b.Property(t => t.MonthlyPrice).HasColumnType("decimal(18,2)");
            // Seed data
            b.HasData(
                new { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Cơ Bản", MonthlyPrice = 199000m, MaxJobsPerMonth = 50, MaxResolution = "720p", HasWatermark = true, QueuePriority = "low", SupportsLoRA = false, SupportsVoiceCloning = false, IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Sáng Tạo Nội Dung", MonthlyPrice = 499000m, MaxJobsPerMonth = 200, MaxResolution = "1080p", HasWatermark = false, QueuePriority = "high", SupportsLoRA = false, SupportsVoiceCloning = false, IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Đại Lý", MonthlyPrice = 1499000m, MaxJobsPerMonth = -1, MaxResolution = "4K", HasWatermark = false, QueuePriority = "realtime", SupportsLoRA = true, SupportsVoiceCloning = true, IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        });

        builder.Entity<Domain.Entities.Subscription>(b =>
        {
            b.ToTable("Subscriptions");
            b.HasIndex(s => s.UserId);
        });
    }
}
