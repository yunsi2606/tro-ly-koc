using MediatR;
using Microsoft.EntityFrameworkCore;
using TroLiKOC.Modules.Payment.Domain.Entities;
using TroLiKOC.SharedKernel.Infrastructure;

namespace TroLiKOC.Modules.Payment.Infrastructure;

public class PaymentDbContext : BaseDbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options, IMediator mediator) : base(options, mediator)
    {
    }

    public DbSet<SePayWebhookLog> WebhookLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.HasDefaultSchema("payment");

        builder.Entity<SePayWebhookLog>(b =>
        {
            b.ToTable("SePayWebhookLogs");
            b.HasIndex(l => l.TransactionId);
            b.Property(l => l.Amount).HasColumnType("decimal(18,2)");
        });
    }
}
