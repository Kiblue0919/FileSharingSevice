using FileSharing.API.Data;
using FileSharing.API.Entities;
using FileSharing.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FileSharing.API.Repositories;

public class FileRepository : IFileRepository
{
	private readonly AppDbContext _context;

	public FileRepository(AppDbContext context)
	{
		_context = context;
	}

	public async Task<FileEntity?> GetByCodeAsync(string code)
	{
		return await _context.Files
			.FirstOrDefaultAsync(f => f.Code == code);
	}

	public async Task<FileEntity> CreateAsync(FileEntity file)
	{
		_context.Files.Add(file);
		await _context.SaveChangesAsync();
		return file;
	}

	public async Task UpdateAsync(FileEntity file)
	{
		_context.Files.Update(file);
		await _context.SaveChangesAsync();
	}

	public async Task DeleteAsync(FileEntity file)
	{
		_context.Files.Remove(file);
		await _context.SaveChangesAsync();
	}

	public async Task<List<FileEntity>> GetExpiredFilesAsync()
	{
		return await _context.Files
			.Where(f =>
				(f.ExpiresAt.HasValue && f.ExpiresAt < DateTime.UtcNow) ||
				(f.MaxDownloads.HasValue && f.DownloadCount >= f.MaxDownloads))
			.ToListAsync();
	}
}