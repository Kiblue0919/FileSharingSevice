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

    public async Task<string> ResolveFileUrlAsync(string publicId, string mimeType)
    {
        var isImage = mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

        var candidateUrls = isImage
            ? new[]
            {
                _cloudinary.Api.Url.ResourceType("image").BuildUrl(publicId),
                _cloudinary.Api.Url.ResourceType("raw").BuildUrl(publicId)
            }
            : new[]
            {
                _cloudinary.Api.Url.ResourceType("raw").BuildUrl(publicId),
                _cloudinary.Api.Url.ResourceType("image").BuildUrl(publicId)
            };

        using var httpClient = new HttpClient();

        foreach (var candidateUrl in candidateUrls.Distinct())
        {
            using var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, candidateUrl);
            using var response = await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead);

            if (response.IsSuccessStatusCode)
                return candidateUrl;
        }

        return candidateUrls[0];
    }
}