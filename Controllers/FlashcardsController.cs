using EduCraftAPI.Data;
using EduCraftAPI.Entities.Flashcards;
using EduCraftAPI.Entities.Quiz;
using EduCraftAPI.Models;
using EduCraftAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace EduCraftAPI.Controllers
{
    [Authorize]
    public class FlashcardsController : Controller
    {
      
        private readonly DataDbContext _context;
        private readonly IUserContextService _userContextService;

        public FlashcardsController(DataDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
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
                 .Include(p=>p.User)
                .FirstOrDefault(p => p.FlashcardsID == FlashcardID);

            if (flashcards == null)
            {
                return NoContent();
            }

            foreach (Flashcard flashcard in flashcards?.Flashcard)
            {
                Debug.WriteLine(" ");
                Debug.WriteLine(" dasaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaadsadas");
                Debug.WriteLine(" ");
                if (!string.IsNullOrEmpty(flashcard.FileName))
                {
                    Debug.WriteLine(" ");
                    Debug.WriteLine(" dupaaaa "); Debug.WriteLine(" ");
                    var filePath = Path.Combine("UserDataImage", "User" + flashcards.UserID , "Flashcard" + flashcards.FlashcardsID, flashcard.FileName);
                    Debug.WriteLine(filePath);


                    if (System.IO.File.Exists(filePath))
                    {
                        Debug.WriteLine("exxx...");

                        var fileBytes = System.IO.File.ReadAllBytes(filePath);
                        string base64String = Convert.ToBase64String(fileBytes);
                        flashcard.FileContent = $"data:image/jpeg;base64,{base64String}";
                    }
                    else
                    {
                        flashcard.FileContent = null;
                    }
                }
            }
            return Ok(flashcards);
        }



        [HttpGet("/flashcard")]
        public IActionResult GetFlashcard([FromQuery] int UserID, int FlashcardID)
        {
            Debug.WriteLine(" ");
            Debug.WriteLine(" ok");
            Debug.WriteLine(" ");
            var flashcards = _context.Flashcards
                 .Include(p => p.Flashcard)
                .FirstOrDefault(p => p.FlashcardsID == FlashcardID && p.User.UserID == UserID);

            Debug.WriteLine(" ");
            Debug.WriteLine(" dasdsadas");
            Debug.WriteLine(" ");

            if (flashcards == null)
            {
                return NoContent();
            }
    

            foreach (Flashcard flashcard in flashcards?.Flashcard)
            {
                Debug.WriteLine(" ");
                Debug.WriteLine(" dasaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaadsadas");
                Debug.WriteLine(" ");
                if (!string.IsNullOrEmpty(flashcard.FileName))
                {
                    Debug.WriteLine(" ");
                    Debug.WriteLine(" dupaaaa "); Debug.WriteLine(" ");
                    var filePath = Path.Combine("UserDataImage", "User" + _userContextService.GetUserID, "Flashcard" + flashcards.FlashcardsID, flashcard.FileName);
                    Debug.WriteLine(filePath);


                    if (System.IO.File.Exists(filePath))
                    {
                        Debug.WriteLine("exxx...");

                        var fileBytes = System.IO.File.ReadAllBytes(filePath);
                        string base64String = Convert.ToBase64String(fileBytes);
                        flashcard.FileContent = $"data:image/jpeg;base64,{base64String}";
                    }
                    else
                    {
                        flashcard.FileContent = null;
                    }
                }
            }

                return Ok(flashcards);
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
            Flashcard flashcar = new Flashcard();

            if (createFlashcard.File != null && createFlashcard.File.Length != 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "UserDataImage", "User" + _userContextService.GetUserID);
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                uploadsFolder = Path.Combine(uploadsFolder, "Flashcard" + flashcards.FlashcardsID);
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }


                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(createFlashcard.File.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    createFlashcard.File.CopyTo(stream);

                }
                using (var memoryStream = new MemoryStream())
                {
                    createFlashcard.File.CopyTo(memoryStream);

                    byte[] fileBytes = memoryStream.ToArray();

                    string base64String = Convert.ToBase64String(fileBytes);

                    flashcar.FileContent = $"data:image/jpeg;base64,{base64String}";
                }
                flashcar.FileName = fileName;
            }
                flashcar.Title = createFlashcard.Title;
                flashcar.Description = createFlashcard.Description;
            flashcar.Flashcards = flashcards;


            if (flashcards.Flashcard == null)
            {
                flashcards.Flashcard = new List<Flashcard>();
            }
            flashcards.Flashcard.Add(flashcar);
            _context.SaveChanges();
            return Ok(flashcar);
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
                var filePath = Path.Combine("UserDataImage", "User" + _userContextService.GetUserID, "Flashcard" + flashcard.FlashcardsID, flashcard.FileName);

                // Sprawdzanie, czy plik istnieje
                if (System.IO.File.Exists(filePath))
                {
                    // Usuwanie pliku
                    System.IO.File.Delete(filePath);
                }
            }



            if (fileDTO.File != null && fileDTO.File.Length != 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "UserDataImage", "User" + _userContextService.GetUserID, "Flashcard" + flashcard.FlashcardsID);




                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(fileDTO.File.FileName);
                var filePath2 = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath2, FileMode.Create))
                {
                    fileDTO.File.CopyTo(stream);

                }
                using (var memoryStream = new MemoryStream())
                {
                    // Kopiujemy plik do MemoryStream
                    fileDTO.File.CopyTo(memoryStream);

                    // Konwertujemy zawartość MemoryStream na tablicę bajtów
                    byte[] fileBytes = memoryStream.ToArray();

                    // Konwertujemy tablicę bajtów na Base64
                    string base64String = Convert.ToBase64String(fileBytes);

                    // Tworzymy pełny ciąg Base64 z odpowiednim prefiksem
                    flashcard.FileContent = $"data:image/jpeg;base64,{base64String}";
                }



                flashcard.FileName = fileName;
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
            // Skonstruowanie pełnej ścieżki pliku
            var filePath = Path.Combine("UserDataImage", "User" + _userContextService.GetUserID, "Flashcard" + flashcard.FlashcardsID, flashcard.FileName);

            // Sprawdzanie, czy plik istnieje
            if (System.IO.File.Exists(filePath))
            {
                // Usuwanie pliku
                System.IO.File.Delete(filePath);
                flashcard.FileName = null;
                _context.SaveChanges();
                // Zwracamy odpowiedź o sukcesie
                return Ok(new { message = "Plik został usunięty." });
            }
            else
            {
                return NoContent();
            }


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
                var filePath = Path.Combine("UserDataImage", "User" + _userContextService.GetUserID, "Flashcard" + flashcard.FlashcardsID, flashcard.FileName);

                // Sprawdzanie, czy plik istnieje
                if (System.IO.File.Exists(filePath))
                {
                    //  pliku
                    System.IO.File.Delete(filePath);

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
