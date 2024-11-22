using EduCraftAPI.Data;
using EduCraftAPI.Entities.Presentation;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using EduCraftAPI.Models;
using Microsoft.AspNetCore.Authorization;
using EduCraftAPI.Services;
using SixLabors.ImageSharp;
using Microsoft.EntityFrameworkCore;

namespace EduCraftAPI.Controllers
{
    [Authorize]
    public class PresentationController : Controller
    {
        private readonly DataDbContext _context;
        private readonly IPresentationService _presentationServices;
        private readonly IFileService _fileServices;
        private readonly IUserContextService _userContextService;

        public PresentationController(DataDbContext context, IPresentationService presentationServices, IFileService fileServices, IUserContextService userContextService)
        {
            _context = context;
            _presentationServices = presentationServices;
            _fileServices = fileServices;
            _userContextService = userContextService;
        }

        [HttpPost("/presentation/upload/image")]
        public async Task<IActionResult> UploadPresentationImage([FromForm] ImgSlideDTO imgSlideDTO)
        {
            if (imgSlideDTO.Image == null || imgSlideDTO.Image.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
           

            Presentation presentation = _fileServices.LoadPresentationFromXml(imgSlideDTO.PresentationID)!;
            if (presentation == null)
            {
                return NoContent();
            }

            var slide = presentation.Slides?.FirstOrDefault(s => s.Id == imgSlideDTO.SlideID);
            if (slide == null)
            {
                return NoContent();
            }

       
            Element element = new Element();
            try
            {
                var newFileName = Guid.NewGuid().ToString() + Path.GetExtension(imgSlideDTO.Image.FileName);

                element.Url = _fileServices.SaveImagePresentation((int)_userContextService.GetUserID, presentation.PresentationID, imgSlideDTO.Image, newFileName);
                element.PathName = newFileName;

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wewnętrzny błąd serwera." + ex);
            }

            if (slide.Elements == null)
            {
                slide.Elements = new List<Element>();
            }
            int newID = slide.Elements.Count + 1;
            element.Id = newID;
            element.Type = "image";

            element.Position = new Position
            {
                Left = imgSlideDTO.PositionX,
                Top = imgSlideDTO.PositionY
            };
            element.Size = new Models.Size
            {
                Width = imgSlideDTO.Width,
                Height = imgSlideDTO.Height
            };

            slide.Elements?.Add(element);
            _fileServices.SavePresentationToXml(presentation, imgSlideDTO.PresentationID);

            return Ok(new { Data = element, SlideId = slide.Id });
        }



        [HttpDelete("/presentation/delete")]
        public async Task<IActionResult> RemovePresentation([FromQuery] int id)
        {
            var presentation = _context.Presentation.FirstOrDefault(p => p.PresentationsID == id);

            if (presentation == null)
            {
                return NotFound();
            }
            try
            {
                _fileServices.RemovePresentation(presentation.PresentationsID);
                _fileServices.RemoveImgDirectory(presentation.UserID, "Presentation", presentation.PresentationsID);
                _context.Presentation.Remove(presentation);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wewnętrzny błąd serwera." + ex);
            }
            _context.SaveChanges();
            return Ok();
        }


        [AllowAnonymous]
        [HttpGet("/presentation")]
        public IActionResult GetPresentation([FromQuery] int presentationId)
        {
            var presentation = _context.Presentation.FirstOrDefault(p => p.PresentationsID == presentationId);
            if (presentation == null)
            {
                return NoContent();
            }
            Presentation presentationData = _fileServices.LoadPresentationFromXml(presentation.PresentationsID);
            if (presentationData == null)
            {
                return NoContent();
            }
            return Ok(presentationData);
        }
        [HttpGet("/presentation/data")]
        public IActionResult GetPresentationData([FromQuery] int presentationId)
        {
            var presentation = _context.Presentation.FirstOrDefault(p => p.PresentationsID == presentationId);
            if (presentation == null)
            {
                return NoContent();
            }

            return Ok(presentation);
        }



        [HttpDelete("/presentation/remove/slide")]
        public IActionResult RemoveSlidePresentationData([FromQuery] int PresentationID, int SlideID)
        {
            try
            {
                var presentation = _context.Presentation.FirstOrDefault(p => p.PresentationsID == PresentationID);
                if (presentation == null)
                {
                    return NoContent();
                }
                var presentationData = _fileServices.LoadPresentationFromXml(presentation.PresentationsID);
                if (presentationData == null)
                {
                    return NoContent();
                }
                var slide = presentationData.Slides?.FirstOrDefault(s => s.Id == SlideID);
                if (slide == null)
                {
                    return NoContent();
                }
                foreach(var element in slide?.Elements)
                {
                    if (element.Type == "image")
                    {
                        _fileServices.RemoveImagePresentation(presentation.UserID, presentation.PresentationsID, element.PathName);
                    }
                }

                presentationData.Slides.Remove(slide);
                _fileServices.SavePresentationToXml(presentationData, PresentationID);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wewnętrzny błąd serwera." + ex);

            }
        }

        [HttpDelete("/presentation/remove/element")]
        public IActionResult RemoveElementPresentationData([FromQuery] int PresentationID, int SlideID, int ElementID)
        {
            try
            {
                var presentation = _context.Presentation.FirstOrDefault(p => p.PresentationsID == PresentationID);
                if (presentation == null)
                {
                    return NoContent();
                }
                var presentationData = _fileServices.LoadPresentationFromXml(presentation.PresentationsID);
                if (presentationData == null)
                {
                    return NoContent();
                }
                var slide = presentationData.Slides?.FirstOrDefault(s => s.Id == SlideID);
                if (slide == null)
                {
                    return NoContent();
                }
                var element = slide.Elements?.FirstOrDefault(e => e.Id == ElementID);
                if (element == null)
                {
                    return NoContent();
                }

                if (element.Type == "image")
                {
                    _fileServices.RemoveImagePresentation(presentation.UserID, presentation.PresentationsID, element.PathName);
                }

                slide.Elements.Remove(element);
                _fileServices.SavePresentationToXml(presentationData, PresentationID);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wewnętrzny błąd serwera." + ex);

            }
        }

        [HttpPost("/presentation/add/text")]
        public IActionResult AddTextPresentationData([FromBody] TextSlideDTO textSlideDTO)
        {
            try
            {
                var presentation = _context.Presentation.FirstOrDefault(p => p.PresentationsID == textSlideDTO.PresentationID);
                if (presentation == null)
                {
                    return NoContent();
                }
                var presentationData = _fileServices.LoadPresentationFromXml(presentation.PresentationsID);
                if (presentationData == null)
                {
                    return NoContent();
                }
                var slide = presentationData.Slides?.FirstOrDefault(s => s.Id == textSlideDTO.SlideID);
                if (slide == null)
                {
                    return NoContent();
                }

                if (slide.Elements == null)
                {
                    slide.Elements = new List<Element>();
                }
                int newID = slide.Elements.Count + 1;
                Element element = new Element();
                element.Id = newID;
                element.Type = "text";
                element.Position = new Position()
                {
                    Left = textSlideDTO.PositionX,
                    Top = textSlideDTO.PositionY,
                };
                element.Size = new EduCraftAPI.Models.Size()
                {
                    Width = textSlideDTO.Width,
                    Height = textSlideDTO.Height,
                };

                element.Ops = new List<Op> { 
                    new Op(){
                    Insert= "Nowy Element \n "
                }};

                slide.Elements.Add(element);
                _fileServices.SavePresentationToXml(presentationData, textSlideDTO.PresentationID);

                return Ok(element);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wewnętrzny błąd serwera." + ex);

            }    
         }

        [AllowAnonymous]
        [HttpGet("/generate/presentation")]
        public IActionResult generatePresentation([FromQuery] int presentationId, string type)
        {
            try
            {
                var presentation = _context.Presentation.FirstOrDefault(p => p.PresentationsID == presentationId);
                if (presentation == null)
                {
                    return NoContent();
                }
                Presentation presentationData = _fileServices.LoadPresentationFromXml(presentation.PresentationsID);
                if (presentationData == null)
                {
                    return NoContent();
                }
                return _presentationServices.GeneratePPTX(presentationData, presentation.UserID, type);

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wewnętrzny błąd serwera.");
            }
        }

        [HttpGet("/presentations")]
        public IActionResult GetPresentationsByUser()
        {
            var presentations = _context.Presentation
                 .Where(p => p.User.UserID == _userContextService.GetUserID);
           
            if (!presentations.Any())
            {
                return NoContent();
            }
            return Ok(presentations.ToList());
        }


        [HttpPost("/presentation/add/slide")]
        public IActionResult sldieAddPresentation([FromBody] DTOID ID)
        {
            Presentation presentation = _fileServices.LoadPresentationFromXml(ID.ID)!;
            if(presentation == null) 
            { 
                return NoContent(); 
            }
            try
            {
                Slide slide = new Slide();
                slide.Elements = new List<Element>();
                if (presentation.Slides == null)
                {
                    presentation.Slides = new List<Slide>();
                }
                int newID = presentation.Slides.Count;

                newID++;
                slide.Id = newID;
                slide.Title = "Slajd " + newID;
                presentation.Slides.Add(slide);
                _fileServices.SavePresentationToXml(presentation, ID.ID);
                return Ok(slide);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wewnętrzny błąd serwera." + ex);
            }     
        }


        [HttpPost("/presentation/update/isPublic")]
        public IActionResult updatePresentation([FromBody] IsPublicDTO isPublicDTO)
        {
            var presentation = _context.Presentation.FirstOrDefault(u => u.PresentationsID == isPublicDTO.ItemID);
            if (presentation == null)
            {
                return NoContent();
            }
            try
            {
                presentation.IsPublic = isPublicDTO.IsPublic;
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wewnętrzny błąd serwera.");
            }
            return Ok(presentation);
        }

        [HttpPost("/presentation/create")]
        public IActionResult CreatePresentation([FromBody] TitleUserDTO request)
        {
            var catrgory = _context.Category.FirstOrDefault(u => u.CategoryID == request.CategoryID);
            if (catrgory == null)
            {
                return NoContent();
            }

            Presentations presentation = new Presentations();
            presentation.Title = request.Title;
            presentation.UserID = (int)_userContextService.GetUserID!;
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
            Presentation presentationData = new Presentation();
            presentationData.Title = request.Title;
            presentationData.PresentationID = presentation.PresentationsID;
            presentationData.Slides = new List<Slide>();
            try
            {
                _fileServices.SavePresentationToXml(presentationData, presentation.PresentationsID);
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
                _fileServices.SavePresentationToXml(presentation, presentation.PresentationID);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Błąd podczas zapisywania prezentacji: {ex.Message}");
            }
            return Ok();
        }
    }
}
