using DocumentFormat.OpenXml.Spreadsheet;
using EduCraftAPI.Data;
using EduCraftAPI.Models;
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
            int pageSize = 12,
            string? type = "presentation"
        )
        {

            PaginatedResult data;

            if(type== "presentation")
            {
                data = getPresentation(phrase, startDate, endDate, category, page, phraseSort, categorySort, dateSort, pageSize, type);
                if (!data.Items.Any())
                {
                    return NoContent();
                }
            }
            else if (type == "quiz")
            {
                data = getQuiz(phrase, startDate, endDate, category, page, phraseSort, categorySort, dateSort, pageSize, type);
                if (!data.Items.Any())
                {
                    return NoContent();
                }
            }
            else if (type == "flashcard")
            {
                data = getFlashcard(phrase, startDate, endDate, category, page, phraseSort, categorySort, dateSort, pageSize, type);
                if (!data.Items.Any())
                {
                    return NoContent();
                }
            }
            else
            {
                return NoContent();
            }
            return Ok(new
            {
                TotalCount = data.TotalCount,
                Page = page,
                PageSize = pageSize,
                Items = data.Items,
            });
        }

        private PaginatedResult getFlashcard(
         string? phrase = null,
         DateTime? startDate = null,
         DateTime? endDate = null,
         int? category = null,
         int page = 1,
         string? phraseSort = null,
         string? categorySort = null,
         string? dateSort = null,
         int pageSize = 12,
         string? type = "presentation"
     )
        {
            var query = _context.Flashcards.AsQueryable();
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

            var items = query.Skip((page - 1) * pageSize).Take(pageSize).Select(p => new SearchDto()
            {
                ItemID = p.FlashcardsID,
                Title = p.Title,
                FirstName = p.User.FirstName,
                LastName = p.User.LastName,
                CreationDate = p.CreationDate,
                CategoryName = p.Category.Name,
                Type = type
            }).ToList();

            var data = new PaginatedResult()
            {
                Items = items,
                TotalCount = totalCount,
            };
            return data;
        }

        private PaginatedResult getQuiz(
         string? phrase = null,
         DateTime? startDate = null,
         DateTime? endDate = null,
         int? category = null,
         int page = 1,
         string? phraseSort = null,
         string? categorySort = null,
         string? dateSort = null,
         int pageSize = 12,
         string? type = "presentation"
     )
        {
            var query = _context.Quizzes.AsQueryable();
            query = query.Where(p => p.IsPublic == true);

            if (!string.IsNullOrWhiteSpace(phrase))
            {
                query = query.Where(p => p.Name.Contains(phrase));
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
                query = phraseSort == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name);
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

            var items = query.Skip((page - 1) * pageSize).Take(pageSize).Select(p => new SearchDto()
            {
                ItemID = p.QuizID,
                Title = p.Name,
                FirstName = p.User.FirstName,
                LastName = p.User.LastName,
                CreationDate = p.CreationDate,
                CategoryName = p.Category.Name,
                Type = type
            }).ToList();

            var data = new PaginatedResult()
            {
                Items = items,
                TotalCount = totalCount,
            };
            return data;
        }

        private PaginatedResult getPresentation(
            string? phrase = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? category = null,
            int page = 1,
            string? phraseSort = null,
            string? categorySort = null,
            string? dateSort = null,
            int pageSize = 12,
            string? type = "presentation"
        ){
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

            var items = query.Skip((page - 1) * pageSize).Take(pageSize).Select(p => new SearchDto()
            {
                ItemID = p.PresentationsID,
                Title = p.Title,
                FirstName = p.User.FirstName,
                LastName = p.User.LastName,
                CreationDate = p.CreationDate,
                CategoryName = p.Category.Name,
                Type = type
            }).ToList();

            var data = new PaginatedResult()
            {
                Items = items,
                TotalCount = totalCount,
            };
            return data;
        }
        [HttpGet("/category")]
        public IActionResult GetFlashcards()
        {
            var categories = _context.Category
                .OrderBy(c => c.Name) 
                .ToList();

            if (categories == null)
            {
                return NoContent();
            }
            return Ok(categories);
        }
    }
}