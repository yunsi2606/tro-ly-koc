using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TroLiKOC.Modules.Payment.Infrastructure;
using TroLiKOC.Modules.Payment.Contracts;

namespace TroLiKOC.Modules.Payment;

public static class PaymentModule
{
    public static IServiceCollection AddPaymentModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<PaymentDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "payment")));

        services.AddScoped<IPaymentModule, Services.PaymentModuleService>();

        return services;
    }
}
