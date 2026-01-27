using Minio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TroLiKOC.SharedKernel.Infrastructure;

public static class MinioExtensions
{
    public static IServiceCollection AddMinioStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var minioConfig = configuration.GetSection("MinIO");
        var endpoint = minioConfig["Endpoint"];
        var accessKey = minioConfig["AccessKey"];
        var secretKey = minioConfig["SecretKey"];

        if (string.IsNullOrEmpty(endpoint)) return services;

        services.AddMinio(configureClient => configureClient
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(false) // Default to false for local/docker internal network
            .Build());

        return services;
    }
}
