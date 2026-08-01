using FileSharing.API.DTOs;
using FileSharing.API.Entities;
using FileSharing.API.Interfaces;
using FileSharing.API.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace FileSharing.Tests;

public class FileServiceTests
{
    private readonly Mock<IFileRepository> _repositoryMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly FileService _fileService;

    public FileServiceTests()
    {
        _repositoryMock = new Mock<IFileRepository>();
        _configMock = new Mock<IConfiguration>();

        // Setup config mock
        _configMock.Setup(c => c["FileStorage:UploadPath"]).Returns("wwwroot/uploads");
        _configMock.Setup(c => c["FileStorage:MaxFileSizeBytes"]).Returns("10485760");

        _fileService = new FileService(_repositoryMock.Object, _configMock.Object);
    }

    // ─── Test 1: Upload null file ────────────────────────────────
    [Fact]
    public async Task UploadFileAsync_NullFile_ThrowsArgumentException()
    {
        // Arrange
        var request = new UploadFileRequestDto { File = null! };

        // Act
        var act = async () => await _fileService.UploadFileAsync(request, "http://localhost:5000");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("No file uploaded.");
    }

    // ─── Test 2: Get file không tồn tại ─────────────────────────
    [Fact]
    public async Task GetFileMetadataAsync_FileNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByCodeAsync("abc123"))
            .ReturnsAsync((FileEntity?)null);

        // Act
        var act = async () => await _fileService.GetFileMetadataAsync("abc123");

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("File not found.");
    }

    // ─── Test 3: Download file đã expired ────────────────────────
    [Fact]
    public async Task DownloadFileAsync_ExpiredFile_ThrowsInvalidOperationException()
    {
        // Arrange
        var expiredFile = new FileEntity
        {
            Code = "abc123",
            OriginalFileName = "test.jpg",
            MimeType = "image/jpeg",
            StoragePath = "wwwroot/uploads/test.jpg",
            ExpiresAt = DateTime.UtcNow.AddHours(-1) // expired 1 hour ago
        };

        _repositoryMock.Setup(r => r.GetByCodeAsync("abc123"))
            .ReturnsAsync(expiredFile);

        // Act
        var act = async () => await _fileService.DownloadFileAsync("abc123");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*expired*");
    }

    // ─── Test 4: Download file đã hết lượt download ──────────────
    [Fact]
    public async Task DownloadFileAsync_OverDownloadLimit_ThrowsInvalidOperationException()
    {
        // Arrange
        var limitedFile = new FileEntity
        {
            Code = "abc123",
            OriginalFileName = "test.jpg",
            MimeType = "image/jpeg",
            StoragePath = "wwwroot/uploads/test.jpg",
            MaxDownloads = 3,
            DownloadCount = 3 // already at limit
        };

        _repositoryMock.Setup(r => r.GetByCodeAsync("abc123"))
            .ReturnsAsync(limitedFile);

        // Act
        var act = async () => await _fileService.DownloadFileAsync("abc123");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*limit*");
    }

    // ─── Test 5: Delete file không tồn tại ───────────────────────
    [Fact]
    public async Task DeleteFileAsync_FileNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByCodeAsync("abc123"))
            .ReturnsAsync((FileEntity?)null);

        // Act
        var act = async () => await _fileService.DeleteFileAsync("abc123");

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("File not found.");
    }

    // ─── Test 6: GetMetadata trả về đúng thông tin ───────────────
    [Fact]
    public async Task GetFileMetadataAsync_ValidCode_ReturnsCorrectMetadata()
    {
        // Arrange
        var file = new FileEntity
        {
            Code = "abc123",
            OriginalFileName = "cat.jpg",
            MimeType = "image/jpeg",
            SizeBytes = 125000,
            DownloadCount = 3,
            MaxDownloads = 10,
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock.Setup(r => r.GetByCodeAsync("abc123"))
            .ReturnsAsync(file);

        // Act
        var result = await _fileService.GetFileMetadataAsync("abc123");

        // Assert
        result.Code.Should().Be("abc123");
        result.OriginalFileName.Should().Be("cat.jpg");
        result.MimeType.Should().Be("image/jpeg");
        result.IsImage.Should().BeTrue();
        result.DownloadCount.Should().Be(3);
        result.MaxDownloads.Should().Be(10);
    }
}