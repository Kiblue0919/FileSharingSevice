using FileSharing.API.DTOs;
using FileSharing.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FileSharing.API.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;

    public FilesController(IFileService fileService)
    {
        _fileService = fileService;
    }

    // POST /api/files
    [HttpPost]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile file,
        [FromForm] int? maxDownloads,
        [FromForm] string? expiresIn,
        [FromForm] string? password)
    {
        try
        {
            var request = new UploadFileRequestDto
            {
                File = file,
                MaxDownloads = maxDownloads,
                ExpiresIn = expiresIn,
                Password = password
            };
    
            var result = await _fileService.UploadFileAsync(request);
    
            return StatusCode(201, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET /api/files/{code}
    [HttpGet("{code}")]
    public async Task<IActionResult> GetMetadata(string code)
    {
        try
        {
            var result = await _fileService.GetFileMetadataAsync(code);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "File not found." });
        }
    }

    // GET /api/files
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _fileService.GetAllFilesAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET /api/files/{code}/download
    [HttpGet("{code}/download")]
    public async Task<IActionResult> Download(string code)
    {
        try
        {
            var (stream, mimeType, fileName) = await _fileService.DownloadFileAsync(code);
            return File(stream, mimeType, fileName);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "File not found." });
        }
        catch (InvalidOperationException ex) when (ex.Message.StartsWith("EXPIRED:"))
        {
            return StatusCode(410, new { message = ex.Message.Replace("EXPIRED:", "") });
        }
        catch (InvalidOperationException ex) when (ex.Message.StartsWith("LIMIT:"))
        {
            return StatusCode(410, new { message = ex.Message.Replace("LIMIT:", "") });
        }
    }

    // DELETE /api/files/{code}
    [HttpDelete("{code}")]
    public async Task<IActionResult> Delete(string code)
    {
        try
        {
            await _fileService.DeleteFileAsync(code);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "File not found." });
        }
    }
}
