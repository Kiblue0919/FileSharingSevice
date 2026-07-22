namespace FileSharing.API.Interfaces;

public interface IStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string mimeType);
    Task DeleteFileAsync(string publicId);
    string GetFileUrl(string publicId);
}