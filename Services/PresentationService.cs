using GemBox.Presentation;
using Microsoft.AspNetCore.Mvc;


namespace EduCraftAPI.Services
{
    public interface IPresentationService
    {
        FileResult GeneratePPTX(EduCraftAPI.Models.Presentation presentationData, string type = "pptx");
    }

    public class PresentationService : IPresentationService
    {

        public FileResult GeneratePPTX(EduCraftAPI.Models.Presentation presentationData ,string type = "pptx")
        {
            var presentation = new PresentationDocument();


            presentation.SlideSize.Width = 1000;

            presentation.SlideSize.Height = 560;


            foreach (var s in presentationData.Slides)
            {
                var slide = presentation.Slides.AddNew(SlideLayoutType.Custom);

                foreach (var e in s.Elements)
                {
                    if (e.Type == "text")
                    {
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
                 
                        using (var imageStream = ConvertBase64ToStream(e.Url))
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
            return new FileContentResult(fileBytes, "application/vnd.openxmlformats-officedocument.presentationml.presentation")
            {
                FileDownloadName = $"{presentationData.Title}.pptx"
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
    }
}
