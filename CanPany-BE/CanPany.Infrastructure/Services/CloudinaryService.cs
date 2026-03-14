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
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (fileStream == null)
            {
                throw new ArgumentNullException(nameof(fileStream));
            }

            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            // In CloudinaryDotNet 1.28.0, raw upload uses Upload (not UploadAsync overload with cancellation token)
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if ((int)uploadResult.StatusCode < 200 ||
                (int)uploadResult.StatusCode >= 300)
            {
                _logger.LogError("Cloudinary upload failed. Status: {StatusCode}, Error: {Error}",
                    uploadResult.StatusCode,
                    uploadResult.Error?.Message);

                throw new InvalidOperationException("Failed to upload file to Cloudinary.");
            }

            var secureUrl = uploadResult.SecureUrl?.ToString() ?? string.Empty;
            var publicId = uploadResult.PublicId ?? string.Empty;

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
}

