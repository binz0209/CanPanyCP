using System.IO;

namespace CanPany.Application.Interfaces.Services;

public interface ICloudinaryService
{
    Task<(string SecureUrl, string PublicId)> UploadAsync(
        Stream fileStream,
        string fileName,
        string folder,
        CancellationToken cancellationToken = default);
}

