using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;

namespace TroLiKOC.SharedKernel.Infrastructure;

public interface IStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string bucketName = "default");
    Task<string> GetPresignedUrlAsync(string objectName, string bucketName = "default", int expirySeconds = 3600);
}

public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _minioClient;

    public MinioStorageService(IConfiguration configuration)
    {
        var endpoint = configuration["MinIO:Endpoint"];
        var accessKey = configuration["MinIO:AccessKey"];
        var secretKey = configuration["MinIO:SecretKey"];

        _minioClient = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .Build();
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string bucketName = "default")
    {
        // Ensure bucket exists
        bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
        if (!found)
        {
            await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
        }

        // Upload
        var putObjectArgs = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(fileName)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(contentType);

        await _minioClient.PutObjectAsync(putObjectArgs);

        return fileName;
    }

    public async Task<string> GetPresignedUrlAsync(string objectName, string bucketName = "default", int expirySeconds = 3600)
    {
        var args = new PresignedGetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithExpiry(expirySeconds);

        return await _minioClient.PresignedGetObjectAsync(args);
    }
}
