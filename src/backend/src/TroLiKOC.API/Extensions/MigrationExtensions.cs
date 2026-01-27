using Microsoft.EntityFrameworkCore;
using TroLiKOC.Modules.Identity.Infrastructure;
using TroLiKOC.Modules.Jobs.Infrastructure;
using TroLiKOC.Modules.Payment.Infrastructure;
using TroLiKOC.Modules.Subscription.Infrastructure;
using TroLiKOC.Modules.Wallet.Infrastructure;

namespace TroLiKOC.API.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Applying migrations for IdentityDbContext...");
            using var identityContext = services.GetRequiredService<IdentityDbContext>();
            identityContext.Database.Migrate();

            logger.LogInformation("Applying migrations for JobsDbContext...");
            using var jobsContext = services.GetRequiredService<JobsDbContext>();
            jobsContext.Database.Migrate();

            logger.LogInformation("Applying migrations for PaymentDbContext...");
            using var paymentContext = services.GetRequiredService<PaymentDbContext>();
            paymentContext.Database.Migrate();

            logger.LogInformation("Applying migrations for SubscriptionDbContext...");
            using var subscriptionContext = services.GetRequiredService<SubscriptionDbContext>();
            subscriptionContext.Database.Migrate();
            
            logger.LogInformation("Applying migrations for WalletDbContext...");
            using var walletContext = services.GetRequiredService<WalletDbContext>();
            walletContext.Database.Migrate();
            
            logger.LogInformation("All migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database.");
            throw; 
        }
    }
}
