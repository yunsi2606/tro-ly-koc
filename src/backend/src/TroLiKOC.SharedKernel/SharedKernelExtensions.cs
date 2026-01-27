using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TroLiKOC.SharedKernel.Infrastructure;

namespace TroLiKOC.SharedKernel;

public static class SharedKernelExtensions
{
    public static IServiceCollection AddSharedKernel(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(SharedKernelExtensions).Assembly));

        // Add MinIO
        services.AddMinioStorage(configuration);
        
        return services;
    }
}
