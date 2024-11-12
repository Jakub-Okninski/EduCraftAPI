using EduCraftAPI.Models;
using GemBox.Presentation;
using Microsoft.AspNetCore.Mvc;


namespace EduCraftAPI.Services
{
    public interface IPresentationService
    {
        FileResult GeneratePPTX(EduCraftAPI.Models.Presentation presentationData, int ID, string type = "pptx");
    }

    public class PresentationService : IPresentationService
    {
        static PresentationService() => ComponentInfo.SetLicense("FREE-LIMITED-KEY");

        public FileResult GeneratePPTX(EduCraftAPI.Models.Presentation presentationData, int ID , string type = "pptx")
        {
            var presentation = new PresentationDocument();


            presentation.SlideSize.Width = 1000;

            presentation.SlideSize.Height = 560;
            string data = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
            if (type == "pptx")
            {
                data = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
            }
            if (type == "pdf")
            {
                data = "application/pdf";
            }
            if (presentationData== null)
            {

                byte[] fileBytes2 = [];
                return new FileContentResult(fileBytes2, data)
                {
                    FileDownloadName = $"{presentationData.Title}."+ type
                };
            }
            if (!presentationData.Slides.Any()){
                byte[] fileBytes2 = [];
                return new FileContentResult(fileBytes2, data)
                {
                    FileDownloadName = $"{presentationData.Title}." + type
                };
            }
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "UserImg", "User" + ID);

            foreach (var s in presentationData.Slides)
            {
                var slide = presentation.Slides.AddNew(SlideLayoutType.Custom);

                if (!s.Elements.Any())
                {
                    continue;
                }
                foreach (var e in s.Elements)
                {
                    if (e.Type == "text")
                    {
                        if (!e.Ops.Any())
                        {
                            continue;
                        }
                        var textShape = slide.Content.AddTextBox(ShapeGeometryType.Rectangle, (double)e.Position.Left, (double)e.Position.Top, (double)e.Size.Width, (double)e.Size.Height);
                        short i = 0;
                      
                        foreach (var o in e.Ops)
                        {
                            var paragraph = textShape.AddParagraph();
                            var run = paragraph.AddRun(o.Insert);
                            if (i+1 < e.Ops.Count - 1)
                            {
                                run.Text = o.Insert.TrimEnd('\n');
                            }           
                            if (o.Attributes?.Bold == true)
                            {
                                run.Format.Bold = true;
                            }
                            i++;    
                        }
                   
                    }
                    if (e.Type == "image")
                    {
                       
                        using (var imageStream = ReadFileAsStream(Path.Combine(uploadsFolder, e.PathName)))
                        {
                        if (imageStream != null)
                        {
                            slide.Content.AddPicture(PictureContentType.Unknown, imageStream, (double)e.Position.Left, (double)e.Position.Top, (double)e.Size.Width, (double)e.Size.Height);
                        } 
                        }
                    }
                }   
            }
            if (type == "pptx")
            {
                var stream = new MemoryStream();
                presentation.Save(stream, SaveOptions.Pptx);

                stream.Position = 0;

                return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.presentationml.presentation")
                {
                    FileDownloadName = $"{presentationData.Title}.pptx"
                };
            }
            if (type == "pdf")
            {
                var stream = new MemoryStream();
                presentation.Save(stream, SaveOptions.Pdf);

                stream.Position = 0;

                return new FileStreamResult(stream, "application/pdf")
                {
                    FileDownloadName = $"{presentationData.Title}.pdf"
                };
            }

            byte[] fileBytes = [];
            return new FileContentResult(fileBytes, data)
            {
                FileDownloadName = $"{presentationData.Title}."+type
            };
        }


        private Stream ConvertBase64ToStream(string base64String)
        {
            // Usunięcie prefiksu, jeśli występuje
            var base64Data = base64String.Replace("data:image/png;base64,", "");

            // Konwersja Base64 na bajty
            byte[] imageBytes = Convert.FromBase64String(base64Data);

            // Utworzenie strumienia z bajtów
            var imageStream = new MemoryStream(imageBytes);

            // Zresetuj pozycję strumienia do początku
            imageStream.Position = 0;

            return imageStream;
        }
        public Stream ReadFileAsStream(string filePath)
        {
            try
            {
                // Otwiera plik do odczytu i zwraca go jako Stream
                return new FileStream(filePath, FileMode.Open, FileAccess.Read);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Plik nie został znaleziony: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wystąpił błąd podczas otwierania pliku: {ex.Message}");
                return null;
            }
        }

    }
}
