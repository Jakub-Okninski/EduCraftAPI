using EduCraftAPI.Entities.Flashcards;
using EduCraftAPI.Entities.Quiz;
using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.Util;
using NPOI.XWPF.UserModel;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

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
            var folderPath = Path.Combine("UserDataImage", "User" + quiz.UserID, "Quiz" + quiz.QuizID);
            XWPFDocument doc = new XWPFDocument();
            XWPFParagraph mainParagraph = doc.CreateParagraph();
            mainParagraph.Alignment = NPOI.XWPF.UserModel.ParagraphAlignment.CENTER;
            XWPFRun mainRun = mainParagraph.CreateRun();
            mainRun.SetText("Quiz: " + quiz.Name);
            mainRun.IsBold = true;
            mainRun.FontSize = 16;

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
            var folderPath = Path.Combine("UserDataImage", "User" + flashcards.UserID, "Flashcard" + flashcards.FlashcardsID);

            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            XFont titleFont = new XFont("Arial", 12);
            XFont descriptionFont = new XFont("Arial", 12);

            double cellWidth = 200;
            double cellHeight = 200;
            double xPosition = 100;
            double yPosition = 50;
            double rowHeight = 200;

            double marginBottom = 5;
            double pageWidth = page.Width;
            double pageHeight = page.Height;

            double maxImageWidth = cellWidth * 0.6;
            double maxImageHeight = rowHeight * 0.6;
            double imageHolderHeight = rowHeight * 0.6;

            foreach (var card in flashcards.Flashcard)
            {
                gfx.DrawRectangle(XPens.Black, xPosition, yPosition, cellWidth, rowHeight); 
                gfx.DrawRectangle(XPens.Black, xPosition + cellWidth, yPosition, cellWidth, rowHeight);


                string text = card.Description;

                string[] lines = text.Split(new[] { "\n" }, StringSplitOptions.None);
                int count = lines.Length;
                if (count <1) {
                    count = 1;
                    continue;
                }
                var rect = new XRect(xPosition + cellWidth, yPosition, cellWidth, rowHeight);

                double lineHeight = descriptionFont.Height;

                double totalTextHeight = count * lineHeight;

                double currentY = rect.Y + (rect.Height - totalTextHeight) / 2;

                foreach (string line in lines)
                {
                    gfx.DrawString(
                        line,
                        descriptionFont,
                        XBrushes.Black,
                        new XRect(rect.X, currentY, rect.Width, lineHeight),
                        XStringFormats.Center
                    );

                    currentY += lineHeight;
                }


                bool imageIsAdded = false;

                if (!string.IsNullOrEmpty(card.FileName))
                {
                    string imagePath = Path.Combine(folderPath, card.FileName);
                    if (File.Exists(imagePath))
                    {
                        XImage image = XImage.FromFile(imagePath);
                        double aspectRatio = image.PixelWidth / (double)image.PixelHeight;
                        double newWidth = maxImageWidth;
                        double newHeight = newWidth / aspectRatio;
                        if (newHeight > maxImageHeight)
                        {
                            newHeight = maxImageHeight;
                            newWidth = newHeight * aspectRatio;
                        }
                        else if (newWidth > maxImageWidth)
                        {
                            newWidth = maxImageWidth;
                            newHeight = newHeight * aspectRatio;
                        }
                        double imageXPosition = xPosition + (cellWidth - newWidth) / 2;
                        double imageYPosition = yPosition + rowHeight * 0.3 + (imageHolderHeight - newHeight) / 2;
                        gfx.DrawImage(image, imageXPosition, imageYPosition, newWidth, newHeight);
                        imageIsAdded = true;
                    }
                }

                if (!imageIsAdded)
                {
                    string titleText = card.Title;
                    string[] titleLines = titleText.Split(new[] { "\n" }, StringSplitOptions.None);
                    int titleLineCount = titleLines.Length;
                    if (titleLineCount < 1)
                    {
                        titleLineCount = 1;
                        continue;
                    }
                    var titleRect = new XRect(xPosition, yPosition, cellWidth, rowHeight);
                    double titleLineHeight = titleFont.Height;
                    double totalTitleHeight = titleLineCount * titleLineHeight;
                    double titleStartY = titleRect.Y + (titleRect.Height - totalTitleHeight) / 2;
                    foreach (string titleLine in titleLines)
                    {
                        gfx.DrawString(
                            titleLine,
                            titleFont,
                            XBrushes.Black,
                            new XRect(titleRect.X, titleStartY, titleRect.Width, titleLineHeight),
                            XStringFormats.Center
                        );
                        titleStartY += titleLineHeight;
                    }
                }
                else
                {
                    string titleText = card.Title;
                    string[] titleLines = titleText.Split(new[] { "\n" }, StringSplitOptions.None);
                    int titleLineCount = titleLines.Length;
                    if (titleLineCount < 1)
                    {
                        titleLineCount = 1;
                        continue;
                    }
                    var titleRect = new XRect(xPosition, yPosition, cellWidth, rowHeight * 0.3);
                    double titleLineHeight = titleFont.Height;
                    double totalTitleHeight = titleLineCount * titleLineHeight;
                    double titleStartY = titleRect.Y + (titleRect.Height - totalTitleHeight) / 2;
                    foreach (string titleLine in titleLines)
                    {
                        gfx.DrawString(
                            titleLine,
                            titleFont,
                            XBrushes.Black,
                            new XRect(titleRect.X, titleStartY, titleRect.Width, titleLineHeight),
                            XStringFormats.Center
                        );
                        titleStartY += titleLineHeight;
                    }

                }

                yPosition += rowHeight + 10;
                if (yPosition + rowHeight > pageHeight - marginBottom)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    yPosition = 50;
                }
            }
            using (MemoryStream stream = new MemoryStream())
            {
                document.Save(stream);
                return stream.ToArray();
            }
        }

    }
}
