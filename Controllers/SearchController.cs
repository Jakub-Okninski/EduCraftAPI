using EduCraftAPI.Data;
using Microsoft.AspNetCore.Mvc;

namespace EduCraftAPI.Controllers
{
    public class SearchController : Controller
    {
        private readonly DataDbContext _context;
        public SearchController(DataDbContext context)
        {
            _context = context;
        }
        [HttpGet("/search")]
        public IActionResult search(
            string? phrase = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? category = null,
            int page = 1,
            string? phraseSort = null,
            string? categorySort = null,
            string? dateSort = null,
            int pageSize = 12
        )
        {
            var query = _context.Presentation.AsQueryable();
            query = query.Where(p => p.IsPublic == true);

            if (!string.IsNullOrWhiteSpace(phrase))
            {
                query = query.Where(p => p.Title.Contains(phrase));
            }

            if (startDate != null)
            {
                query = query.Where(p => p.CreationDate >= startDate.Value);
            }

            if (endDate != null)
            {
                query = query.Where(p => p.CreationDate <= endDate.Value);
            }

            if (category != null)
            {
                query = query.Where(p => p.Category.CategoryID == category.Value);
            }

            if (!string.IsNullOrWhiteSpace(phraseSort))
            {
                query = phraseSort == "asc" ? query.OrderBy(p => p.Title) : query.OrderByDescending(p => p.Title);
            }
            else if (!string.IsNullOrWhiteSpace(categorySort))
            {
                query = categorySort == "asc" ? query.OrderBy(p => p.Category.Name) : query.OrderByDescending(p => p.Category.Name);
            }
            else if (!string.IsNullOrWhiteSpace(dateSort))
            {
                query = dateSort == "asc" ? query.OrderBy(p => p.CreationDate) : query.OrderByDescending(p => p.CreationDate);
            }

   
            var totalCount = query.Count(); 
      
            var presentations = query.Skip((page - 1) * pageSize).Take(pageSize).Select(p => new
            {
                PresentationsID = p.PresentationsID,
                Title = p.Title,
                FirstName = p.User.FirstName,
                CreationDate = p.CreationDate,
                CategoryName = p.Category.Name,
                Type = "Presentation"
            }).ToList();

            if (!presentations.Any())
            {
                return NoContent();
            }
            return Ok(new
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Items = presentations
            });
        }

        [HttpGet("/category")]
        public IActionResult GetFlashcards()
        {
            var categories = _context.Category.ToList();

            if (categories == null)
            {
                return NoContent();
            }
            return Ok(categories);
        }
    }
}