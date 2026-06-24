using FileSharing.API.Entities;

namespace FileSharing.API.Interfaces;

public interface IFileRepository
{
    Task<FileEntity?> GetByCodeAsync(string code);
    Task<FileEntity> CreateAsync(FileEntity file);
    Task UpdateAsync(FileEntity file);
    Task DeleteAsync(FileEntity file);
    Task<List<FileEntity>> GetExpiredFilesAsync();
}