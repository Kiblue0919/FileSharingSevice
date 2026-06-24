namespace FileSharing.API.Entities;

public class FileEntity
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public int DownloadCount { get; set; } = 0;
    public int? MaxDownloads { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}