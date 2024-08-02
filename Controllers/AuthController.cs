using EduCraftAPI.Data;
using EduCraftAPI.Entities.User;
using EduCraftAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EduCraftAPI.Controllers
{
    [Route("api/account")]
    public class AuthController : Controller
    {
        private readonly DataDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly AuthenticationSettings _authenticationSettings;
        public AuthController(DataDbContext dataDbContext, IPasswordHasher<User> passwordHasher, AuthenticationSettings authenticationSettings) {
            _context = dataDbContext;
            _passwordHasher = passwordHasher;
            _authenticationSettings = authenticationSettings;
        }


        [HttpPost("register")]
        public ActionResult Register([FromBody] RegisterUserDto registerUserDto)
        {
            if (registerUserDto == null || string.IsNullOrWhiteSpace(registerUserDto.Username) ||
                string.IsNullOrWhiteSpace(registerUserDto.Password) ||
                string.IsNullOrWhiteSpace(registerUserDto.FirstName) ||
                string.IsNullOrWhiteSpace(registerUserDto.LastName))
            {
                return BadRequest("All fields are required.");
            }

            var existingUser = _context.Users.FirstOrDefault(u => u.Username == registerUserDto.Username);
            if (existingUser != null)
            {
                return Conflict("Username or password already exists.");
            }

            User newUser = new User()
            {
                Username = registerUserDto.Username,
                FirstName = registerUserDto.FirstName,
                LastName = registerUserDto.LastName,
                RoleID = registerUserDto.ID,
            };

            var hashedPassword = _passwordHasher.HashPassword(newUser, registerUserDto.Password);
            newUser.Password = hashedPassword;

            try
            {
                _context.Users.Add(newUser);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }

            return Ok("User registered successfully.");
        }


        [HttpPost("login")]
        public ActionResult Login([FromBody] LoginDto loginDto)
        {
            var user = _context.Users.Include(u=>u.Role).FirstOrDefault(u => u.Username == loginDto.Username);
            if (user is null)
            {
                return Unauthorized("Invalid username or password.");

            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, loginDto.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Invalid username or password.");

            }

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.FirstName),
                new Claim(ClaimTypes.Role, user.Role.Name),
                new Claim("LastName", user.LastName)

            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationSettings.JwtKey));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(_authenticationSettings.JwtExpireDays);

            var token = new JwtSecurityToken(_authenticationSettings.JwtIssuer, _authenticationSettings.JwtIssuer, claims, expires: expires, signingCredentials: cred);
                var tokenHandler = new JwtSecurityTokenHandler();
            return Ok(new { token = tokenHandler.WriteToken(token) });
        }

    }
}
