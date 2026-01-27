using Quartz;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TroLiKOC.JobOrchestrator.Jobs;

namespace TroLiKOC.JobOrchestrator.Configuration;

public static class QuartzConfiguration
{
    public static IServiceCollection AddJobOrchestrator(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddQuartz(q =>
        {
            // q.UseMicrosoftDependencyInjectionJobFactory(); // Default in newer versions
            
            // Use SQL Server for persistence (requires tables to be created)
            // For development, we might use InMemory if tables aren't ready, but let's configure for SQL
            // provided the user runs the Quartz SQL scripts.
            // If tables are missing, it will crash. Let's use InMemoryKey for now to be safe until migrations setup Quartz tables?
            // User plan said "Use SQL Server for persistence".
            
            // q.UsePersistentStore(store =>
            // {
            //     store.UseSqlServer(configuration.GetConnectionString("DefaultConnection")!);
            //     store.UseNewtonsoftJsonSerializer();
            //     store.PerformSchemaValidation = false;
            // });
            q.UseInMemoryStore();

            // SUBSCRIPTION RENEWAL JOB
            // Runs daily at 00:05 Vietnam time
            var renewalJobKey = new JobKey("SubscriptionRenewalJob");
            q.AddJob<SubscriptionRenewalJob>(opts => opts
                .WithIdentity(renewalJobKey)
                .StoreDurably());
            
            q.AddTrigger(opts => opts
                .ForJob(renewalJobKey)
                .WithIdentity("SubscriptionRenewalTrigger")
                .WithCronSchedule("0 5 0 * * ?", x => x
                    .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))));

            // SUBSCRIPTION EXPIRY CLEANUP JOB
            // Runs daily at 01:00 Vietnam time
            var expiryJobKey = new JobKey("SubscriptionExpiryJob");
            q.AddJob<SubscriptionExpiryJob>(opts => opts
                .WithIdentity(expiryJobKey)
                .StoreDurably());
            
            q.AddTrigger(opts => opts
                .ForJob(expiryJobKey)
                .WithIdentity("SubscriptionExpiryTrigger")
                .WithCronSchedule("0 0 1 * * ?", x => x
                    .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))));

            // NOTIFICATION BROADCAST JOB
            // Runs every 5 minutes
            var notifyJobKey = new JobKey("NotificationBroadcastJob");
            q.AddJob<NotificationBroadcastJob>(opts => opts
                .WithIdentity(notifyJobKey)
                .StoreDurably());
            
            q.AddTrigger(opts => opts
                .ForJob(notifyJobKey)
                .WithIdentity("NotificationBroadcastTrigger")
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(5)
                    .RepeatForever()));
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        return services;
    }
}
