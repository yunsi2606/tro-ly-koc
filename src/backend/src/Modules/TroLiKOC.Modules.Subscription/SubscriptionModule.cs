using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TroLiKOC.Modules.Subscription.Application.Services;
using TroLiKOC.Modules.Subscription.Contracts;
using TroLiKOC.Modules.Subscription.Infrastructure;

namespace TroLiKOC.Modules.Subscription;

public static class SubscriptionModule
{
    public static IServiceCollection AddSubscriptionModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<SubscriptionDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "subscription")));

        services.AddScoped<ISubscriptionModule, SubscriptionService>();

        return services;
    }
}
