using FileSharing.API.Data;
using Microsoft.EntityFrameworkCore;

namespace FileSharing.API.Services;

public class CleanupHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CleanupHostedService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);

    public CleanupHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<CleanupHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Cleanup Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunCleanupAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cleanup service encountered an error. Will retry next interval.");
            }
            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task RunCleanupAsync()
    {
        _logger.LogInformation("Running cleanup at {Time}", DateTime.UtcNow);

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var expiredFiles = await db.Files
            .Where(f =>
                (f.ExpiresAt.HasValue && f.ExpiresAt < DateTime.UtcNow) ||
                (f.MaxDownloads.HasValue && f.DownloadCount >= f.MaxDownloads))
            .ToListAsync();

        foreach (var file in expiredFiles)
        {
            if (File.Exists(file.StoragePath))
            {
                File.Delete(file.StoragePath);
                _logger.LogInformation("Deleted file: {Path}", file.StoragePath);
            }
        }

        db.Files.RemoveRange(expiredFiles);
        await db.SaveChangesAsync();

        _logger.LogInformation("Cleanup done. Removed {Count} file(s).", expiredFiles.Count);
    }
}
