using MediatR;
using Microsoft.EntityFrameworkCore;
using TroLiKOC.Modules.Wallet.Domain.Entities;
using TroLiKOC.SharedKernel.Infrastructure;

namespace TroLiKOC.Modules.Wallet.Infrastructure;

public class WalletDbContext : BaseDbContext
{
    public WalletDbContext(DbContextOptions<WalletDbContext> options, IMediator mediator) : base(options, mediator)
    {
    }

    public DbSet<Domain.Entities.Wallet> Wallets { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.HasDefaultSchema("wallet");

        builder.Entity<Domain.Entities.Wallet>(b =>
        {
            b.ToTable("Wallets");
            b.HasKey(w => w.Id);
            b.HasIndex(w => w.UserId).IsUnique();
            b.Property(w => w.Balance).HasColumnType("decimal(18,2)");
        });

        builder.Entity<Transaction>(b =>
        {
            b.ToTable("Transactions");
            b.HasKey(t => t.Id);
            b.HasIndex(t => t.WalletId);
            b.Property(t => t.Amount).HasColumnType("decimal(18,2)");
            b.Property(t => t.BalanceBefore).HasColumnType("decimal(18,2)");
            b.Property(t => t.BalanceAfter).HasColumnType("decimal(18,2)");
        });
    }
}
