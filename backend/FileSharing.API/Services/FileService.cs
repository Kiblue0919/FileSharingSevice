using FileSharing.API.DTOs;
using FileSharing.API.Entities;
using FileSharing.API.Interfaces;

namespace FileSharing.API.Services;

public class FileService : IFileService
{
    private readonly IFileRepository _repository;
    private readonly IConfiguration _configuration;
    private readonly IStorageService _storageService;

    private static readonly string[] AllowedImageTypes =
    {
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp"
    };

    private static readonly string[] AllowedMimeTypes =
    {
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "text/plain",
        "application/zip",
        "video/mp4"
    };

    private string FrontendUrl =>
        (_configuration["FrontendUrl"]
            ?? "https://filesharingsevice-production-eb82.up.railway.app")
        .TrimEnd('/');

    public FileService(
        IFileRepository repository,
        IConfiguration configuration,
        IStorageService storageService)
    {
        _repository = repository;
        _configuration = configuration;
        _storageService = storageService;
    }

    public async Task<List<FileResponseDto>> GetAllFilesAsync()
    {
        var entities = await _repository.GetAllAsync();
        var result = new List<FileResponseDto>();

        foreach (var entity in entities)
        {
            result.Add(await MapToDtoAsync(entity));
        }

        return result;
    }

    public async Task<FileResponseDto> UploadFileAsync(
        UploadFileRequestDto request)
    {
        if (request.File == null || request.File.Length == 0)
            throw new ArgumentException("No file uploaded.");

        var maxSize = long.Parse(
            _configuration["FileStorage:MaxFileSizeBytes"]
            ?? "10485760");

        if (request.File.Length > maxSize)
            throw new ArgumentException(
                "File exceeds the maximum allowed size of 10 MB.");

        if (!AllowedMimeTypes.Contains(request.File.ContentType))
            throw new ArgumentException(
                $"File type '{request.File.ContentType}' is not allowed.");

        var code = GenerateCode();

        using var stream = request.File.OpenReadStream();

        var publicId = await _storageService.UploadFileAsync(
            stream,
            request.File.FileName,
            request.File.ContentType);

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
            StoragePath = publicId,
            MaxDownloads = request.MaxDownloads,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(entity);

        return await MapToDtoAsync(entity);
    }

    public async Task<FileResponseDto> GetFileMetadataAsync(string code)
    {
        var entity = await _repository.GetByCodeAsync(code)
            ?? throw new KeyNotFoundException("File not found.");

        return await MapToDtoAsync(entity);
    }

    public async Task<string> GetDownloadUrlAsync(string code)
    {
        var entity = await _repository.GetByCodeAsync(code)
            ?? throw new KeyNotFoundException("File not found.");

        if (entity.ExpiresAt.HasValue &&
            entity.ExpiresAt < DateTime.UtcNow)
        {
            await DeleteFileFromStorageAndDb(entity);

            throw new InvalidOperationException(
                "EXPIRED:This file has expired and has been deleted.");
        }

        if (entity.MaxDownloads.HasValue &&
            entity.DownloadCount >= entity.MaxDownloads)
        {
            await DeleteFileFromStorageAndDb(entity);

            throw new InvalidOperationException(
                "LIMIT:This file has reached its download limit and has been deleted.");
        }

        entity.DownloadCount++;
        await _repository.UpdateAsync(entity);

        return await _storageService.ResolveFileUrlAsync(
            entity.StoragePath,
            entity.MimeType);
    }

    public async Task<(Stream stream, string mimeType, string fileName)>
        DownloadFileAsync(string code)
    {
        var entity = await _repository.GetByCodeAsync(code)
            ?? throw new KeyNotFoundException("File not found.");

        if (entity.ExpiresAt.HasValue &&
            entity.ExpiresAt < DateTime.UtcNow)
        {
            await DeleteFileFromStorageAndDb(entity);

            throw new InvalidOperationException(
                "EXPIRED:This file has expired and has been deleted.");
        }

        if (entity.MaxDownloads.HasValue &&
            entity.DownloadCount >= entity.MaxDownloads)
        {
            await DeleteFileFromStorageAndDb(entity);

            throw new InvalidOperationException(
                "LIMIT:This file has reached its download limit and has been deleted.");
        }

        entity.DownloadCount++;
        await _repository.UpdateAsync(entity);

        var fileUrl = await _storageService.ResolveFileUrlAsync(entity.StoragePath, entity.MimeType);

        using var httpClient = new HttpClient();
        var fileBytes = await httpClient.GetByteArrayAsync(fileUrl);
        var stream = new MemoryStream(fileBytes);

        return (
            stream,
            entity.MimeType,
            entity.OriginalFileName
        );
    }

    public async Task DeleteFileAsync(string code)
    {
        var entity = await _repository.GetByCodeAsync(code)
            ?? throw new KeyNotFoundException("File not found.");

        await DeleteFileFromStorageAndDb(entity);
    }

    private async Task DeleteFileFromStorageAndDb(FileEntity entity)
    {
        await _storageService.DeleteFileAsync(entity.StoragePath, entity.MimeType);
        await _repository.DeleteAsync(entity);
    }

    private static string GenerateCode()
    {
        const string chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        return new string(
            Enumerable.Range(0, 6)
                .Select(_ => chars[Random.Shared.Next(chars.Length)])
                .ToArray());
    }

    private async Task<FileResponseDto> MapToDtoAsync(FileEntity entity)
    {
        var isImage = AllowedImageTypes.Contains(entity.MimeType);

        var isExpired =
            entity.ExpiresAt.HasValue &&
            entity.ExpiresAt < DateTime.UtcNow;

        return new FileResponseDto
        {
            Code = entity.Code,
            OriginalFileName = entity.OriginalFileName,
            MimeType = entity.MimeType,
            SizeBytes = entity.SizeBytes,
            DownloadUrl = $"{FrontendUrl}/f/{entity.Code}",
            FileUrl = isImage
                ? await _storageService.ResolveFileUrlAsync(entity.StoragePath, entity.MimeType)
                : null,
            DownloadCount = entity.DownloadCount,
            MaxDownloads = entity.MaxDownloads,
            ExpiresAt = entity.ExpiresAt,
            IsExpired = isExpired,
            IsImage = isImage,
            IsPasswordProtected =
                !string.IsNullOrEmpty(entity.PasswordHash),
            CreatedAt = entity.CreatedAt
        };
    }
}
