using EduCraftAPI.Data;
using EduCraftAPI.Entities.Presentation;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Xml.Serialization;
using EduCraftAPI.Models;
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
        public void SavePresentationToXml(Presentation presentation, string filePath)
        {
          

            string directoryPath = Path.Combine("Presentations", filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var xmlSerializer = new XmlSerializer(typeof(Presentation));
            using (var stream = new FileStream(directoryPath, FileMode.Create))
            using (var writer = new StreamWriter(stream))
            {
                xmlSerializer.Serialize(writer, presentation);
            }
        }
        public Presentation LoadPresentationFromXml(string filePath)
        {
            string fullFilePath = Path.Combine("Presentations", filePath);

            var xmlSerializer = new XmlSerializer(typeof(Presentation));
            using (var stream = new FileStream(fullFilePath, FileMode.Open))
            using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
            {
                return (Presentation)xmlSerializer.Deserialize(reader);
            }
        }

        [AllowAnonymous]
        [HttpPost("/presentation/upload/image")]
        public async Task<IActionResult> UploadImage([FromForm] ImgSlideDTO imgSlideDTO)
        {
            if (imgSlideDTO.Image == null || imgSlideDTO.Image.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            string uploadsFolder = Path.Combine("UserImg","User"+imgSlideDTO.UserID);

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imgSlideDTO.Image.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imgSlideDTO.Image.CopyToAsync(stream);
                }
            }
            catch(Exception e)
            {

            }
            Presentation p = LoadPresentationFromXml(imgSlideDTO.PresentationID+"");
            p.Slides?.ForEach(s =>
            {
                if (s.Id == imgSlideDTO.SlideID)
                {
                    Element e = new Element();
                    EduCraftAPI.Models.Position p = new EduCraftAPI.Models.Position();

                    p.Left = imgSlideDTO.Position.Left;
                    p.Top = imgSlideDTO.Position.Top;

                    e.Position = p;
                    e.Type = "image";
                    e.Url = fileName;

                    s.Elements?.Add(e);

                }
            });
            this.SavePresentationToXml(p, imgSlideDTO.PresentationID + "");

            return Ok(new { FilePath = filePath, FileName = fileName });
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

            return Ok(LoadPresentationFromXml("" + presentation.PresentationsID));
        }
        [HttpGet("/presentation/data")]
        [AllowAnonymous]
        public IActionResult GetPresentationData([FromQuery] int presentationId)
        {
            var presentation = _context.Presentation.FirstOrDefault(p => p.PresentationsID == presentationId);
            if (presentation == null)
            {
                return NoContent();
            }

            return Ok(presentation);
        }

        [AllowAnonymous]
        [HttpGet("/generate/presentation")]
        public IActionResult generatePresentation([FromQuery] int presentationId, string type)
        {

            var presentation = _context.Presentation.FirstOrDefault(p => p.PresentationsID == presentationId);
            if (presentation == null)
            {
                return NoContent();
            }

            return _presentationServices.GeneratePPTX(LoadPresentationFromXml(presentation.PresentationsID.ToString()),type);
      
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

                 });

            if (!presentations.Any())
            {
                return NoContent();
            }
            return Ok(presentations.ToList());
        }




        [HttpPost("/presentation/update/isPublic")]
        public IActionResult updatePresentation([FromBody] IsPublicDTO request)
        {
            var presentation = _context.Presentation.FirstOrDefault(u => u.PresentationsID == request.ItemID);
            if (presentation == null)
            {
                return NoContent();
            }
            try
            {
            presentation.IsPublic = request.IsPublic;
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wewnętrzny błąd serwera.");
            }
            return StatusCode(200, presentation);
        }





        [HttpPost("/presentation/create")]
        public IActionResult CreatePresentation([FromBody] TitleUserDTO request)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserID == request.UserId);
            if (user == null)
            {
                return NoContent();
            }
            var catrgory = _context.Category.FirstOrDefault(u => u.CategoryID == request.CategoryID);
            if (catrgory == null)
            {
                return NoContent();
            }

            Presentations presentation = new Presentations();
            presentation.Title = request.Title;
            presentation.User = user;
            presentation.CreationDate = DateTime.Now;
            presentation.IsPublic = request.IsPublic;
            presentation.Category = catrgory;
            try
            {
                _context.Add(presentation);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wewnętrzny błąd serwera. base");
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
            catch (Exception ex)
            {
                _context.Remove(presentation);
                _context.SaveChanges();
                return StatusCode(500, "Wewnętrzny błąd serwera. file");
            }

            return StatusCode(201, presentation);
        }

        [HttpPost("/presentation/save")]
        public IActionResult SavePresentation([FromBody] Presentation presentation)
        {
            Debug.WriteLine("Zapisywanie...");
            if (presentation == null)
            {
                return NoContent();
            }
            try
            {
                SavePresentationToXml(presentation, "" + presentation.PresentationID);
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
