using EduCraftAPI.Data;
using EduCraftAPI.Entities.Presentation;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using EduCraftAPI.Models;
using Microsoft.AspNetCore.Authorization;
using EduCraftAPI.Services;

namespace EduCraftAPI.Controllers
{
    [Authorize(Policy = "IsBlock")]
    public class PresentationController : Controller
    {
        private readonly DataDbContext _context;
        private readonly IPresentationService _presentationServices;
        private readonly IFileService _fileServices;
        private readonly IUserContextService _userContextService;
        private readonly IGenerateService _generateService;

        public PresentationController(DataDbContext context, IPresentationService presentationServices, IFileService fileServices, IUserContextService userContextService, IGenerateService generateService)
        {
            _context = context;
            _presentationServices = presentationServices;
            _fileServices = fileServices;
            _userContextService = userContextService;
            _generateService = generateService;
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
            int newID = 0;
            if (slide.Elements == null)
            {
                slide.Elements = new List<Element>();
                newID = slide.Elements.Count;
            }
            else
            {
                foreach(var el in slide.Elements)
                {
                    if (el.Id >= newID)
                    {
                        newID = el.Id;
                    }
                }
            }
           
            element.Id = newID+1;
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

                Element element = new Element();
                int newID = 0;
                if (slide.Elements == null)
                {
                    slide.Elements = new List<Element>();
                    newID = slide.Elements.Count;
                }
                else
                {
                    foreach (var el in slide.Elements)
                    {
                        if (el.Id >= newID)
                        {
                            newID = el.Id;
                        }
                    }
                }

                element.Id = newID+1;
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
        public ActionResult generatePresentation([FromQuery] int presentationId, string type)
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
                if (presentationData.Slides == null|| presentationData.Slides.Count==0)
                {
                    return NoContent();
                }
                string mimeType = (type == "pptx") ? "application/vnd.openxmlformats-officedocument.presentationml.presentation" : "application/pdf";
            
                return File(_presentationServices.GeneratePPTX(presentationData, presentation.UserID, type), mimeType);
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

                int newID = 0;     
                if (presentation.Slides == null)
                {
                    presentation.Slides = new List<Slide>();
                    newID = presentation.Slides.Count;
                }
                else
                {
                    foreach (var sl in presentation.Slides)
                    {
                        if (sl.Id >= newID)
                        {
                            newID = sl.Id;
                        }
                    }
                }
                slide.Id = newID + 1;
                slide.Title = "Slajd " + slide.Id;
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



        [HttpPost("/presentation/create")]
        public async Task<IActionResult> CreatePresentation([FromBody] TitleUserDTO request)
        {
            var catrgory = _context.Category.FirstOrDefault(u => u.CategoryID == request.CategoryID);
            if (catrgory == null)
            {
                return NoContent();
            }

            Presentation presentationData = null;
            try
            {
                presentationData = await _generateService.generatePresentationDataText(request.Description, request.Title);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wewnętrzny błąd serwera. base");
            }


            if (presentationData == null)
            {
                return NoContent();
            }
            presentationData.Title = request.Title;



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


            presentationData.PresentationID = presentation.PresentationsID;

            try
            {
                presentationData = await _generateService.generatePresentationDataImage(presentationData, (int)_userContextService.GetUserID, presentation.PresentationsID, request.Description);
            }
            catch (Exception ex)
            {

                Debug.WriteLine("Wystąpił bład........");
                Debug.WriteLine(ex.Message);

            }





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

        
    }
}
