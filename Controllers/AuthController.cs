using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using EduCraftAPI.Data;
using EduCraftAPI.Entities.User;
using EduCraftAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EduCraftAPI.Controllers
{
    [Route("account")]
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


        [Authorize]
        [HttpGet("/user")]
        public IActionResult getUser([FromQuery] int userID)
        {

            var user = _context.Users
                .Where(u => u.UserID == userID)
                .Select(u => new
                {
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.Role.Name,
         
                })
                .FirstOrDefault(); 
            
            if (user == null)
            {
                return NotFound("Brak użytkownika.");
            }
         
            return Ok(user);
        }
        [Authorize]
        [HttpGet("/user/statistic")]
        public IActionResult getUserStatistic([FromQuery] int userID)
        {
            var user = _context.Users.FirstOrDefault(p => p.UserID == userID);

            if (user == null)
            {
                return NotFound("Brak użytkownika.");
            }

            var PresentationCount = _context.Presentation.Count(p => p.UserID == userID);
            var QuizzesCount = _context.Quizzes.Count(p => p.UserID == userID);
            var FlashcardsCount = _context.Flashcards.Count(p => p.UserID == userID);


          

            return Ok(new
            {
                PresentationCount,
                QuizzesCount,
                FlashcardsCount
            });
        }

        [Authorize]
        [HttpPost("/user/password/change")]
        public IActionResult userPassword([FromBody] UserDTO userDTO)
        {
            var user = _context.Users.Include(u => u.Role).FirstOrDefault(u => u.UserID == userDTO.UserID);
            if (user is null)
            {
                return Unauthorized("Brak autoryzacji");

            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, userDTO.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Brak autoryzacji");

            }
            var hashedPassword = _passwordHasher.HashPassword(user, userDTO.NewPassword);
            user.Password = hashedPassword;

            try
            {       
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wewnętrzny błąd serwera.");
            }
            return Ok();
        }

        [HttpPost("register")]
        public ActionResult Register([FromBody] RegisterUserDto registerUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUser = _context.Users.FirstOrDefault(u => u.Email == registerUserDto.Email);
            if (existingUser != null)
            {
                return Conflict("Konto o takim adresie e-mail już istnieje.");
            }

            User newUser = new User()
            {
                Email = registerUserDto.Email,
                FirstName = registerUserDto.FirstName,
                LastName = registerUserDto.LastName,
                RoleID = _context.Roles.FirstOrDefault(u => u.Name == "User").RoleID
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
                return StatusCode(500, "Wewnętrzny błąd serwera.");
            }

            return Created();
        }

        [HttpPost("login")]
        public ActionResult Login([FromBody] LoginDto loginDto)
        {
            var user = _context.Users.Include(u=>u.Role).FirstOrDefault(u => u.Email == loginDto.Username);
            if (user is null)
            {
                return Unauthorized("Nieprawidłowa nazwa użytkownika lub hasło.");

            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, loginDto.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Nieprawidłowa nazwa użytkownika lub hasło.");

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
            return Ok(new { token = tokenHandler.WriteToken(token) , name = user.FirstName, id= user.UserID.ToString(), role=user.Role.Name });
        }
    }
}