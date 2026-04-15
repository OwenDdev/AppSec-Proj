using Microsoft.AspNetCore.Mvc;
using SecureApp.API.Data;
using SecureApp.API.Models;
using System.Security.Claims;

namespace SecureApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FileController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            // 1. File size limit (5MB)
            const long maxSize = 5 * 1024 * 1024;
            if (file.Length > maxSize)
                return BadRequest("File too large (max 5MB)");

            // 2. File type validation
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Only PDF and JPG allowed");

            // 3. Secure filename
            var safeFileName = Guid.NewGuid().ToString() + extension;

            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Uploads",
                safeFileName
            );

            // 4. Save file to disk
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 5. Get user from JWT
            var username = User.FindFirst(ClaimTypes.Name)?.Value;

            // 6. Save to DB
            var fileRecord = new AppFile
            {
                FileName = file.FileName,
                FilePath = "/Uploads/" + safeFileName,
                UploadedBy = username
            };

            _context.Files.Add(fileRecord);
            await _context.SaveChangesAsync();

            return Ok(new { message = "File uploaded successfully" });
        }
    }
}

