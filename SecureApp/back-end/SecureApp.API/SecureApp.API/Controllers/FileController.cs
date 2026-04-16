using Microsoft.AspNetCore.Mvc;
using SecureApp.API.Data;
using SecureApp.API.Models;
using System.Security.Claims;

using SecureApp.API.Services;

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
    }
}

