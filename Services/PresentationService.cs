using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Mvc;
using DocumentFormat.OpenXml.Drawing;
using NonVisualGroupShapeProperties = DocumentFormat.OpenXml.Drawing.NonVisualGroupShapeProperties;
using NonVisualDrawingProperties = DocumentFormat.OpenXml.Drawing.NonVisualDrawingProperties;
using NonVisualShapeProperties = DocumentFormat.OpenXml.Drawing.NonVisualShapeProperties;
using NonVisualShapeDrawingProperties = DocumentFormat.OpenXml.Drawing.NonVisualShapeDrawingProperties;
using TextBody = DocumentFormat.OpenXml.Drawing.TextBody;
using Shape = DocumentFormat.OpenXml.Drawing.Shape;



namespace EduCraftAPI.Services
{
    public interface IPresentationService
    {
        FileResult GeneratePPTX(EduCraftAPI.Models.Presentation presentationData);
    }

    public class PresentationService : IPresentationService
    {
        public FileResult GeneratePPTX(EduCraftAPI.Models.Presentation presentationData)
        {
            string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test_presentation.pptx");

            // Tworzenie pliku PowerPoint
            using (PresentationDocument presentationDocument = PresentationDocument.Create(filePath, PresentationDocumentType.Presentation))
            {
                // Dodanie prezentacji
                PresentationPart presentationPart = presentationDocument.AddPresentationPart();
                presentationPart.Presentation = new Presentation();

                // Dodanie sekwencji slajdów do prezentacji
                presentationPart.Presentation.SlideIdList = new SlideIdList();

                // Dodanie slajdu
                Slide slide = new Slide(new CommonSlideData(new ShapeTree()));
                SlidePart slidePart = presentationPart.AddNewPart<SlidePart>();
                slidePart.Slide = slide;

                // Ustawienie wymaganych właściwości ShapeTree
                var shapeTree = slide.GetFirstChild<CommonSlideData>().ShapeTree;
                shapeTree.AppendChild(new NonVisualGroupShapeProperties(new NonVisualDrawingProperties() { Id = 1, Name = "" },
                    new DocumentFormat.OpenXml.Drawing.NonVisualGroupShapeDrawingProperties(),
                    new ApplicationNonVisualDrawingProperties()));
                shapeTree.AppendChild(new GroupShapeProperties());

                // Dodanie tekstu do slajdu
             
                // Utworzenie SlideId i dodanie go do SlideIdList
                SlideId slideId = new SlideId() { Id = (UInt32Value)256U, RelationshipId = presentationPart.GetIdOfPart(slidePart) };
                presentationPart.Presentation.SlideIdList.Append(slideId);

                // Zapisanie zmian
                presentationPart.Presentation.Save();
            }

            // Zwrócenie pliku jako wynik
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
           // File.Delete(filePath);  // Usunięcie pliku tymczasowego

            return new FileContentResult(fileBytes, "application/vnd.openxmlformats-officedocument.presentationml.presentation")
            {
                FileDownloadName = $"{presentationData.Title}.pptx"
            };
        }
    }
}
