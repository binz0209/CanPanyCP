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
}

