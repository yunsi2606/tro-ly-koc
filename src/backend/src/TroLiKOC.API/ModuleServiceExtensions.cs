using TroLiKOC.Modules.Identity;
using TroLiKOC.Modules.Wallet;
using TroLiKOC.Modules.Subscription;
using TroLiKOC.Modules.Jobs;
using TroLiKOC.Modules.Payment;

namespace TroLiKOC.API;

public static class ModuleServiceExtensions
{
    public static IServiceCollection AddModularMonolith(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddIdentityModule(configuration);
        services.AddWalletModule(configuration);
        services.AddSubscriptionModule(configuration);
        services.AddJobsModule(configuration);
        services.AddPaymentModule(configuration);

        return services;
    }
}
