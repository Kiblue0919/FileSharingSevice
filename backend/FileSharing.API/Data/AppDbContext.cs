using FileSharing.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileSharing.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<FileEntity> Files { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Code)
                  .IsRequired()
                  .HasMaxLength(10);

            entity.HasIndex(e => e.Code)
                  .IsUnique();

            entity.Property(e => e.OriginalFileName)
                  .IsRequired()
                  .HasMaxLength(255);

            entity.Property(e => e.MimeType)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.StoragePath)
                  .IsRequired();

            entity.Property(e => e.DownloadCount)
                  .HasDefaultValue(0);

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        });
    }
}