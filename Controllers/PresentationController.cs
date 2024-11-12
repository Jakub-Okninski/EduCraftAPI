using EduCraftAPI.Data;
using EduCraftAPI.Entities.Presentation;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Xml.Serialization;
using EduCraftAPI.Models;
using Microsoft.AspNetCore.Authorization;
using EduCraftAPI.Services;
using Path = System.IO.Path;
using SixLabors.ImageSharp;
using System.Drawing;
using System.Xml.Linq;


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

        public void SavePresentationToXml(Presentation presentation, string filename)
        {
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Presentations");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string fullFilePath = Path.Combine(directoryPath, filename);
            var xmlSerializer = new XmlSerializer(typeof(Presentation));
            using (var stream = new FileStream(fullFilePath, FileMode.Create))
            using (var writer = new StreamWriter(stream))
            {
                xmlSerializer.Serialize(writer, presentation);
            }
        }
        public Presentation LoadPresentationFromXml(string filePath)
        {
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Presentations");
            string fullFilePath = Path.Combine(directoryPath, filePath);

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

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "UserImg", "User" + imgSlideDTO.UserID);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imgSlideDTO.Image.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);
            Element e = new Element();

            try
            {
                using (var image = System.Drawing.Image.FromStream(imgSlideDTO.Image.OpenReadStream()))
                {
                    int maxWidth = 854;
                    int maxHeight = 480;

                    int newWidth = image.Width;
                    int newHeight = image.Height;

                    if (image.Width > maxWidth || image.Height > maxHeight)
                    {
                        float ratioX = (float)maxWidth / image.Width;
                        float ratioY = (float)maxHeight / image.Height;
                        float ratio = Math.Min(ratioX, ratioY);

                        newWidth = (int)(image.Width * ratio);
                        newHeight = (int)(image.Height * ratio);
                    }

                    using (var resizedImage = new Bitmap(newWidth, newHeight))
                    {
                        using (var graphics = Graphics.FromImage(resizedImage))
                        {
                            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            graphics.DrawImage(image, 0, 0, newWidth, newHeight);
                        }


                        resizedImage.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);

                        using (var memoryStream = new MemoryStream())
                        {
                            resizedImage.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                            string base64Image = Convert.ToBase64String(memoryStream.ToArray());
                            string fileNameLower = fileName.ToLower();
                            if (fileNameLower.EndsWith(".jpg") || fileNameLower.EndsWith(".jpeg"))
                            {
                                base64Image = "data:image/jpeg;base64," + base64Image;
                            }
                            else if (fileNameLower.EndsWith(".png"))
                            {
                                base64Image = "data:image/png;base64," + base64Image;
                            }
                            else if (fileNameLower.EndsWith(".gif"))
                            {
                                base64Image = "data:image/gif;base64," + base64Image;
                            }

                            e.Url = base64Image;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error resizing the image: {ex.Message}");
            }

            // Dodajemy dodatkowe informacje do slajdu i elementu
            Presentation p = LoadPresentationFromXml(imgSlideDTO.PresentationID + "");
          
            var slide = p.Slides.FirstOrDefault(s => s.Id == imgSlideDTO.SlideID);
            if (slide == null)
            {
                return NoContent();
            }

            int id = slide.Elements.Count + 1;
            e.Id = id;
            e.Type = "image";
            e.PathName = fileName;

            // Ustawienie pozycji i rozmiaru elementu
            e.Position = new EduCraftAPI.Models.Position
            {
                Left = imgSlideDTO.PositionX,
                Top = imgSlideDTO.PositionY
            };
            e.Size = new Models.Size
            {
                Width = imgSlideDTO.Width,
                Height = imgSlideDTO.Height
            };

            slide.Elements?.Add(e);
            SavePresentationToXml(p, imgSlideDTO.PresentationID + "");

            return Ok(new { Data = e, SlideId = slide.Id });
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
            
            return _presentationServices.GeneratePPTX(LoadPresentationFromXml(presentation.PresentationsID.ToString()), presentation.UserID, type);
      
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

        [HttpPost("/presentation/add/slide")]
        public IActionResult sldieAddPresentation([FromBody] DTOID ID)
        {
            int presentationID = ID.ID;
            Debug.WriteLine(presentationID);

            Presentation p = LoadPresentationFromXml(presentationID + "");

            try
            {
               Slide s = new Slide();
                s.Title = "Nowy slajd";
                s.Elements = new List<Element>();
                if (p.Slides == null)
                {
                    p.Slides = new List<Slide>();
                }
                int id = p.Slides.Count;
                Debug.WriteLine(id);

                if (id == null)
                {
                    id = 0;
                }
                    id++;
                
                Debug.WriteLine(id);

                s.Id = id;
                p?.Slides?.Add(s);
                this.SavePresentationToXml(p, presentationID + "");
                return Ok(s);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wewnętrzny błąd serwera."+ ex);
            }
           
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
            Presentation presentationData = new Presentation();
            presentationData.Title = request.Title;
            presentationData.PresentationID = newPresentationId;
            presentationData.Slides = new List<Slide>();

            this.SavePresentationToXml(presentationData, "" + newPresentationId);
            try
            {
              
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
