using FileSharing.API.DTOs;

namespace FileSharing.API.Interfaces;

public interface IFileService
{
    Task<FileResponseDto> UploadFileAsync(UploadFileRequestDto request, string baseUrl);
    Task<FileResponseDto> GetFileMetadataAsync(string code);
    Task<(Stream stream, string mimeType, string fileName)> DownloadFileAsync(string code);
    Task DeleteFileAsync(string code);
    Task<List<FileResponseDto>> GetAllFilesAsync();
}