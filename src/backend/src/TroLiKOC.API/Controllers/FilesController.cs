using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;
using System.Security.Claims;

namespace TroLiKOC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IMinioClient _minioClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FilesController> _logger;

    private readonly string _bucketName;
    private readonly string _minioEndpoint;

    public FilesController(
        IMinioClient minioClient,
        IConfiguration configuration,
        ILogger<FilesController> logger)
    {
        _minioClient = minioClient;
        _configuration = configuration;
        _logger = logger;
        _bucketName = configuration["Minio:BucketName"] ?? "trolikoc";
        _minioEndpoint = configuration["Minio:Endpoint"] ?? "localhost:9000";
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file, [FromForm] string folder = "inputs")
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No file provided" });
        }

        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            var fileExtension = Path.GetExtension(file.FileName);
            var objectName = $"{folder}/{userId}/{Guid.NewGuid()}{fileExtension}";

            // Ensure bucket exists
            var bucketExists = await _minioClient.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(_bucketName)
            );

            if (!bucketExists)
            {
                await _minioClient.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(_bucketName)
                );
            }

            // Upload file
            using var stream = file.OpenReadStream();
            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(file.Length)
                .WithContentType(file.ContentType)
            );

            // Generate URL
            var useHttps = _configuration.GetValue<bool>("Minio:UseSSL", false);
            var protocol = useHttps ? "https" : "http";
            var url = $"{protocol}://{_minioEndpoint}/{_bucketName}/{objectName}";

            _logger.LogInformation("File uploaded: {ObjectName} by user {UserId}", objectName, userId);

            return Ok(new { url, objectName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, new { error = "Upload failed" });
        }
    }

    [HttpGet("presigned-url")]
    public async Task<IActionResult> GetPresignedUrl([FromQuery] string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
        {
            return BadRequest(new { error = "objectName is required" });
        }

        try
        {
            var url = await _minioClient.PresignedGetObjectAsync(
                new PresignedGetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(objectName)
                    .WithExpiry(3600) // 1 hour
            );

            return Ok(new { url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL");
            return StatusCode(500, new { error = "Failed to generate URL" });
        }
    }
}
