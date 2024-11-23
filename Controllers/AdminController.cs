using DocumentFormat.OpenXml.Wordprocessing;
using EduCraftAPI.Data;
using EduCraftAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCraftAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly DataDbContext _context;
        public AdminController(DataDbContext context)
        {
            _context = context;
        }

        [HttpGet("/admin/search")]
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
                    Type= type,
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
                    Type = type,
                }).ToList();

                return Ok(items);
            }
            else
            {
                return NoContent();
            }         
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
