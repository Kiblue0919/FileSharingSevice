using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FileSharing.API.Interfaces;

namespace FileSharing.API.Services;

public class CloudinaryStorageService : IStorageService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryStorageService(IConfiguration configuration)
    {
        var account = new Account(
            configuration["Cloudinary:CloudName"],
            configuration["Cloudinary:ApiKey"],
            configuration["Cloudinary:ApiSecret"]
        );
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string mimeType)
    {
        var publicId = $"filesharing/{Path.GetFileNameWithoutExtension(fileName)}_{Guid.NewGuid():N}";

        var result = mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
            ? await _cloudinary.UploadAsync(new ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                PublicId = publicId,
                Overwrite = false
            })
            : await _cloudinary.UploadAsync(new RawUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                PublicId = publicId,
                Overwrite = false
            });

        if (result.Error != null)
            throw new Exception($"Cloudinary upload failed: {result.Error.Message}");

        return result.PublicId;
    }

    public async Task DeleteFileAsync(string publicId, string mimeType)
    {
        var deleteParams = new DeletionParams(publicId)
        {
            ResourceType = mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
                ? ResourceType.Image
                : ResourceType.Raw
        };

        await _cloudinary.DestroyAsync(deleteParams);
    }

    public Task<string> ResolveFileUrlAsync(string publicId, string mimeType)
    {
        var isImage = mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

        var urlBuilder = _cloudinary.Api.Url
            .Secure(true)
            .ResourceType(isImage ? "image" : "raw")
            .Type("upload");

        var format = isImage ? GetImageFormat(mimeType) : null;

        var url = format == null 
            ? urlBuilder.BuildUrl(publicId)
            : urlBuilder.Format(format).BuildUrl(publicId);

        return Task.FromResult(url);
    }

    private static string? GetImageFormat(string mimeType)
    {
        return mimeType switch
        {
            "image/jpeg" => "jpg",
            "image/png" => "png",
            "image/gif" => "gif",
            "image/webp" => "webp",
            _ => null
        };
    }
}
