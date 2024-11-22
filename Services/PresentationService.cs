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
            if ((type != "pptx" && type != "pdf")|| presentationData == null || presentationData.Slides ==null|| presentationData.Slides.Count == 0)
            {
                using (MemoryStream emptyStream = new MemoryStream())
                {
                    return new FileStreamResult(emptyStream, type == "pptx" ? "application/vnd.openxmlformats-officedocument.presentationml.presentation" : "application/pdf")
                    {
                        FileDownloadName = $"{presentationData?.Title ?? "Untitled"}.{type}"
                    };
                }
            }

            var presentation = new PresentationDocument();
            presentation.SlideSize.Width = 1000;
            presentation.SlideSize.Height = 560;
         
          
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "UserDataImage", "User" + ID, "Presentation"+ presentationData.PresentationID);

            foreach (var s in presentationData.Slides)
            {
                var slide = presentation.Slides.AddNew(SlideLayoutType.Custom);

                if (s.Elements == null || s.Elements.Count==0)
                {
                    continue;
                }
                foreach (var e in s.Elements)
                {
                    if (e.Type == "text")
                    {
                        if (e.Ops == null || e.Ops.Count == 0)
                        {
                            continue;
                        }
                        var textShape = slide.Content.AddTextBox(ShapeGeometryType.Rectangle, (double)e.Position.Left, (double)e.Position.Top, (double)e.Size.Width, (double)e.Size.Height);
                        short i = 0;   
                        foreach (var o in e.Ops)
                        {
                            var paragraph = textShape.AddParagraph();
                            var run = paragraph.AddRun(""+o.Insert);
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
                    else if (e.Type == "image")
                    {
                        if(e.PathName == null)
                        {
                            continue;
                        }
                       var localPath =  Path.Combine(uploadsFolder, e.PathName);
                        if (Path.Exists(localPath))
                        {
                            using (var imageStream = ReadFileAsStream(localPath))
                            {
                                if (imageStream != null)
                                {
                                    slide.Content.AddPicture(PictureContentType.Unknown, imageStream, (double)e.Position.Left, (double)e.Position.Top, (double)e.Size.Width, (double)e.Size.Height);
                                }
                            }
                        }
                    }
                }   
            }

            var stream = new MemoryStream();
            presentation.Save(stream, type == "pptx" ?  SaveOptions.Pptx : SaveOptions.Pdf);
            stream.Position = 0;

            return new FileStreamResult(stream, type == "pptx" ? "application/vnd.openxmlformats-officedocument.presentationml.presentation" : "application/pdf")
            {
                FileDownloadName = $"{presentationData.Title}.pptx"
            };    
        }


        private Stream ConvertBase64ToStream(string base64String)
        {
            var imageStream = new MemoryStream(Convert.FromBase64String(base64String.Replace("data:image/png;base64,", "")));
            imageStream.Position = 0;
            return imageStream;
        }
        public Stream ReadFileAsStream(string filePath)
        {
            try
            {
                return new FileStream(filePath, FileMode.Open, FileAccess.Read);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wystąpił błąd podczas otwierania pliku: {ex.Message}");
                return null;
            }
        }
    }
}
