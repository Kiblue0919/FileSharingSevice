namespace FileSharing.API.Interfaces;

public interface IStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string mimeType);
    Task DeleteFileAsync(string publicId, string mimeType);
    Task<string> ResolveFileUrlAsync(string publicId, string mimeType);
}