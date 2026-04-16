using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureApp.API.Data;
using SecureApp.API.Models;
using SecureApp.API.Services;
using System.Security.Claims;

namespace SecureApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICustomLogger _customLogger;


        public FileController(AppDbContext context, ICustomLogger customLogger)
        {
            _context = context;
            _customLogger = customLogger;
        }

        [Authorize]
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;

            _customLogger.Log($"File upload attempt by {username}");

            if (file == null || file.Length == 0)
            {
                _customLogger.LogWarning($"Upload failed: No file provided by {username}");
                return BadRequest("No file uploaded");
            }


            const long maxSize = 5242880;
            if (file.Length > maxSize)
            {
                _customLogger.LogWarning($"Upload blocked: File too large ({file.Length} bytes) by {username}");
                return BadRequest("File too large (max 5MB)");
            }



            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".csv" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                _customLogger.LogWarning($"Upload blocked: Invalid file type ({extension}) by {username}");
                return BadRequest("Only PDF and JPG allowed");
            }


            try
            {
                var safeFileName = Guid.NewGuid().ToString() + extension;

                var path = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Uploads",
                    safeFileName
                );

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var fileRecord = new AppFile
                {
                    FileName = file.FileName,
                    FilePath = "/Uploads/" + safeFileName,
                    UploadedBy = username
                };

                _context.Files.Add(fileRecord);
                await _context.SaveChangesAsync();

                _customLogger.Log($"File uploaded SUCCESS: {file.FileName} by {username}");

                return Ok(new { message = "File uploaded successfully" });
            }
            catch (Exception ex)
            {
                _customLogger.LogError($"Upload ERROR for {username}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

        }
        [Authorize]
        [HttpGet("myfiles")]
        public IActionResult GetMyFiles()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            _customLogger.Log($"User {username} requested their file list");

            var files = _context.Files
                .Where(f => f.UploadedBy == username)
                .Select(f => new
                {
                    f.Id,
                    f.FileName,
                    f.FilePath
                })
                .ToList();

            return Ok(files);
        }



        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;

            var file = await _context.Files.FindAsync(id);

            if (file == null)
            {
                _customLogger.LogWarning($"Delete failed: File ID {id} not found by {username}");
                return NotFound();
            }

            // Prevent deleting other users' files
            if (file.UploadedBy != username)
            {
                _customLogger.LogWarning($"Unauthorized delete attempt by {username} on file ID {id}");
                return Forbid();
            }

            // Delete physical file
            var fullPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                file.FilePath.TrimStart('/')
            );
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            _context.Files.Remove(file);
            await _context.SaveChangesAsync();

            _customLogger.Log($"File deleted: {file.FileName} by {username}");

            return Ok(new { message = "File deleted" });
        }

    }
    
}

