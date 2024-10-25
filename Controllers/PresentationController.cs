using EduCraftAPI.Data;
using EduCraftAPI.Entities.User;
using EduCraftAPI.Entities.Presentation;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Xml.Serialization;
using EduCraftAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using EduCraftAPI.Services;

namespace EduCraftAPI.Controllers
{
    [Authorize]
    public class PresentationController : Controller
    {
        private readonly DataDbContext _context;
        private readonly IPresentationService _presentationServices;

        public PresentationController(DataDbContext context, IPresentationService presentationServices)
        {
            _context = context;
            _presentationServices = presentationServices;
        }
        public void SavePresentationToXml(Presentation presentation, string filePath){
            string directoryPath = Path.Combine("Presentations", filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string fullFilePath = Path.Combine(directoryPath, filePath);
            var xmlSerializer = new XmlSerializer(typeof(Presentation));
            using (var stream = new FileStream(fullFilePath, FileMode.Create))
            using (var writer = new StreamWriter(stream))
            {
                xmlSerializer.Serialize(writer, presentation);
            }
        }
        public Presentation LoadPresentationFromXml(string filePath) {
            string fullFilePath = Path.Combine("Presentations", filePath, filePath);

            var xmlSerializer = new XmlSerializer(typeof(Presentation));
            using (var stream = new FileStream(fullFilePath, FileMode.Open))
            using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
            {
                return (Presentation)xmlSerializer.Deserialize(reader);
            }
        }


        [HttpGet("/presentation")]
        [AllowAnonymous]
        public IActionResult GetPresentation([FromQuery] int presentationId)
        {
            var presentation = _context.Presentation.FirstOrDefault(p => p.PresentationsID == presentationId);
            if (presentation == null)
            {
                return NoContent();
            }

            return Ok(LoadPresentationFromXml(""+presentation.PresentationsID));
        }


        [AllowAnonymous]
        [HttpGet("/generate/presentation")]
        public IActionResult generatePresentation([FromQuery] int presentationId)
        {

            var presentation = _context.Presentation.FirstOrDefault(p => p.PresentationsID == presentationId);
            if (presentation == null)
            {      
                return NoContent();
            }
            Debug.WriteLine("                    ");
            Debug.WriteLine("dupa2.");
            Debug.WriteLine("                    ");


            return _presentationServices.GeneratePPTX(LoadPresentationFromXml(presentation.PresentationsID.ToString()));
            //var fileName = "presentation.pptx";
            //var contentType = "application/vnd.openxmlformats-officedocument.presentationml.presentation"; 

            //return File(fileContent, contentType, fileName);
        }

        [HttpGet("/presentations")] 
        public IActionResult GetPresentationsByUser([FromQuery] int userId) 
        {

       

            var user = _context.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null)
            {
                return NoContent();
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
                return NoContent();
            }
            return Ok(presentations); 
        }


        [HttpPost("/presentation/create")]
        public IActionResult CreatePresentation([FromBody] PresentationRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserID == request.UserId);
            if (user == null)
            {
                return NoContent();
            }
            var category = _context.Category.FirstOrDefault(u => u.Name == "IT");
            if (category == null)
            {
                return NoContent();
            }
            Presentations presentation = new Presentations();
            presentation.Title = request.Title; 
            presentation.User = user;
            presentation.CreationDate = DateTime.Now;
            presentation.IsPublic = false;
            presentation.Category = category;
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
                return NoContent();
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
