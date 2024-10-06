using EduCraftAPI.Entities.Presentation;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Xml.Serialization;

namespace EduCraftAPI.Controllers
{
    public class PresentationController : Controller
    {
        public void SavePresentationToXml(Presentation presentation, string filePath)
        {
            var xmlSerializer = new XmlSerializer(typeof(Presentation));

            using (var stream = new FileStream(filePath, FileMode.Create))
            using (var writer = new StreamWriter(stream))
            {
                xmlSerializer.Serialize(writer, presentation);
            }
        }
        private void ReplaceNewLineCharacters(Presentation presentation)
        {
            if (presentation.Slides != null)
            {
                foreach (var slide in presentation.Slides)
                {
                    if (slide.Title != null)
                    {
                        slide.Title = slide.Title.Replace("&#10;", "\n");
                    }

                    if (slide.Elements != null)
                    {
                        foreach (var element in slide.Elements)
                        {
                            if (element.Ops != null)
                            {
                                foreach (var op in element.Ops)
                                {
                                    if (op.Insert != null)
                                    {
                                       // op.InsertDecode = op.Insert.Replace("&#10;", "\n");
                                    }
                                }
                            }

                            if (element.Url != null)
                            {
                                element.Url = element.Url.Replace("&#10;", "\n");
                            }
                        }
                    }
                }
            }
        }
            public Presentation LoadPresentationFromXml(string filePath)
        {
            var xmlSerializer = new XmlSerializer(typeof(Presentation));

            using (var stream = new FileStream(filePath, FileMode.Open))
            using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
            {
                var presentation = (Presentation)xmlSerializer.Deserialize(reader);

                //ReplaceNewLineCharacters(presentation);

                return presentation;
            }
        }


        [HttpGet("/presentation")]
        public IActionResult GetPresentation()
        { return Ok(LoadPresentationFromXml("presentation.xml"));

             

            var presentationData = new Presentation
            {
                Title = "Moja Prezentacja",
                Slides = new List<Slide>
            {
                new Slide
                {
                    Id = "1",
                    Title = "Slajd 1",
                    Elements = new List<Element>
                    {
                        new Element
                        {
                            Type = "text",
                            Ops = new List<Op>
                            {
                                new Op { Insert = "Hello, " },
                                new Op { Insert = "world", Attributes = new Attributes { Bold = true } },
                                new Op { Insert = "!\nThis is a list:\nItem 1" },
                                new Op { Insert = "\n", Attributes = new Attributes { List = "bullet" } },
                                new Op { Insert = "Item 2" },
                                new Op { Insert = "\n", Attributes = new Attributes { List = "bullet" } }
                            }
                        },
                        new Element
                        {
                            Type = "image",
                            Url = "/src/assets/deadpool.jpg"
                        }
                    }
                },
                new Slide
                {
                    Id = "2",
                    Title = "Slajd 2",
                    Elements = new List<Element>
                    {
                        new Element
                        {
                            Type = "image",
                            Url = "/src/assets/deadpool.jpg"
                        }
                    }
                }
            }
            };
            SavePresentationToXml(presentationData, "presentation.xml");

            return Ok(presentationData);
        }

     
        [HttpPost("/presentation/save")]
        public IActionResult SavePresentation([FromBody] Presentation presentation)
        {
            Debug.WriteLine("Zapisywanie...");
            Debug.WriteLine(presentation.Slides[0].Elements[0].Ops[0].ToString);
        


            if (presentation == null)
            {
                return BadRequest("Prezentacja jest pusta.");
            }
            try
            {
                SavePresentationToXml(presentation, "presentation.xml");
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
