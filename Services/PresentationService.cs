using GemBox.Presentation;
using Color = GemBox.Presentation.Color;

namespace EduCraftAPI.Services
{
    public interface IPresentationService
    {
        byte[] GeneratePPTX(EduCraftAPI.Models.Presentation presentationData, int ID, string type = "pptx");
    }
    public class PresentationService : IPresentationService
    {
        static PresentationService() => ComponentInfo.SetLicense("FREE-LIMITED-KEY");
        public byte[] GeneratePPTX(EduCraftAPI.Models.Presentation presentationData, int ID, string type = "pptx")
        {
            if ((type != "pptx" && type != "pdf") || presentationData == null || presentationData.Slides == null || presentationData.Slides.Count == 0)
            {
                return [];
            }

            var presentation = new PresentationDocument();
            presentation.SlideSize.Width = 1000;
            presentation.SlideSize.Height = 560;
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "UserDataImage", "User" + ID, "Presentation" + presentationData.PresentationID);
            int mainIndex = 0;
            foreach (var s in presentationData.Slides)
            {
                var slide = presentation.Slides.AddNew(SlideLayoutType.Custom);

                if (s.Elements == null || s.Elements.Count == 0)
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

                            float sizeMain = 1;
                            if (o.Attributes?.Size != null)
                            {
                                if (o.Attributes?.Size == "small")
                                {
                                    sizeMain = 0.5f;
                                }
                                else if (o.Attributes?.Size == "large")
                                {
                                    sizeMain = 1.3f;
                                }
                                else if (o.Attributes?.Size == "huge")
                                {
                                    sizeMain = 1.9f;
                                }
                            }
                            var paragraph = textShape.AddParagraph();
                            var run = paragraph.AddRun("" + o.Insert);
                            run.Format.Size = (20 * sizeMain);

                            if (i + 1 < e.Ops.Count - 1)
                            {
                                run.Text = o.Insert.TrimEnd('\n');
                            }

                            if (o.Attributes?.Color != null)
                            {
                                try
                                {
                                    paragraph.Format.Character.Fill.SetSolid(Color.FromHexString(o.Attributes.Color));
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                            if (o.Attributes?.Background != null)
                            {
                                try
                                {
                                    paragraph.Format.Character.HighlightColor = Color.FromHexString(o.Attributes.Background);
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                            if (o.Attributes?.Font != null)
                            {
                                if (o.Attributes?.Font == "monospace")
                                    run.Format.Font = "Courier New";
                                if (o.Attributes?.Font == "serif")
                                    run.Format.Font = "Times New Roman";
                            }
                            if (o.Attributes?.Script == "sub")
                            {
                                paragraph.Format.Character.Offset = -0.5;
                            }
                            else if (o.Attributes?.Script == "sup")
                            {
                                paragraph.Format.Character.Offset = 0.5;
                            }

                            if (o.Attributes?.Strike == true)
                            {
                                run.Format.Strikethrough = StrikethroughType.Single;
                            }

                            if (o.Attributes?.Underline == true)
                            {
                                run.Format.UnderlineStyle = UnderlineStyle.Single;
                            }
                            if (o.Attributes?.Italic == true)
                            {
                                run.Format.Italic = true;
                            }

                            if (o.Attributes?.Bold == true || o.Attributes?.Header != null)
                            {
                                try
                                {
                                    if (textShape.Paragraphs[i - 1]?.Elements[0] != null)
                                    {
                                        textShape.Paragraphs[i - 1].Elements[0].Format.Bold = true;
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                                run.Format.Bold = true;
                            }
                            if (o.Attributes?.Header != null)
                            {
                                int sizeLocal = 50;
                                if (o.Attributes?.Header == 2) sizeLocal = 40;
                                else if (o.Attributes?.Header == 3) sizeLocal = 35;
                                else if (o.Attributes?.Header == 4) sizeLocal = 30;
                                else if (o.Attributes?.Header == 5) sizeLocal = 25;
                                else if (o.Attributes?.Header == 6) sizeLocal = 20;

                                sizeLocal = ((int)(sizeLocal * sizeMain));
                                try
                                {
                                    if (textShape.Paragraphs[i - 1]?.Elements[0] != null)
                                    {
                                        textShape.Paragraphs[i - 1].Elements[0].Format.Size = sizeLocal;
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                                run.Format.Size = sizeLocal;
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
                            else if (o.Attributes?.Align == "right")
                            {
                                try
                                {
                                    if (textShape.Paragraphs[i - 1] != null)
                                    {
                                        textShape.Paragraphs[i - 1].Format.Alignment = HorizontalAlignment.Right;
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                                paragraph.Format.Alignment = HorizontalAlignment.Right;
                            }
                            else if (o.Attributes?.Align == "left")
                            {
                                try
                                {
                                    if (textShape.Paragraphs[i - 1] != null)
                                    {
                                        textShape.Paragraphs[i - 1].Format.Alignment = HorizontalAlignment.Left;
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                                paragraph.Format.Alignment = HorizontalAlignment.Left;
                            }
                            else if (o.Attributes?.Align == "justify")
                            {
                                try
                                {
                                    if (textShape.Paragraphs[i - 1] != null)
                                    {
                                        textShape.Paragraphs[i - 1].Format.Alignment = HorizontalAlignment.Justify;
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                                paragraph.Format.Alignment = HorizontalAlignment.Justify;
                            }
                            i++;
                        }
                    }
                    else if (e.Type == "image")
                    {
                        if (e.PathName == null)
                        {
                            continue;
                        }
                        var localPath = Path.Combine(uploadsFolder, e.PathName);
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
                mainIndex++;
                if (mainIndex == 5)
                {
                    break;
                }
            }
            var stream = new MemoryStream();
            presentation.Save(stream, type == "pptx" ? SaveOptions.Pptx : SaveOptions.Pdf);
            stream.Position = 0;
            return stream.ToArray();
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
