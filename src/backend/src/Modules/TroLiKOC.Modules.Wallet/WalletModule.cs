using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TroLiKOC.Modules.Wallet.Application.Services;
using TroLiKOC.Modules.Wallet.Contracts;
using TroLiKOC.Modules.Wallet.Infrastructure;

namespace TroLiKOC.Modules.Wallet;

public static class WalletModule
{
    public static IServiceCollection AddWalletModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<WalletDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "wallet")));

        services.AddScoped<IWalletModule, WalletService>();

        return services;
    }
}
