using DocumentFormat.OpenXml.Wordprocessing;
using EduCraftAPI.Data;
using EduCraftAPI.Entities.Quiz;
using EduCraftAPI.Models;
using EduCraftAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCraftAPI.Controllers
{
    [Authorize(Roles = "Admin",Policy = "IsBlock")]
    public class AdminController : Controller
    {
        private readonly DataDbContext _context;
        private readonly IUserContextService _userContextService;

        public AdminController(DataDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        [HttpPost("/admin/user/update")]
        public IActionResult updatePresentation([FromBody] IsBlockedDTO isBlockedDTO)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserID == isBlockedDTO.userID);
            if (user == null)
            {
                return NoContent();
            }
            try
            {
                user.RoleID = isBlockedDTO.roleID;
                user.IsBlocked = isBlockedDTO.IsBlocked;
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wewnętrzny błąd serwera.");
            }
            return Ok(user);
        }

        [HttpGet("/admin/search/documents")]
        public IActionResult adminSearch(
           string? name = null,
           string? type = "presentation",
           string? username = null
        )
        {

            if(type == "presentation")
            {
                var query = _context.Presentation.AsQueryable();
                query = query.Where(p => p.IsPublic == true);

                if (!string.IsNullOrWhiteSpace(name))
                {
                    query = query.Where(p => p.Title.Contains(name));
                }
                if (!string.IsNullOrWhiteSpace(username))
                {
                    query = query.Where(p => (p.User.FirstName + " " +p.User.LastName).Contains(username));
                }

                var items = query.Select(p => new
                {
                    ItemID = p.PresentationsID,
                    Title = p.Title,
                    FirstName = p.User.FirstName,
                    LastName = p.User.LastName,
                    UserID = p.User.UserID,
                    Type = type,
                }).ToList();

                return Ok(items);
            }
            else if(type == "quiz")
            {
                var query = _context.Quizzes.AsQueryable();
                query = query.Where(p => p.IsPublic == true);

                if (!string.IsNullOrWhiteSpace(name))
                {
                    query = query.Where(p => p.Name.Contains(name));
                }
                if (!string.IsNullOrWhiteSpace(username))
                {
                    query = query.Where(p => (p.User.FirstName + " " + p.User.LastName).Contains(username));
                }

                var items = query.Select(p => new
                {
                    ItemID = p.QuizID,
                    Title = p.Name,
                    FirstName = p.User.FirstName,
                    LastName = p.User.LastName,
                    UserID = p.User.UserID,
                    Type = type,
                }).ToList();

                return Ok(items);

            }
            else if(type == "flashcard")
            {
                var query = _context.Flashcards.AsQueryable();
                query = query.Where(p => p.IsPublic == true);

                if (!string.IsNullOrWhiteSpace(name))
                {
                    query = query.Where(p => p.Title.Contains(name));
                }
                if (!string.IsNullOrWhiteSpace(username))
                {
                    query = query.Where(p => (p.User.FirstName + " " + p.User.LastName).Contains(username));
                }

                var items = query.Select(p => new
                {
                    ItemID = p.FlashcardsID,
                    Title = p.Title,
                    FirstName = p.User.FirstName,
                    LastName = p.User.LastName,
                    UserID = p.User.UserID,
                    Type = type,
                }).ToList();

                return Ok(items);
            }
            else
            {
                return NoContent();
            }         
        }

        [HttpGet("/admin/search/users")]
        public IActionResult adminSearchUsers(
          string? username = null,
          string? email = null,
          int? id = null

        )
        {
            var query = _context.Users.AsQueryable();
            query.Include(r => r.Role);
            query = query.Where(p => p.UserID != _userContextService.GetUserID);
            if (id != null)
            {
                query = query.Where(p => p.UserID == id);
            }
            if (!string.IsNullOrWhiteSpace(username))
            {
                query = query.Where(p => (p.FirstName + " " + p.LastName).Contains(username));
            }
            if (!string.IsNullOrWhiteSpace(email))
            {
                query = query.Where(p => p.Email.Contains(email));
            }
            var items = query.Select(p => new
            {
                Email = p.Email,
                Role = p.Role.Name,
                RoleID = p.RoleID,
                FirstName = p.FirstName,
                LastName = p.LastName,
                UserID = p.UserID,
                IsBlocked = p.IsBlocked
            }).ToList();

            return Ok(items);
          
        }


        [HttpDelete("admin/delete/item")]
        public async Task<IActionResult> deleteItem([FromQuery] int itemID, string type)
        {
            if (type == "presentation")
            {
                var query = _context.Presentation.FirstOrDefault(i=>i.PresentationsID == itemID);
                if (query == null)
                {
                    return NoContent();
                }
                return Redirect($"/presentation/delete?id={query.PresentationsID}");

            }
            else if (type == "quiz")
            {
                var query = _context.Quizzes.FirstOrDefault(i => i.QuizID == itemID);

                if (query == null)
                {
                    return NoContent();
                }


                return Redirect($"/quiz/delete?id={query.QuizID}");
            }
            else if (type == "flashcard")
            {
                var query = _context.Flashcards.FirstOrDefault(i => i.FlashcardsID == itemID);
                if (query == null)
                {
                    return NoContent();
                }
                return Redirect($"/flashcards/delete?id={query.FlashcardsID}");

            }
            return Ok();
        }
    }
}
