using EduCraftAPI.Data;
using EduCraftAPI.Entities.Flashcards;
using EduCraftAPI.Models;
using EduCraftAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IO;

namespace EduCraftAPI.Controllers
{
    [Authorize]
    public class FlashcardsController : Controller
    {
        private readonly DataDbContext _context;
        private readonly IUserContextService _userContextService;
        private readonly IFileService _fileServices;
        private readonly IDocumentService _documentService;

        public FlashcardsController(DataDbContext context, IUserContextService userContextService, IFileService fileServices, IDocumentService documentService)
        {
            _context = context;
            _userContextService = userContextService;
            _fileServices = fileServices;
            _documentService= documentService; 
        }

        [Authorize(Roles= "User,Admin")]
        [HttpGet("/flashcards")]
        public IActionResult GetFlashcards()
        {
            var flashcards = _context.Flashcards
                .Where(p => p.User.UserID == _userContextService.GetUserID)
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
                 _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Błąd podczas zapisywania prezentacji: {ex.Message}");
            }

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("/flashcards/play")]
        public IActionResult GetFlashcardsOnID([FromQuery] int FlashcardID)
        {
            var flashcards = _context.Flashcards
                .Include(p => p.Flashcard)
                .FirstOrDefault(p => p.FlashcardsID == FlashcardID);

            if (flashcards == null)
            {
                return NoContent();
            }
            return Ok(_fileServices.AddFlashCardsImg(flashcards));
        }


        [AllowAnonymous]
        [HttpGet("/flashcards/generate")]
        public IActionResult GenerateFlashcardsOnID([FromQuery] int FlashcardID)
        {
            var flashcards = _context.Flashcards
                .Include(p => p.Flashcard)
                .FirstOrDefault(p => p.FlashcardsID == FlashcardID);

            if (flashcards == null)
            {
                return NoContent();
            }


            var stream = _documentService.GenerateFlashcards(flashcards);
            return File(stream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "flashcards_" + flashcards.Title + ".docx");

        }


        [HttpGet("/flashcard")]
        public IActionResult GetFlashcard([FromQuery] int FlashcardID)
        {
            var flashcards = _context.Flashcards
                .Include(p => p.Flashcard)
                .FirstOrDefault(p => p.FlashcardsID == FlashcardID && p.User.UserID == _userContextService.GetUserID);

            if (flashcards == null)
            {
                return NoContent();
            }
            return Ok(_fileServices.AddFlashCardsImg(flashcards));
        }
        [HttpPost("/flashcard/create")]
       
        public IActionResult CreateFlashcard([FromForm] CreateFlashcardDto createFlashcard)
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
            Flashcard flashcard = new Flashcard();

            if (createFlashcard.File != null && createFlashcard.File.Length != 0)
            {
                try
                {
                    flashcard.FileName = _fileServices.SaveFileImgFlashCard((int)_userContextService.GetUserID, flashcards.FlashcardsID, createFlashcard.File);
                    flashcard.FileContent = _fileServices.getBase64(createFlashcard.File);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Błąd podczas zapisywania prezentacji: {ex.Message}");
                }
            }
            flashcard.Description = createFlashcard.Description;
            flashcard.Title = createFlashcard.Title;
            if (flashcards.Flashcard == null)
            {
                flashcards.Flashcard = new List<Flashcard>();
            }
            flashcards.Flashcard.Add(flashcard);
            _context.SaveChanges();
            return Ok(flashcard);
        }

        [HttpPut("/edit/flashcard/image")]
        public IActionResult editImageFlashcard([FromForm] FileDTO fileDTO)
        {
            var flashcard = _context.Flashcard
                .FirstOrDefault(p => p.FlashcardID == fileDTO.ID);
            if (flashcard == null)
            {
                return NoContent();
            }
           
            if (flashcard.FileName != null)
            {
                try
                {
                    _fileServices.RemoveImageFlashCard((int)_userContextService.GetUserID, flashcard.FlashcardsID, flashcard.FileName);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Błąd: {ex.Message}");
                }
            }

            if (fileDTO.File != null && fileDTO.File.Length != 0)
            {
                try
                {
                    flashcard.FileName = _fileServices.SaveFileImgFlashCard((int)_userContextService.GetUserID, flashcard.FlashcardsID, fileDTO.File);
                    flashcard.FileContent = _fileServices.getBase64(fileDTO.File);
                }
                catch (Exception ex)
                {
                    if(flashcard.FileName != null) 
                    {
                        _fileServices.RemoveImageFlashCard((int)_userContextService.GetUserID, flashcard.FlashcardsID, flashcard.FileName);
                        flashcard.FileName = null;
                    }
                    return StatusCode(500, $"Błąd: {ex.Message}");
                }
                _context.SaveChanges();
            }
            return Ok(flashcard);
        }



        [HttpDelete("/flashcard/remove/image")]
        public IActionResult removeImageQuiz([FromQuery] int flashcardID)
        {
            var flashcard = _context.Flashcard
                .FirstOrDefault(p => p.FlashcardID == flashcardID);
            if (flashcard == null || flashcard.FileName==null)
            {
                return NoContent();
            }

            try
            {
                _fileServices.RemoveImageFlashCard((int)_userContextService.GetUserID, flashcard.FlashcardsID, flashcard.FileName);
                flashcard.FileName = null;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Błąd: {ex.Message}");
            }
            return Ok(new { message = "Plik został usunięty." });

        }

        [HttpDelete("/flashcard/remove")]
        public IActionResult RemoveFlashcards([FromQuery] int flashcardsID)
        {
            var flashcard = _context.Flashcard.FirstOrDefault(f => f.FlashcardID == flashcardsID);

            if (flashcard == null)
            {
                return NoContent();
            }

            if (flashcard.FileName != null)
            {
                try
                {
                    _fileServices.RemoveImageFlashCard((int)_userContextService.GetUserID, flashcard.FlashcardsID, flashcard.FileName);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Błąd: {ex.Message}");
                }
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
    
            var flashcards = new Flashcards
            {
                UserID = (int)_userContextService.GetUserID,
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
