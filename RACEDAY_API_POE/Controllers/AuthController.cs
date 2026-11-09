using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RACEDAY_API_POE.Data;
using RACEDAY_API_POE.DTOs;
using RACEDAY_API_POE.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RACEDAY_API_POE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        // GET: api/Auth
        private readonly RaceDayDatabaseContext _context;
        private readonly IConfiguration _configuration;
        // Constructor to inject the database context and configuration
        public AuthController(RaceDayDatabaseContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            string email = dto.Email.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(dto.FirstName) ||
                string.IsNullOrWhiteSpace(dto.LastName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { message = "Name, email and password are required." });
            }

            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                return Conflict(new { message = "An account with this email already exists." });
            }

            // 1. Create the base User
            var user = new User
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = email,
                // Hash the password. Never store plain text!
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, 12)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(); // Saves and generates the UserId

            // 2. Create the specific role profile
            if (dto.Role.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
            {
                var organiser = new Organiser
                {
                    UserId = user.UserId,
                    CompanyName = $"{user.FirstName}'s Events", // Default company name
                    ContactPhone = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? "N/A" : dto.PhoneNumber.Trim()
                };
                _context.Organisers.Add(organiser);
            }
            else
            {
                var participant = new Participant
                {
                    UserId = user.UserId,
                    DateOfBirth = dto.DateOfBirth ?? new DateTime(2000, 1, 1), // Default if not provided
                    Gender = string.IsNullOrWhiteSpace(dto.Gender) ? "Other" : dto.Gender.Trim()
                };
                _context.Participants.Add(participant);
            }

            await _context.SaveChangesAsync(); // Saves the role profile

            return StatusCode(201, new
            {
                message = "Registration successful.",
                userId = user.UserId,
                role = dto.Role
            });
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            string email = dto.Email.Trim().ToLowerInvariant();

            var user = await _context.Users
                .Include(u => u.Organiser)
                .Include(u => u.Participant)
                .FirstOrDefaultAsync(u => u.Email == email);

            // Check if user exists and password matches
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            // Generate JWT Token
            string token = GenerateJwtToken(user);

            // Determine the role string
            string role = user.Organiser != null ? "Organiser" : "Participant";

            return Ok(new
            {
                message = "Login successful.",
                token,
                tokenType = "Bearer",
                expiresInMinutes = GetJwtExpiryMinutes(),
                user = new
                {
                    user.UserId,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    Role = role
                }
            });
        }



        private string GenerateJwtToken(User user)
        {
            string key = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT key is missing from configuration.");

            string issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("JWT issuer is missing from configuration.");

            string audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("JWT audience is missing from configuration.");

            if (Encoding.UTF8.GetByteCount(key) < 32)
            {
                throw new InvalidOperationException("The JWT key must be at least 32 bytes for HS256.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            //Setup base claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
            };

            //Add the Role Claim based on their profile
            if (user.Organiser != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Organiser"));
            }
            else if (user.Participant != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Participant"));
            }

            //Create the token
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(GetJwtExpiryMinutes()),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private int GetJwtExpiryMinutes()
        {
            int minutes = _configuration.GetValue<int?>("Jwt:ExpiryMinutes") ?? 60;
            if (minutes <= 0)
            {
                throw new InvalidOperationException("JWT expiry must be greater than zero.");
            }
            return minutes;
        }
    }
}
