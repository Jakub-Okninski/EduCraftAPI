
using DocumentFormat.OpenXml.Spreadsheet;
using EduCraftAPI.Entities.Flashcards;
using EduCraftAPI.Entities.Quiz;
using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.Util;
using NPOI.XWPF.UserModel;


namespace EduCraftAPI.Services
{
    public interface IDocumentService
    {
        public byte[] GenerateFlashcards(Flashcards flashcards);
        public byte[] GenerateQuiz(Quiz quiz, bool withCorrect = false);
    }

    public class DocumentService : IDocumentService
    {
        private readonly IFileService _fileService;

        public DocumentService(IFileService fileService)
        {
            _fileService = fileService;
        }
        public byte[] GenerateQuiz(Quiz quiz, bool withCorrect = false)
        {
            var folderPath = Path.Combine("UserDataImage", "User" + quiz.UserID, "Quiz" + quiz.QuizID);

            XWPFDocument doc = new XWPFDocument();
            XWPFParagraph mainParagraph = doc.CreateParagraph();
            mainParagraph.Alignment = ParagraphAlignment.CENTER;

            XWPFRun mainRun = mainParagraph.CreateRun();
            mainRun.SetText("Quiz: " + quiz.Name);
            mainRun.IsBold = true;
            mainRun.FontSize = 16;
            foreach (var question in quiz.Questions)
            {

                XWPFParagraph questionParagraph = doc.CreateParagraph();
                questionParagraph.Alignment = ParagraphAlignment.CENTER;

                XWPFRun questionRun = questionParagraph.CreateRun();
                questionRun.SetText("Pytanie: " + question.Name);
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
                            titleParagraphPicture.Alignment = ParagraphAlignment.CENTER;
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
                                    newWidth = 2000;
                                    newHeight = (int)(newWidth / aspectRatio);
                                }
                                else
                                {
                                    newHeight = 2000;
                                    newWidth = (int)(newHeight * aspectRatio);
                                }
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
                XWPFTable table = doc.CreateTable(1, question.Answers.Count);
                table.GetRow(0).Height = 1500;

                var ctTable = table.GetCTTbl();
                var tblProperties = ctTable.AddNewTblPr();
                tblProperties.jc = new CT_Jc { val = ST_Jc.center };

                var tblLayout1 = table.GetCTTbl().tblPr.AddNewTblLayout();
                tblLayout1.type = ST_TblLayoutType.@fixed;
             
                int colIndex = 0;
                ulong size = 2300;
                if(question.Answers.Count > 5)
                {
                    size = (ulong)(11500 / question.Answers.Count);
                }
                foreach (var answer in question.Answers)
                {
                    XWPFTableCell cell = table.GetRow(0).GetCell(colIndex);
                    table.SetColumnWidth(colIndex, size);
                    cell.SetVerticalAlignment(XWPFTableCell.XWPFVertAlign.CENTER);

                    XWPFParagraph cellParagraph = cell.Paragraphs[0];
                    cellParagraph.Alignment = ParagraphAlignment.CENTER; 

                    XWPFRun cellRun = cellParagraph.CreateRun();
                    char letter = (char)('A' + colIndex);
                    cellRun.SetText("("+letter+ ")  "+answer.Name);
                    cellRun.FontSize = 12;

                    if (answer.IsCorrect && withCorrect)
                    {
                        cellRun.IsBold = true;
                        cellRun.SetColor("008000"); 
                    }
                    colIndex++;
                }

               doc.CreateParagraph();
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
            XWPFParagraph mainParagraph = doc.CreateParagraph();
            mainParagraph.Alignment = ParagraphAlignment.CENTER;
            
            XWPFRun mainRun = mainParagraph.CreateRun();
            mainRun.SetText("Fiszki: " + flashcards.Title);
            mainRun.IsBold = true;
            mainRun.FontSize = 16;
            foreach (var card in flashcards.Flashcard)
            {
                
                XWPFTable table = doc.CreateTable(1, 2);
                var ctTable = table.GetCTTbl();
                var tblProperties = ctTable.AddNewTblPr();
                tblProperties.jc = new CT_Jc { val = ST_Jc.center };


                var tblLayout1 = table.GetCTTbl().tblPr.AddNewTblLayout();
                tblLayout1.type = ST_TblLayoutType.@fixed;
                table.SetColumnWidth(0, 3000);
                table.SetColumnWidth(1, 3000);
          
                XWPFTableCell titleCell = table.GetRow(0).GetCell(0);
                titleCell.SetVerticalAlignment(XWPFTableCell.XWPFVertAlign.CENTER);
                XWPFParagraph titleParagraph = titleCell.Paragraphs[0];
                titleParagraph.Alignment = ParagraphAlignment.CENTER;
                XWPFRun titleRun = titleParagraph.CreateRun();
                titleRun.SetText(card.Title);
                titleRun.IsBold = true;
                titleRun.FontSize = 14;


                XWPFTableCell descriptionCell = table.GetRow(0).GetCell(1);
                table.GetRow(0).Height = 3500;
                descriptionCell.SetVerticalAlignment(XWPFTableCell.XWPFVertAlign.CENTER);
                XWPFParagraph descriptionParagraph = descriptionCell.Paragraphs[0];
                descriptionParagraph.Alignment = ParagraphAlignment.CENTER;

                XWPFRun descriptionRun = descriptionParagraph.CreateRun();
                descriptionRun.SetText(card.Description);
                descriptionRun.IsBold = true;
                descriptionRun.FontSize = 14;

                if (!string.IsNullOrEmpty(card.FileName))
                {
                    string imagePath = Path.Combine(folderPath, card.FileName);

                    if (File.Exists(imagePath)) 
                    {
                        using (FileStream imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                        {
                            XWPFParagraph titleParagraphPicture = titleCell.AddParagraph();

                            titleParagraphPicture.Alignment = ParagraphAlignment.CENTER;
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
                                    Units.ToEMU(newWidth/10),  
                                    Units.ToEMU(newHeight/10)  
                                );
                            }
                        }
                    
                    } else
                    {
                        descriptionRun.SetText("[Image not found: " + card.FileName + "]");
                        descriptionRun.FontSize = 12;
                        descriptionRun.IsItalic = true; 
                    }
                }
            }
            using (MemoryStream memoryStream = new MemoryStream())
            {
                doc.Write(memoryStream);  
                return memoryStream.ToArray(); 
            };       
        }
    }
}
