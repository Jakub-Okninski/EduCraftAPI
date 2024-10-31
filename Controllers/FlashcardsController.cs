using EduCraftAPI.Data;
using EduCraftAPI.Entities.Flashcards;
using EduCraftAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCraftAPI.Controllers
{
    [Authorize]
    public class FlashcardsController : Controller
    {
        private readonly DataDbContext _context;
        public FlashcardsController(DataDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles= "User,Admin")]
        [HttpGet("/flashcards")]
        public IActionResult GetFlashcards([FromQuery] int UserID)
        {
            var flashcards = _context.Flashcards
                .Where(p => p.User.UserID == UserID)
                .ToList(); 
            if (flashcards == null)
            {
                return NoContent();
            }
            return Ok(flashcards);
        }

        [HttpPut("/flashcard/edit")]
        public async Task<IActionResult> UpdateFlashcard([FromBody] Flashcard updatedFlashcard)
        {
            var flashcard = _context.Flashcard.FirstOrDefault(u => u.FlashcardID == updatedFlashcard.FlashcardID);

            if (flashcard == null)
            {
                return NoContent();
            }
            flashcard.Title = updatedFlashcard.Title;
            flashcard.Description = updatedFlashcard.Description;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Błąd podczas zapisywania prezentacji: {ex.Message}");
            }

            return Ok();
        }
        [AllowAnonymous]
        [HttpGet("/play/flashcards")]
        public IActionResult GetFlashcardsOnID([FromQuery] int FlashcardID)
        {
            var flashcards = _context.Flashcards
                 .Include(p => p.Flashcard)
                .FirstOrDefault(p => p.FlashcardsID == FlashcardID);

            if (flashcards == null)
            {
                return NoContent();
            }
            return Ok(flashcards);
        }



        [HttpGet("/flashcard")]
        public IActionResult GetFlashcard([FromQuery] int UserID, int FlashcardID)
        {
            var flashcards = _context.Flashcards
                 .Include(p => p.Flashcard)
                .FirstOrDefault(p => p.FlashcardsID == FlashcardID && p.User.UserID == UserID);
           
            if (flashcards == null)
            {
                return NoContent();
            }
            return Ok(flashcards);
        }
        [HttpPost("/flashcard/create")]
       
        public IActionResult CreateFlashcard([FromBody] CreateFlashcardDto createFlashcard)
        {
            if (createFlashcard == null)
            {
                return BadRequest("Nieprawidłowe dane.");
            }

            var flashcards = _context.Flashcards.FirstOrDefault(u => u.FlashcardsID == createFlashcard.FlashcardsId);
            if (flashcards == null)
            {
                return NoContent();
            }

            var flashcar = new Flashcard()
            {
                Title = createFlashcard.Title,
                Description = createFlashcard.Description
            };
            _context.Flashcard.Add(flashcar);
            if (flashcards.Flashcard == null)
            {
                flashcards.Flashcard = new List<Flashcard>();
            }
            flashcards.Flashcard.Add(flashcar);
            _context.SaveChanges();
            return Ok(flashcar);
        }


        [HttpDelete("/flashcard/remove")]
        public IActionResult RemoveFlashcards([FromQuery] int flashcardsID)
        {
            var flashcard = _context.Flashcard.FirstOrDefault(f => f.FlashcardID == flashcardsID);

            if (flashcard == null)
            {
                return NoContent();
            }

            _context.Flashcard.Remove(flashcard);

            _context.SaveChanges();

            return Ok("Flashcard zostało pomyślnie usunięte.");
        }


        [HttpPost("/flashcards/create")]
        public IActionResult CreateFlashcards([FromBody] CreateFlashcardsDto createFlashcardsDto)
        {
            if (createFlashcardsDto == null)
            {
                return BadRequest("Nieprawidłowe dane.");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserID == createFlashcardsDto.UserId);
            if (user == null)
            {
                return NoContent();
            }

            var flashcards = new Flashcards
            {
                User = user,
                Title = createFlashcardsDto.Title,
                Flashcard = new List<Flashcard>()
            };

            if (createFlashcardsDto?.Flashcards != null)
            {
                foreach (var cardDto in createFlashcardsDto.Flashcards)
                {
                    var flashcard = new Flashcard
                    {
                        Title = cardDto.Title,
                        Description = cardDto.Description
                    };
                    flashcards.Flashcard.Add(flashcard);
                }
            }
            _context.Flashcards.Add(flashcards);
            _context.SaveChanges();
            return Ok(flashcards);
        }
    }
}
