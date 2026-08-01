namespace FileSharing.API.DTOs;

public class FileResponseDto
{
    public string Code { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public int DownloadCount { get; set; }
    public int? MaxDownloads { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsExpired { get; set; }
    public bool IsImage { get; set; }
    public bool IsPasswordProtected { get; set; }
    public DateTime CreatedAt { get; set; }
}