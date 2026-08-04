namespace KPW.Application.Interfaces;

public interface IVideoStorage
{
    Task<string> UploadAsync(Stream content, string fileName, CancellationToken cancellationToken = default);
    string GetPublicUrl(string storagePath);
}
