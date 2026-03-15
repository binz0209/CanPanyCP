using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CanPany.Infrastructure.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryService> _logger;

    public CloudinaryService(
        IOptions<CloudinaryOptions> options,
        ILogger<CloudinaryService> logger)
    {
        _logger = logger;

        var config = options.Value ?? new CloudinaryOptions();

        // Fallback to environment variables if appsettings values are missing
        var cloudName = string.IsNullOrWhiteSpace(config.CloudName)
            ? Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME") ?? string.Empty
            : config.CloudName;

        var apiKey = string.IsNullOrWhiteSpace(config.ApiKey)
            ? Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY") ?? string.Empty
            : config.ApiKey;

        var apiSecret = string.IsNullOrWhiteSpace(config.ApiSecret)
            ? Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET") ?? string.Empty
            : config.ApiSecret;

        if (string.IsNullOrWhiteSpace(cloudName) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(apiSecret))
        {
            _logger.LogWarning("Cloudinary configuration is missing or incomplete. Uploads will fail until configured.");
        }

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account)
        {
            Api = { Secure = true }
        };
    }

    public async Task<(string SecureUrl, string PublicId)> UploadAsync(
        Stream fileStream,
        string fileName,
        string folder,
        string resourceType = "raw",
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (fileStream == null)
            {
                throw new ArgumentNullException(nameof(fileStream));
            }

            // Select upload parameters based on resource type
            string secureUrl;
            string publicId;

            if (resourceType.ToLower() == "image")
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(fileName, fileStream),
                    Folder = folder,
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };
                var imageResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

                if (imageResult.Error != null)
                {
                    _logger.LogError("Cloudinary upload failed. Error: {Error}", imageResult.Error.Message);
                    throw new InvalidOperationException($"Failed to upload file to Cloudinary: {imageResult.Error.Message}");
                }

                secureUrl = imageResult.SecureUrl?.ToString() ?? string.Empty;
                publicId = imageResult.PublicId ?? string.Empty;
            }
            else
            {
                // For raw/document files (PDF, DOCX), use RawUploadParams
                // SDK v1.23.0 signature: UploadAsync(RawUploadParams, string resourceType, CancellationToken)
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(fileName, fileStream),
                    Folder = folder,
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };
                var rawResult = await _cloudinary.UploadAsync(uploadParams, "raw", cancellationToken);

                if (rawResult.Error != null)
                {
                    _logger.LogError("Cloudinary upload failed. Error: {Error}", rawResult.Error.Message);
                    throw new InvalidOperationException($"Failed to upload file to Cloudinary: {rawResult.Error.Message}");
                }

                secureUrl = rawResult.SecureUrl?.ToString() ?? string.Empty;
                publicId = rawResult.PublicId ?? string.Empty;
            }

            if (string.IsNullOrEmpty(secureUrl))
            {
                _logger.LogError("Cloudinary upload returned empty secure URL for file {FileName}", fileName);
                throw new InvalidOperationException("Cloudinary upload did not return a secure URL.");
            }

            return (secureUrl, publicId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName} to Cloudinary", fileName);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string publicId, string resourceType = "raw")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                return false;
            }

            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = resourceType.ToLower() == "image" ? ResourceType.Image : ResourceType.Raw
            };

            var result = await _cloudinary.DestroyAsync(deletionParams);

            if (result.Result == "ok")
            {
                _logger.LogInformation("Successfully deleted file from Cloudinary: {PublicId}", publicId);
                return true;
            }

            _logger.LogWarning("Cloudinary deletion returned non-ok result: {Result} for {PublicId}", result.Result, publicId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {PublicId} from Cloudinary", publicId);
            return false;
        }
    }
}

