using LapBox.Application.Common.Interfaces.Storage;

namespace LapBox.Infrastructure.Services.Storage;

/// <summary>
/// Local file storage implementation for development
/// In production, replace with cloud storage (Azure Blob, AWS S3, etc.)
/// </summary>
public sealed class LocalFileService : IFileService
{
    private readonly string _basePath;

    public LocalFileService(string basePath = "wwwroot/uploads")
    {
        _basePath = basePath;
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string folder, CancellationToken ct = default)
    {
        var directory = Path.Combine(_basePath, folder);
        Directory.CreateDirectory(directory);

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(directory, uniqueFileName);

        await using var outputStream = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(outputStream, ct);

        return $"/{folder}/{uniqueFileName}";
    }

    public Task DeleteAsync(string fileUrl, CancellationToken ct = default)
    {
        var filePath = Path.Combine(_basePath, fileUrl.TrimStart('/'));
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }
}
