using EduCraftAPI.Data;
using EduCraftAPI.Entities.User;
using EduCraftAPI.Entities.Presentation;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Xml.Serialization;
using EduCraftAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EduCraftAPI.Controllers
{
    public class PresentationController : Controller
    {
        private readonly DataDbContext _context;
        public PresentationController(DataDbContext context)
        {
            _context = context;
        }
        public void SavePresentationToXml(Presentation presentation, string filePath){
            var xmlSerializer = new XmlSerializer(typeof(Presentation));
            using (var stream = new FileStream("Presentations\\" + filePath, FileMode.Create))
            using (var writer = new StreamWriter(stream))
            {
                xmlSerializer.Serialize(writer, presentation);
            }
        }
      
        public Presentation LoadPresentationFromXml(string filePath) {
            var xmlSerializer = new XmlSerializer(typeof(Presentation));
            using (var stream = new FileStream("Presentations\\"+filePath, FileMode.Open))
            using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
            {
                return (Presentation)xmlSerializer.Deserialize(reader);
            }
        }


        [HttpGet("/presentation")]
        public IActionResult GetPresentation([FromQuery] int presentationId)
        {

            var presentation = _context.Presentation.FirstOrDefault(p => p.PresentationsID == presentationId);
            if (presentation == null)
            {
                return NotFound("Prezentacja nie istnieje.");
            }

            return Ok(LoadPresentationFromXml(""+presentation.PresentationsID));
        }

        [HttpGet("/presentations")] 
        public IActionResult GetPresentationsByUser([FromQuery] int userId) 
        {


            var user = _context.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null)
            {
                return NotFound("Użytkownik nie istnieje."); 
            }

            var presentations = _context.Presentation
                .Where(p => p.User.UserID == userId)
                 .Select(p => new PresentationDTO
                 {
                     PresentationID = p.PresentationsID,
                     Title = p.Title,
    
                 }).ToList();

            if (!presentations.Any())
            {
                return NotFound("Brak prezentacji dla tego użytkownika.");
            }

            return Ok(presentations); 
        }


        [HttpPost("/presentation/create")]
        public IActionResult CreatePresentation([FromBody] PresentationRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserID == request.UserId);
            if (user == null)
            {
                return NotFound("Użytkownik nie istnieje.");
            }
            Presentations presentation = new Presentations();
            presentation.Title = request.Title; 
            presentation.User = user;
            try
            {
                _context.Add(presentation);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wewnętrzny błąd serwera.");
            }
            int newPresentationId = presentation.PresentationsID;

            try
            {
                Presentation presentationData = new Presentation();
                presentationData.Title = request.Title;
                presentationData.PresentationID = newPresentationId;
                presentationData.Slides = [];
                this.SavePresentationToXml(presentationData, "" + newPresentationId);
            }
            catch (Exception ex) {
                _context.Remove(presentation);
                _context.SaveChanges();
                return StatusCode(500, "Wewnętrzny błąd serwera.");
            }

            return StatusCode(201, presentation);
        }

        [HttpPost("/presentation/save")]
        public IActionResult SavePresentation([FromBody] Presentation presentation){
            Debug.WriteLine("Zapisywanie...");
            if (presentation == null)
            {
                return BadRequest("Prezentacja jest pusta.");
            }
            try
            {
                SavePresentationToXml(presentation, ""+ presentation.PresentationID);
                Debug.WriteLine("Zapisano.");
                return Ok($"Prezentacja została zapisana jako ");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Błąd podczas zapisywania prezentacji: {ex.Message}");
            }
        }
    }
}
