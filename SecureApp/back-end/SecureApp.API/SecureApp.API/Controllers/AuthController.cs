using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SecureApp.API.Data;
using SecureApp.API.Models;
//jwt
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
//logger
using SecureApp.API.Services;

namespace SecureApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
            private readonly AppDbContext _context;
            private readonly IConfiguration _config;

        private readonly ICustomLogger _customLogger;

        public AuthController(AppDbContext context, IConfiguration config, ICustomLogger customLogger)
            {
                _context = context;
                _config = config;
            _customLogger = customLogger;
        }

            [HttpPost("login")]
            public IActionResult Login([FromBody] LoginRequest request)
            {

                _customLogger.Log($"Login attempt from for user: {request.Username}");

                var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);
                var refreshToken = Guid.NewGuid().ToString();

            if (user == null)
            {
                _customLogger.LogWarning($"Login failed (user not found) from : {request.Username}");
                return Unauthorized("Invalid Username or password");
            }
                  

                bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isValid)
            {
                _customLogger.LogWarning($"Login failed from: {request.Username}");
                return Unauthorized("Invalid Username or password");
            }

            _customLogger.Log($"Login SUCCESS for user: {request.Username}");

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
                    refreshToken = refreshToken,
                    role = user.Role,
                    username = user.Username
                });
            }

        
            [Authorize]
            [HttpGet("protected")]
            public IActionResult Protected()
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value;

                _customLogger.Log($"Protected endpoint accessed by {username}");
                return Ok($"Hello {username}, you are authorized");
            }
        
           
            [Authorize(Roles = "Admin")]
            [HttpGet("admin")]
            public IActionResult AdminOnly()
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                _customLogger.Log($"ADMIN endpoint accessed by {username}");
                return Ok("Admin access granted");
            }

            [Authorize(Roles = "Admin")]
            [HttpGet("users")]
            public IActionResult GetUsers()
            {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            _customLogger.Log($"Admin {username} requested user list");
                
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


        [Authorize(Roles = "Admin")]
        [HttpDelete("users/{id}")]
        public IActionResult DeleteUser(int id)
        {
            var admin = User.FindFirst(ClaimTypes.Name)?.Value;
            var user = _context.Users.Find(id);

            if (user == null)
            {
                _customLogger.LogWarning($"Admin {admin} attempted to delete NON-EXISTENT user ID {id}");
                return NotFound();
            }
                

            _context.Users.Remove(user);
            _context.SaveChanges();

            _customLogger.Log($"Admin {admin} DELETED user {user.Username} (ID: {id})");

            return Ok("User deleted");
        }
        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshRequest request)
        {

            _customLogger.Log($"Token refresh attempt for {request.Username}");

            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                _customLogger.LogWarning($"Refresh FAILED (missing token)");
                return Unauthorized();
            }

            _customLogger.Log($"Token refresh SUCCESS for {request.Username}");
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));

            var claims = new[]
            {
        new Claim(ClaimTypes.Name, request.Username),
        new Claim(ClaimTypes.Role, request.Role)
    };

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var newToken = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(newToken)
            });
        }

    }

}
