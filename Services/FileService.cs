using DocumentFormat.OpenXml.Office2010.Excel;
using EduCraftAPI.Entities.Flashcards;
using EduCraftAPI.Entities.Quiz;
using EduCraftAPI.Migrations;
using EduCraftAPI.Models;
using System.Diagnostics;
using System.Drawing;
using System.Xml.Serialization;

namespace EduCraftAPI.Services
{
    public interface IFileService
    {
        public void SavePresentationToXml(Models.Presentation presentation, int filename);
        public Models.Presentation? LoadPresentationFromXml(int filename);
        public string SaveImagePresentation(int userID, int fileID, IFormFile file, string newFileName);
        public void RemoveImageQuiz(int userID, int QuizID, string fileName);
        public string SaveFileImgQuiz(int userID, int QuizID, IFormFile file);
        public string getBase64(IFormFile file);
        public Quiz AddQuestionImg(Quiz quiz);
        public Flashcards AddFlashCardsImg(Flashcards flashcards);
        public string SaveFileImgFlashCard(int userID, int FlashCardID, IFormFile file);
        public void RemoveImageFlashCard(int userID, int FlashCardID, string fileName);
        public string[] getAllFIle(int UserID, string Type, int ItemID);


    }
    public class FileService : IFileService
    {
        public void SavePresentationToXml(Models.Presentation presentation, int filename)
        {   
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Presentations");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string fullFilePath = Path.Combine(directoryPath, filename+"");
            var xmlSerializer = new XmlSerializer(typeof(Models.Presentation));
            using (var stream = new FileStream(fullFilePath, FileMode.Create))
            using (var writer = new StreamWriter(stream))
            {
                xmlSerializer.Serialize(writer, presentation);
            } 
        }
        public Models.Presentation? LoadPresentationFromXml(int filename)
        {
            try
            {
                string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Presentations");
                string fullFilePath = Path.Combine(directoryPath, filename + "");

                var xmlSerializer = new XmlSerializer(typeof(Models.Presentation));
                using (var stream = new FileStream(fullFilePath, FileMode.Open))
                using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                {
                    return (Models.Presentation)xmlSerializer.Deserialize(reader);
                }
            }
            catch (Exception ex) {
                
                return null;
            }
         
        }
        private Image ResizeImage(IFormFile file, int maxWidth, int maxHeight)
        {
            using (var image = Image.FromStream(file.OpenReadStream()))
            {

                int newWidth = image.Width;
                int newHeight = image.Height;

                if (image.Width > maxWidth || image.Height > maxHeight)
                {
                    float ratioX = (float)maxWidth / image.Width;
                    float ratioY = (float)maxHeight / image.Height;
                    float ratio = Math.Min(ratioX, ratioY);

                    newWidth = (int)(image.Width * ratio);
                    newHeight = (int)(image.Height * ratio);
                }

                
                var resizedImage = new Bitmap(newWidth, newHeight);
                using (var graphics = Graphics.FromImage(resizedImage))
                {
                    graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    graphics.DrawImage(image, 0, 0, newWidth, newHeight);
                }
                return resizedImage;
            } 
        }
        private string ConvertImageToBase64(Image image, string fileName)
        {
            using (var memoryStream = new MemoryStream())
            {
                image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                string base64Image = Convert.ToBase64String(memoryStream.ToArray());

                string fileNameLower = fileName.ToLower();
                if (fileNameLower.EndsWith(".jpg") || fileNameLower.EndsWith(".jpeg"))
                {
                    base64Image = "data:image/jpeg;base64," + base64Image;
                }
                else if (fileNameLower.EndsWith(".png"))
                {
                    base64Image = "data:image/png;base64," + base64Image;
                }
                else if (fileNameLower.EndsWith(".gif"))
                {
                    base64Image = "data:image/gif;base64," + base64Image;
                }

                return base64Image;
            }
        }
        private void SaveImageToDisk(Image image, string filePath)
        {
            image.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
        }
        public string SaveImagePresentation(int userID , int fileID, IFormFile file, string newFileName)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "UserDataImage", "User" + userID);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            uploadsFolder = Path.Combine(uploadsFolder, "Presentation" + fileID);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, newFileName);

            var newFile = ResizeImage(file, 854,480);
            SaveImageToDisk(newFile, filePath);

            return ConvertImageToBase64(newFile, newFileName);
        }
        public void RemoveImageQuiz(int userID, int QuizID, string fileName)
        {
            var filePath = Path.Combine("UserDataImage", "User" + userID, "Quiz" + QuizID, fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }   
        }
        public string SaveFileImgQuiz(int userID, int QuizID, IFormFile file)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "UserDataImage", "User" + userID);

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            uploadsFolder = Path.Combine(uploadsFolder, "Quiz" + QuizID);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }


            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            return fileName; 
        }
        public string getBase64(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);

                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                string mimeType = fileExtension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".bmp" => "image/bmp",
                    ".tiff" => "image/tiff",
                    _ => "application/octet-stream" 
                };

                return $"data:{mimeType};base64,{Convert.ToBase64String(memoryStream.ToArray())}";
            }
        }
        public Quiz AddQuestionImg(Quiz quiz)
        {
            foreach (Question question in quiz.Questions)
            {
                if (!string.IsNullOrEmpty(question.FileName))
                {
                    var filePath = Path.Combine("UserDataImage", "User" + quiz.UserID, "Quiz" + quiz.QuizID, question.FileName);
                    if (File.Exists(filePath))
                    {
                        question.FileContent = $"data:image/jpeg;base64,{Convert.ToBase64String(File.ReadAllBytes(filePath))}";
                    }
                    else
                    {
                        question.FileContent = null;
                    }
                }
            }
            return quiz;
        }
        public Flashcards AddFlashCardsImg(Flashcards flashcards)
        {
            foreach (Flashcard flashcard in flashcards?.Flashcard)
            {

                if (!string.IsNullOrEmpty(flashcard.FileName))
                {
                    var filePath = Path.Combine("UserDataImage", "User" + flashcards.UserID, "Flashcard" + flashcards.FlashcardsID, flashcard.FileName);
                    if (File.Exists(filePath))
                    {
                        flashcard.FileContent = $"data:image/jpeg;base64,{Convert.ToBase64String(File.ReadAllBytes(filePath))}";
                    }
                    else
                    {
                        flashcard.FileContent = null;
                    }
                }
            }
            return flashcards;
        }
        public string SaveFileImgFlashCard(int userID, int FlashCardID, IFormFile file)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "UserDataImage", "User" + userID);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            uploadsFolder = Path.Combine(uploadsFolder, "Flashcard" + FlashCardID);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            return fileName;
        }
        public void RemoveImageFlashCard(int userID, int FlashCardID, string fileName)
        {
            var filePath = Path.Combine("UserDataImage", "User" + userID, "Flashcard" + FlashCardID, fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
        public string[] getAllFIle(int UserID, string Type, int ItemID)
        {

            var filePath = Path.Combine("UserDataImage", "User" + UserID, Type + ItemID);

            if (Directory.Exists(filePath))
            {

                string[] files = Directory.GetFiles(filePath);

                string[] base64Files = new string[files.Length];

                for (int i = 0; i < files.Length; i++)
                {
                    try
                    {
                        var fileName = Path.GetFileName(files[i]);

                        string fileNameLower = fileName.ToLower();
                        if (fileNameLower.EndsWith(".jpg") || fileNameLower.EndsWith(".jpeg"))
                        {
                            base64Files[i] = "data:image/jpeg;base64," + Convert.ToBase64String(File.ReadAllBytes(Path.Combine(filePath, fileName)));
                        }
                        else if (fileNameLower.EndsWith(".png"))
                        {
                            base64Files[i] = "data:image/png;base64," + Convert.ToBase64String(File.ReadAllBytes(Path.Combine(filePath, fileName)));
                        }
                        else if (fileNameLower.EndsWith(".gif"))
                        {
                            base64Files[i] = "data:image/gif;base64," + Convert.ToBase64String(File.ReadAllBytes(Path.Combine(filePath, fileName)));
                        }
                        else
                        {
                            base64Files[i] = null;
                        }

                    }
                    catch (Exception e)
                    {

                        base64Files[i] = null;
                    }

                }
                return base64Files;
            }
            else
            {
                return null;
            }

        }


    }
}
