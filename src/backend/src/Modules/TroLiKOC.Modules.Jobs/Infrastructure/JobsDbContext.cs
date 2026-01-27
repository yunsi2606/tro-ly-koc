using MediatR;
using Microsoft.EntityFrameworkCore;
using TroLiKOC.Modules.Jobs.Domain.Entities;
using TroLiKOC.SharedKernel.Infrastructure;

namespace TroLiKOC.Modules.Jobs.Infrastructure;

public class JobsDbContext : BaseDbContext
{
    public JobsDbContext(DbContextOptions<JobsDbContext> options, IMediator mediator) : base(options, mediator)
    {
    }

    public DbSet<RenderJob> RenderJobs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.HasDefaultSchema("jobs");

        builder.Entity<RenderJob>(b =>
        {
            b.ToTable("RenderJobs");
            b.HasIndex(j => j.UserId);
            b.HasIndex(j => j.Status);
            b.Property(j => j.JobType).HasConversion<string>();
            b.Property(j => j.Status).HasConversion<string>();
        });
    }
}
