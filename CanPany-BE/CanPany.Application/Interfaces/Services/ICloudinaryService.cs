using System.IO;

namespace CanPany.Application.Interfaces.Services;

public interface ICloudinaryService
{
    Task<(string SecureUrl, string PublicId)> UploadAsync(
        Stream fileStream,
        string fileName,
        string folder,
        string resourceType = "raw",
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string publicId, string resourceType = "raw");

    /// <summary>
    /// Generates a signed (time-limited) download URL for a Cloudinary resource.
    /// Required when the account has "Secure raw files" or access-control restrictions.
    /// </summary>
    string GetSignedDownloadUrl(string publicId, string resourceType = "raw", int expiresInSeconds = 3600);
}

