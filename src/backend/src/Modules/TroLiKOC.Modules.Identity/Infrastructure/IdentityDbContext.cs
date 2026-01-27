using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TroLiKOC.Modules.Identity.Domain.Entities;

namespace TroLiKOC.Modules.Identity.Infrastructure;

public class IdentityDbContext : IdentityDbContext<User, Microsoft.AspNetCore.Identity.IdentityRole<Guid>, Guid>
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Isolate Identity module tables in 'identity' schema
        builder.HasDefaultSchema("identity");

        builder.Entity<User>(b =>
        {
            b.ToTable("Users");
            b.Property(u => u.DisplayName).HasMaxLength(100).IsRequired();
            b.Property(u => u.IsActive).HasDefaultValue(true);
        });
    }
}
