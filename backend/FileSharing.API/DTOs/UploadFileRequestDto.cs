namespace FileSharing.API.DTOs;

public class UploadFileRequestDto
{
    public IFormFile File { get; set; } = null!;
    public int? MaxDownloads { get; set; }
    public string? ExpiresIn { get; set; }
    public string? Password { get; set; }
}