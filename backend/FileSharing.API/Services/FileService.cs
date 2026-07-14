using FileSharing.API.DTOs;
using FileSharing.API.Entities;
using FileSharing.API.Interfaces;

namespace FileSharing.API.Services;

public class FileService : IFileService
{
    private readonly IFileRepository _repository;
    private readonly IConfiguration _configuration;
    private readonly string _uploadPath;

    private static readonly string[] AllowedImageTypes =
        { "image/jpeg", "image/png", "image/gif", "image/webp" };

    public FileService(IFileRepository repository, IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
        _uploadPath = _configuration["FileStorage:UploadPath"] ?? "wwwroot/uploads";
    }

    public async Task<FileResponseDto> UploadFileAsync(UploadFileRequestDto request, string baseUrl)
    {
        if (request.File == null || request.File.Length == 0)
            throw new ArgumentException("No file uploaded.");

        var maxSize = long.Parse(_configuration["FileStorage:MaxFileSizeBytes"] ?? "10485760");
        if (request.File.Length > maxSize)
            throw new ArgumentException("File exceeds the maximum allowed size of 10 MB.");

        var code = GenerateCode();

        var fileName = $"{code}_{request.File.FileName}";
        var filePath = Path.Combine(_uploadPath, fileName);
        Directory.CreateDirectory(_uploadPath);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await request.File.CopyToAsync(stream);
        }

        DateTime? expiresAt = request.ExpiresIn switch
        {
            "1h" => DateTime.UtcNow.AddHours(1),
            "1d" => DateTime.UtcNow.AddDays(1),
            "1w" => DateTime.UtcNow.AddDays(7),
            _ => null
        };

        var entity = new FileEntity
        {
            Code = code,
            OriginalFileName = request.File.FileName,
            MimeType = request.File.ContentType,
            SizeBytes = request.File.Length,
            StoragePath = filePath,
            MaxDownloads = request.MaxDownloads,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(entity);

        return MapToDto(entity, baseUrl);
    }

    public async Task<FileResponseDto> GetFileMetadataAsync(string code)
    {
        var entity = await _repository.GetByCodeAsync(code)
            ?? throw new KeyNotFoundException("File not found.");

        return MapToDto(entity, "http://localhost:5000");
    }

    public async Task<(Stream stream, string mimeType, string fileName)> DownloadFileAsync(string code)
    {
        var entity = await _repository.GetByCodeAsync(code)
            ?? throw new KeyNotFoundException("File not found.");

        if (entity.ExpiresAt.HasValue && entity.ExpiresAt < DateTime.UtcNow)
        {
            await DeleteFileFromDiskAndDb(entity);
            throw new InvalidOperationException("EXPIRED:This file has expired and has been deleted.");
        }

        if (entity.MaxDownloads.HasValue && entity.DownloadCount >= entity.MaxDownloads)
        {
            await DeleteFileFromDiskAndDb(entity);
            throw new InvalidOperationException("LIMIT:This file has reached its download limit and has been deleted.");
        }

        entity.DownloadCount++;
        await _repository.UpdateAsync(entity);

        if (entity.MaxDownloads.HasValue && entity.DownloadCount >= entity.MaxDownloads)
        {
            await DeleteFileFromDiskAndDb(entity);
        }

        var stream = new FileStream(entity.StoragePath, FileMode.Open, FileAccess.Read);
        return (stream, entity.MimeType, entity.OriginalFileName);
    }

    public async Task DeleteFileAsync(string code)
    {
        var entity = await _repository.GetByCodeAsync(code)
            ?? throw new KeyNotFoundException("File not found.");

        await DeleteFileFromDiskAndDb(entity);
    }

    private async Task DeleteFileFromDiskAndDb(FileEntity entity)
    {
        if (File.Exists(entity.StoragePath))
            File.Delete(entity.StoragePath);

        await _repository.DeleteAsync(entity);
    }

    private static string GenerateCode()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Range(0, 6)
            .Select(_ => chars[random.Next(chars.Length)])
            .ToArray());
    }

    private static FileResponseDto MapToDto(FileEntity entity, string baseUrl)
    {
        var isImage = AllowedImageTypes.Contains(entity.MimeType);
        var isExpired = entity.ExpiresAt.HasValue && entity.ExpiresAt < DateTime.UtcNow;

        return new FileResponseDto
        {
            Code = entity.Code,
            OriginalFileName = entity.OriginalFileName,
            MimeType = entity.MimeType,
            SizeBytes = entity.SizeBytes,
            DownloadUrl = string.IsNullOrEmpty(baseUrl) ? "" : $"{baseUrl}/f/{entity.Code}",
            DownloadCount = entity.DownloadCount,
            MaxDownloads = entity.MaxDownloads,
            ExpiresAt = entity.ExpiresAt,
            IsExpired = isExpired,
            IsImage = isImage,
            IsPasswordProtected = !string.IsNullOrEmpty(entity.PasswordHash),
            CreatedAt = entity.CreatedAt
        };
    }
}