namespace LapBox.Application.Common.Interfaces.Storage;

public interface IFileService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string folder, CancellationToken ct = default);
    Task DeleteAsync(string fileUrl, CancellationToken ct = default);
}