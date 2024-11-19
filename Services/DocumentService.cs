
using DocumentFormat.OpenXml.Drawing.Charts;
using EduCraftAPI.Entities.Flashcards;
using Microsoft.AspNetCore.Mvc;
using NPOI.Util;
using NPOI.XWPF.UserModel;
using System.IO;  // Upewnij się, że masz właściwą przestrzeń nazw

using Microsoft.AspNetCore.Mvc;
using SkiaSharp;

namespace EduCraftAPI.Services
{
    public interface IDocumentService
    {
        byte[] GenerateFlashcards(Flashcards flashcards);
    }

    public class DocumentService : IDocumentService
    {
        private readonly IFileService _fileService;

        public DocumentService(IFileService fileService)
        {
            _fileService = fileService;
        }

        public byte[] GenerateFlashcards(Flashcards flashcards)
        {
            // Określenie ścieżki do folderu na obrazki
            var folderPath = Path.Combine("UserDataImage", "User" + flashcards.UserID, "Flashcard" + flashcards.FlashcardsID);

            // Tworzymy nowy dokument Word (DOCX) za pomocą NPOI
            XWPFDocument doc = new XWPFDocument();

            // Dodajemy tytuł dokumentu
            XWPFParagraph titleParagraph = doc.CreateParagraph();
            titleParagraph.Alignment = ParagraphAlignment.CENTER;
            XWPFRun titleRun = titleParagraph.CreateRun();
            titleRun.SetText("Flashcards for User " + flashcards.UserID);
            titleRun.IsBold = true;
            titleRun.FontSize = 16;

            // Iterujemy przez każdą fiszkę
            foreach (var card in flashcards.Flashcard)
            {
                // Dodajemy nazwę pliku jako nagłówek
                XWPFParagraph fileNameParagraph = doc.CreateParagraph();
                fileNameParagraph.Alignment = ParagraphAlignment.LEFT;
                XWPFRun fileNameRun = fileNameParagraph.CreateRun();
                fileNameRun.SetText(card.FileName);
                fileNameRun.IsBold = true;
                fileNameRun.FontSize = 14;

                // Dodajemy opis fiszki
                XWPFParagraph descriptionParagraph = doc.CreateParagraph();
                XWPFRun descriptionRun = descriptionParagraph.CreateRun();
                descriptionRun.SetText(card.Description);
                descriptionRun.FontSize = 12;

                // Jeżeli karta ma przypisany obraz, dodajemy go do dokumentu
                if (!string.IsNullOrEmpty(card.FileName))
                {
                    var imagePath = Path.Combine(folderPath, card.FileName);

                    if (System.IO.File.Exists(imagePath))
                    {
                        // Dodajemy obraz do dokumentu w odpowiednim bloku
                        using (FileStream imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                        {

                            // Dodajemy obraz do dokumentu
                            XWPFRun pictureRun = descriptionParagraph.CreateRun();
                            pictureRun.AddPicture(imageStream, 6, "logo.png", Units.ToEMU(200), Units.ToEMU(200));
                            // Strumień imageStream jest automatycznie zamykany po zakończeniu używania
                        }
                    }
                    else
                    {
                        // Jeśli plik obrazu nie istnieje, możemy dodać komunikat (opcjonalnie)
                        XWPFRun errorRun = descriptionParagraph.CreateRun();
                        errorRun.SetText("[Image not found: " + card.FileName + "]");
                        errorRun.FontSize = 12;
                    }
                }

                // Dodajemy odstęp przed następną kartą
                XWPFParagraph spacerParagraph = doc.CreateParagraph();
                spacerParagraph.SpacingAfter = 15; // Dodajemy odstęp przed następną kartą
            }
            // Zapisujemy dokument do pliku
            using (FileStream fileStream = new FileStream("filePath.docx", FileMode.Create, FileAccess.Write))
            {
                doc.Write(fileStream);  // Używamy metody Write do zapisania dokumentu na dysk
            }


            using (MemoryStream memoryStream = new MemoryStream())
            {
                
                doc.Write(memoryStream);  // Używamy metody Write do zapisania dokumentu na dysk
                byte[] fileBytes = memoryStream.ToArray(); // Konwertuj do tablicy bajtów
                return fileBytes; // Zwróć tablicę bajtów

            }
           ;
            
        }
    }
}
