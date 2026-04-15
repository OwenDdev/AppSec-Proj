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
                var refreshToken = Guid.NewGuid().ToString();

                if (user == null)
                    return Unauthorized("Invalid Username or password");

                bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

                if (!isValid)
                    return Unauthorized("Invalid Username or password");

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
        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshRequest request)
        {
            // VERY SIMPLE VERSION (assignment level)
            // Normally you'd validate against DB

            if (string.IsNullOrEmpty(request.RefreshToken))
                return Unauthorized();

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
