using EduCraftAPI.Entities.Flashcards;
using EduCraftAPI.Entities.Quiz;
using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.Util;
using NPOI.XWPF.UserModel;
using PdfSharp.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EduCraftAPI.Services
{
    public interface IDocumentService
    {
        public byte[] GenerateFlashcards(Flashcards flashcards);
        public byte[] GenerateQuiz(Quiz quiz, bool withCorrect = false);
        public byte[] GenerateQuizAsPdf(Quiz quiz, bool withCorrect = false);
        public byte[] GenerateFlashcardsAsPdf(Flashcards flashcards);
    }

    public class DocumentService : IDocumentService
    {
        private readonly IFileService _fileService;
        public DocumentService(IFileService fileService)
        {
            _fileService = fileService;
        }
        public byte[] GenerateQuizAsPdf(Quiz quiz, bool withCorrect = false)
        {
            var folderPath = Path.Combine("UserDataImage", "User" + quiz.UserID, "Quiz" + quiz.QuizID);
            PdfSharp.Pdf.PdfDocument document = new PdfSharp.Pdf.PdfDocument();
            PdfSharp.Pdf.PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font = new XFont("Arial", 12);

            double yPosition = 20;
            double mainTitleWiedt = gfx.MeasureString("Quiz: " + quiz.Name, new XFont("Arial", 16, XFontStyleEx.Bold)).Width;
            double maintitlexPosition = (page.Width - mainTitleWiedt) / 2;

            gfx.DrawString("Quiz: " + quiz.Name, new XFont("Arial", 16, XFontStyleEx.Bold), XBrushes.Black, new XRect(maintitlexPosition, yPosition, mainTitleWiedt, 0));
            yPosition += 40;
            foreach (var question in quiz.Questions)
            {
                double questionTextWidth = gfx.MeasureString(question.Name, new XFont("Arial", 14, XFontStyleEx.Bold)).Width;
                double xPosition = (page.Width - questionTextWidth) / 2;

                gfx.DrawString(question.Name, new XFont("Arial", 14, XFontStyleEx.Bold), XBrushes.Black, new XRect(xPosition, yPosition, questionTextWidth, 0));
                int data = 20;
                if (!string.IsNullOrEmpty(question.FileName))
                {
                    string imagePath = Path.Combine(folderPath, question.FileName);
                    if (File.Exists(imagePath))
                    {
                        XImage img = XImage.FromFile(imagePath);
                        double maxWidth = page.Width / 3;
                        double scaleX = maxWidth / img.PixelWidth;
                        double newWidth = img.PixelWidth * scaleX;
                        double newHeight = img.PixelHeight * scaleX;
                        double centerX = (page.Width - newWidth) / 2;
                        yPosition += 10;
                        gfx.DrawImage(img, centerX, yPosition, newWidth, newHeight);
                        yPosition += (newHeight + 20);
                        data = 0;
                    }
                }
                yPosition += data;
                foreach (var (answer, index) in question.Answers.Select((a, i) => (a, i)))
                {
                    char letter = (char)('A' + index);
                    string cleanedText = answer.Name.Replace("\n", " ");

                    string answerText = $"({letter}) {cleanedText}";
                    if (answer.IsCorrect && withCorrect)
                    {
                        answerText = "(+) " + answerText;
                        gfx.DrawString(answerText, font, XBrushes.Green, new XRect(30, yPosition, 30, 0));
                    }
                    else
                    {
                        gfx.DrawString(answerText, font, XBrushes.Black, new XRect(30, yPosition, 30, 0));
                    }
                    yPosition += 20;
                    if (yPosition > page.Height - 20)
                    {
                        page = document.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        yPosition = 20;
                    }
                }
                yPosition += 10;
                if (yPosition > page.Height - 20)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    yPosition = 20;
                }
            }
            using (MemoryStream stream = new MemoryStream())
            {
                document.Save(stream);
                return stream.ToArray();
            }
        }
        public byte[] GenerateQuiz(Quiz quiz, bool withCorrect = false)
        {
            XWPFDocument doc = new XWPFDocument();
            XWPFParagraph mainParagraph = doc.CreateParagraph();
            mainParagraph.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
            XWPFRun mainRun = mainParagraph.CreateRun();
            mainRun.SetText("Quiz: " + quiz.Name);
            mainRun.IsBold = true;
            mainRun.FontSize = 16;

            var folderPath = Path.Combine("UserDataImage", "User" + quiz.UserID, "Quiz" + quiz.QuizID);
            foreach (var question in quiz.Questions)
            {
                XWPFParagraph questionParagraph = doc.CreateParagraph();
                questionParagraph.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                XWPFRun questionRun = questionParagraph.CreateRun();
                questionRun.SetText(question.Name);
                questionRun.IsBold = true;
                questionRun.FontSize = 14;

                if (!string.IsNullOrEmpty(question.FileName))
                {
                    string imagePath = Path.Combine(folderPath, question.FileName);

                    if (File.Exists(imagePath))
                    {
                        using (FileStream imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                        {
                            XWPFParagraph titleParagraphPicture = doc.CreateParagraph();
                            titleParagraphPicture.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                            XWPFRun titleRunPicture = titleParagraphPicture.CreateRun();
                            using (var image = System.Drawing.Image.FromStream(imageStream))
                            {
                                double aspectRatio = (double)image.Width / image.Height;
                                int documentWidth = 6000;
                                int newWidth = documentWidth / 3;
                                int newHeight = (int)(newWidth / aspectRatio);
                                imageStream.Seek(0, SeekOrigin.Begin);
                                titleRunPicture.AddPicture(
                                    imageStream,
                                    (int)PictureType.PNG,
                                    "logo.png",
                                    Units.ToEMU(newWidth / 10),
                                    Units.ToEMU(newHeight / 10)
                                );
                            }
                        }
                    }
                }
                int index = 0;
                foreach (var answer in question.Answers)
                {
                    XWPFParagraph answerParagraf = doc.CreateParagraph();
                    answerParagraf.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.LEFT;
                    XWPFRun answerRun = answerParagraf.CreateRun();
                    char letter = (char)('A' + index);
                    string cleanedText = answer.Name.Replace("\n", " ");

                    string dataText = $"({letter})  {cleanedText}";
                    if (answer.IsCorrect && withCorrect)
                    {
                        dataText = "(✓) " + dataText;
                        answerRun.IsBold = true;
                        answerRun.SetColor("008000");
                    }
                    answerRun.SetText(dataText);
                    index++;
                }
            }
            using (MemoryStream memoryStream = new MemoryStream())
            {
                doc.Write(memoryStream);
                return memoryStream.ToArray();
            };
        }
        public byte[] GenerateFlashcards(Flashcards flashcards)
        {
            var folderPath = Path.Combine("UserDataImage", "User" + flashcards.UserID, "Flashcard" + flashcards.FlashcardsID);

            XWPFDocument doc = new XWPFDocument();

            foreach (var card in flashcards.Flashcard)
            {

                XWPFTable table = doc.CreateTable(1, 2);
                var ctTable = table.GetCTTbl();
                var tblProperties = ctTable.AddNewTblPr();
                tblProperties.jc = new CT_Jc { val = ST_Jc.center };


                var tblLayout1 = table.GetCTTbl().tblPr.AddNewTblLayout();
                tblLayout1.type = ST_TblLayoutType.@fixed;
                table.SetColumnWidth(0, 3500);
                table.SetColumnWidth(1, 3500);

                XWPFTableCell titleCell = table.GetRow(0).GetCell(0);
                titleCell.RemoveParagraph(0);
                titleCell.SetVerticalAlignment(XWPFTableCell.XWPFVertAlign.CENTER);



                string titleText = card.Title;
                string[] titleLines = titleText.Split(new[] { "\n" }, StringSplitOptions.None);
                int titleLinesCount = titleLines.Length;
                if (titleLinesCount < 1)
                {
                    titleLinesCount = 1;
                    titleLines = new[] { "" };
                }
                else
                {
                    foreach (var item in titleLines)
                    {
                        XWPFParagraph titleParagraph = titleCell.AddParagraph();
                        titleParagraph.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                        XWPFRun titleRun = titleParagraph.CreateRun();
                        titleRun.SetText(item);
                        titleRun.IsBold = true;
                        titleRun.FontSize = 14;
                    }
                }
                XWPFTableCell descriptionCell = table.GetRow(0).GetCell(1);
                table.GetRow(0).Height = 3500;
                descriptionCell.RemoveParagraph(0);
                descriptionCell.SetVerticalAlignment(XWPFTableCell.XWPFVertAlign.CENTER);
     
                string descriptionText = card.Description;
                string[] descriptionLines = descriptionText.Split(new[] { "\n" }, StringSplitOptions.None);
                int descriptionLinesCount = descriptionLines.Length;
                if (descriptionLinesCount < 1)
                {
                    descriptionLinesCount = 1;
                    descriptionLines = new[] { "" };
                }
                else
                {
                    foreach(var item in descriptionLines)
                    {
                        XWPFParagraph descriptionParagraph = descriptionCell.AddParagraph();
                        descriptionParagraph.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                        XWPFRun descriptionRun = descriptionParagraph.CreateRun();
                        descriptionRun.SetText(item);
                        descriptionRun.IsBold = true;
                        descriptionRun.FontSize = 14;
                        descriptionRun.AddBreak();
                    }
                }

                if (!string.IsNullOrEmpty(card.FileName))
                {
                    string imagePath = Path.Combine(folderPath, card.FileName);

                    if (File.Exists(imagePath))
                    {
                        using (FileStream imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                        {
                            XWPFParagraph titleParagraphPicture = titleCell.AddParagraph();

                            titleParagraphPicture.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
                            XWPFRun titleRunPicture = titleParagraphPicture.CreateRun();

                            using (var image = System.Drawing.Image.FromStream(imageStream))
                            {
                                int imageWidth = image.Width;
                                int imageHeight = image.Height;
                                double aspectRatio = (double)imageWidth / imageHeight;
                                int newWidth = imageWidth;
                                int newHeight = imageHeight;
                                if (imageWidth > imageHeight)
                                {
                                    newWidth = 1400;
                                    newHeight = (int)(newWidth / aspectRatio);
                                }
                                else
                                {
                                    newHeight = 1400;
                                    newWidth = (int)(newHeight * aspectRatio);
                                }
                                imageStream.Seek(0, SeekOrigin.Begin);
                                titleRunPicture.AddPicture(
                                    imageStream,
                                    (int)PictureType.PNG,
                                    "logo.png",
                                    Units.ToEMU(newWidth / 12),
                                    Units.ToEMU(newHeight / 12)
                                );
                            }
                        }

                    }
                }
            }
            using (MemoryStream memoryStream = new MemoryStream())
            {
                doc.Write(memoryStream);
                return memoryStream.ToArray();
            };
        }
        public byte[] GenerateFlashcardsAsPdf(Flashcards flashcards)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var documentNew = new CenteredTableDocument(flashcards);
            byte[] pdfBytes = documentNew.GeneratePdf();
            return pdfBytes;
        }
    }

    public class CenteredTableDocument : IDocument
    {
        public Flashcards _flashcards;
        public string folderPath;
        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public CenteredTableDocument(Flashcards flashcards)
        {
            _flashcards = flashcards;
            folderPath = Path.Combine(Directory.GetCurrentDirectory(),"UserDataImage", "User" + flashcards.UserID, "Flashcard" + flashcards.FlashcardsID);
        }
        public void Compose(IDocumentContainer container)
        {
            
            container.Page(page =>
            {
                page.Margin(10);
                page.Size(PageSizes.A4);
                page.Content().Column(column =>
                {
                    column.Item().Element(header =>
                    {
                        header.AlignCenter().Text(_flashcards.Title)
                            .FontSize(20).SemiBold().FontColor(Colors.Black);
                    });

                    column.Item().AlignCenter().Element(ComposeTable);
                });
            });
        }
        void ComposeTable(IContainer container)
        {

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(220);
                    columns.ConstantColumn(220);
                });
                foreach (var card in _flashcards.Flashcard)
                {


                    if (string.IsNullOrEmpty(card.FileName))
                    {
                        table.Cell().Height(250).Padding(10).Border(1).BorderColor(Colors.Grey.Medium).AlignMiddle().AlignCenter().Text(card.Title).FontSize(18).Bold();
                    }
                    else
                    {
                        string imagePath = Path.Combine(folderPath, card.FileName);
                        if (File.Exists(imagePath))
                        {
                            table.Cell().Height(250).Padding(10).Border(1).BorderColor(Colors.Grey.Medium).Element(container =>
                            {
                                container.AlignMiddle().AlignCenter().Column(column =>
                                {
                                    column.Item().AlignCenter().Text(card.Title).FontSize(18).Bold();

                                    column.Item().Height(10);

                                    column.Item().AlignMiddle().AlignCenter().Element(imageContainer =>
                                    {
                                        imageContainer
                                            .Width(150)
                                            .Image(imagePath, ImageScaling.FitArea); 
                                    });
                                });
                            });
                        }

                        else
                        {
                            table.Cell().Height(250).Padding(10).Border(1).BorderColor(Colors.Grey.Medium).AlignMiddle().AlignCenter().Text(card.Description).FontSize(18).Bold();

                        }

                    }
                    table.Cell().Height(250).Padding(10).Border(1).BorderColor(Colors.Grey.Medium).AlignMiddle().AlignCenter().Text(card.Description).FontSize(18).Bold();
                }
            });
        }
    }
}
