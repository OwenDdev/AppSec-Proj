using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureApp.API.Data;
using SecureApp.API.Models;
using BCrypt.Net;
//jwt
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using Microsoft.AspNetCore.Authorization;

namespace SecureApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
            private readonly AppDbContext _context;
            private readonly IConfiguration _config;

            public AuthController(AppDbContext context, IConfiguration config)
            {
                _context = context;
                _config = config;
            }

            // =========================
            // LOGIN (JWT GENERATION)
            // =========================
            [HttpPost("login")]
            public IActionResult Login([FromBody] LoginRequest request)
            {
                var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);

                if (user == null)
                    return Unauthorized("User not found");

                bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

                if (!isValid)
                    return Unauthorized("Wrong password");

                var jwtSettings = _config.GetSection("Jwt");

                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings["Key"])
                );

                var claims = new[]
                {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: jwtSettings["Issuer"],
                    audience: jwtSettings["Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: creds
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                return Ok(new
                {
                    token = tokenString,
                    role = user.Role,
                    username = user.Username
                });
            }

            // =========================
            // PROTECTED TEST ROUTE
            // =========================
            [Authorize]
            [HttpGet("protected")]
            public IActionResult Protected()
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                return Ok($"Hello {username}, you are authorized");
            }

            // =========================
            // ADMIN ONLY ROUTE
            // =========================
            [Authorize(Roles = "Admin")]
            [HttpGet("admin")]
            public IActionResult AdminOnly()
            {
                return Ok("Admin access granted");
            }

            // =========================
            // GET ALL USERS (ADMIN ONLY)
            // =========================
            [Authorize(Roles = "Admin")]
            [HttpGet("users")]
            public IActionResult GetUsers()
            {
                var users = _context.Users
                    .Select(u => new
                    {
                        u.Id,
                        u.Username,
                        u.Role
                    })
                    .ToList();

                return Ok(users);
            }

        //ADMIN: delete user
        [Authorize(Roles = "Admin")]
        [HttpDelete("users/{id}")]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);

            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            _context.SaveChanges();

            return Ok("User deleted");
        }
    }

}
