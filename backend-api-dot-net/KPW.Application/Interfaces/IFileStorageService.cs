namespace KPW.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(
        Stream content,
        string fileName,
        string folder = "uploads",
        string? contentType = null,
        CancellationToken cancellationToken = default);

    string GetPublicUrl(string storagePath, TimeSpan? duration = null);
}
