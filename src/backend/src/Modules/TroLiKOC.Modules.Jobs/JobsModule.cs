using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TroLiKOC.Modules.Jobs.Contracts;
using TroLiKOC.Modules.Jobs.Infrastructure;
using TroLiKOC.Modules.Jobs.Infrastructure.Messaging.Publishers;

namespace TroLiKOC.Modules.Jobs;

public static class JobsModule
{
    public static IServiceCollection AddJobsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<JobsDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "jobs")));

        services.AddScoped<IJobsModule, Application.Services.RenderJobService>();
        services.AddScoped<IJobRequestPublisher, JobRequestPublisher>();

        return services;
    }
}
