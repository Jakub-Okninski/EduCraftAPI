using GemBox.Presentation;

namespace EduCraftAPI.Services
{
    public interface IPresentationService
    {
        byte[] GeneratePPTX(EduCraftAPI.Models.Presentation presentationData, int ID, string type = "pptx");
    }

    public class PresentationService : IPresentationService
    {
        static PresentationService() => ComponentInfo.SetLicense("FREE-LIMITED-KEY");

        public byte[] GeneratePPTX(EduCraftAPI.Models.Presentation presentationData, int ID , string type = "pptx")
        {
            if ((type != "pptx" && type != "pdf")|| presentationData == null || presentationData.Slides ==null|| presentationData.Slides.Count == 0)
            {
                return [];
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
                            run.Format.Size = 26;

                            if (i+1 < e.Ops.Count - 1)
                            {
                                run.Text = o.Insert.TrimEnd('\n');
                            }           
                            if (o.Attributes?.Bold == true)
                            {
                                try
                                {
                                    if (textShape.Paragraphs[i - 1]?.Elements[0] != null)
                                    {
                                        textShape.Paragraphs[i - 1].Elements[0].Format.Bold = true;
                                    }

                                }
                                catch(Exception ex)
                                {

                                }
                             
                                run.Format.Bold = true; 
                            }
                            if (o.Attributes?.Header == 1)
                            {
                                try
                                {
                                    if (textShape.Paragraphs[i - 1]?.Elements[0] != null)
                                    {
                                        textShape.Paragraphs[i - 1].Elements[0].Format.Size = 50;
                                    }

                                }
                                catch (Exception ex)
                                {

                                }
                              
                                run.Format.Size = 50;

                            }
                            if (o.Attributes?.Align == "center")
                            {
                                try
                                {
                                    if (textShape.Paragraphs[i - 1] != null)
                                    {
                                        textShape.Paragraphs[i - 1].Format.Alignment = HorizontalAlignment.Center;
                                    }

                                }
                                catch (Exception ex)
                                {

                                }
                             
                                paragraph.Format.Alignment = HorizontalAlignment.Center;
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
                if (s.Id == 5)
                {
                    break;
                }
            }

            var stream = new MemoryStream();
            presentation.Save(stream, type == "pptx" ?  SaveOptions.Pptx : SaveOptions.Pdf);
            stream.Position = 0;
            return stream.ToArray();
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
