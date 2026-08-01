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
        // Tạo public ID từ filename (bỏ extension)
        var publicId = $"filesharing/{Path.GetFileNameWithoutExtension(fileName)}_{Guid.NewGuid():N}";

        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            PublicId = publicId,
            Overwrite = false
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
            throw new Exception($"Cloudinary upload failed: {result.Error.Message}");

        return result.PublicId;
    }

    public async Task DeleteFileAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId)
        {
            ResourceType = ResourceType.Raw
        };

        await _cloudinary.DestroyAsync(deleteParams);
    }

    public string GetFileUrl(string publicId)
    {
        return _cloudinary.Api.UrlImgUp.BuildUrl(publicId);
    }
}