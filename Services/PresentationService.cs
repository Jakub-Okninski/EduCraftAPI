using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using A = DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace EduCraftAPI.Models
{
    // Twoje modele danych: Presentation, Slide, Element itd.
    // ... (Pomijam, bo już je dostarczyłeś)
}

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
            using (var memoryStream = new MemoryStream())
            {
                using (PresentationDocument presentationDocument = PresentationDocument.Create(memoryStream, PresentationDocumentType.Presentation))
                {
                    // Utworzenie części prezentacji
                    PresentationPart presentationPart = presentationDocument.AddPresentationPart();
                    presentationPart.Presentation = new Presentation();

                    // Utworzenie części głównej slajdu
                    SlideMasterPart slideMasterPart = presentationPart.AddNewPart<SlideMasterPart>();
                    slideMasterPart.SlideMaster = new SlideMaster(new CommonSlideData(new ShapeTree()));
                    slideMasterPart.SlideMaster.Append(new SlideLayoutIdList());

                    // Utworzenie układu slajdu
                    SlideLayoutPart slideLayoutPart = slideMasterPart.AddNewPart<SlideLayoutPart>();
                    slideLayoutPart.SlideLayout = new SlideLayout(new CommonSlideData(new ShapeTree()));

                    // Powiązanie układu slajdu z częścią główną slajdu
                    slideMasterPart.SlideMaster.SlideLayoutIdList = new SlideLayoutIdList(new SlideLayoutId() { Id = 1U, RelationshipId = slideMasterPart.GetIdOfPart(slideLayoutPart) });

                    // Powiązanie części głównej slajdu z częścią prezentacji
                    presentationPart.Presentation.SlideMasterIdList = new SlideMasterIdList(new SlideMasterId() { Id = 1U, RelationshipId = presentationPart.GetIdOfPart(slideMasterPart) });

                    // Utworzenie listy identyfikatorów slajdów
                    SlideIdList slideIdList = presentationPart.Presentation.AppendChild(new SlideIdList());

                    // Iteracja przez slajdy i tworzenie nowego slajdu dla każdego z nich
                    foreach (var slideData in presentationData.Slides)
                    {
                        SlidePart slidePart = presentationPart.AddNewPart<SlidePart>();
                        slidePart.Slide = new Slide(new CommonSlideData(new ShapeTree()));

                        // Dodawanie zawartości slajdu
                        AddSlideContent(slidePart, slideData);
                        UInt32Value id = slideData.Id!=null ? UInt32Value.FromUInt32((uint)slideData.Id) : UInt32Value.FromUInt32(0);

                        // Dodanie slajdu do listy identyfikatorów slajdów
                        slideIdList.Append(new SlideId() { Id = (UInt32Value)(256 + id), RelationshipId = presentationPart.GetIdOfPart(slidePart) });

                        // Zapisanie slajdu
                        slidePart.Slide.Save();
                    }

                    // Zapisanie części
                    slideMasterPart.SlideMaster.Save();
                    slideLayoutPart.SlideLayout.Save();
                    presentationPart.Presentation.Save();
                }

                // Zapisanie pliku na pulpicie na potrzeby testów
                File.WriteAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "presentation.pptx"), memoryStream.ToArray());

                // Zwrócenie pliku jako wynik
                return new FileContentResult(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.presentationml.presentation")
                {
                    FileDownloadName = $"{presentationData.Title}.pptx"
                };
            }
        }

        private void AddSlideContent(SlidePart slidePart, EduCraftAPI.Models.Slide slideData)
        {
            var shapeTree = slidePart.Slide.CommonSlideData.ShapeTree;

            // Dodawanie tytułu slajdu
            if (!string.IsNullOrEmpty(slideData.Title))
            {
                var titleShape = shapeTree.AppendChild(new Shape());
                var titleTextBody = titleShape.AppendChild(new A.TextBody());
                var titleParagraph = titleTextBody.AppendChild(new A.Paragraph());
                var titleRun = new A.Run();
                titleRun.Append(new A.Text(slideData.Title));
                titleParagraph.Append(titleRun);
            }

            // Dodawanie elementów slajdu
            if (slideData.Elements != null)
            {
                foreach (var element in slideData.Elements)
                {
                    var contentShape = shapeTree.AppendChild(new Shape());
                    var contentTextBody = contentShape.AppendChild(new A.TextBody());
                    var contentParagraph = contentTextBody.AppendChild(new A.Paragraph());
                    foreach (var op in element.Ops)
                    {
                        var contentRun = new A.Run();
                        contentRun.Append(new A.Text(op.Insert));
                        contentParagraph.Append(contentRun);
                    }
                }
            }
        }
    }
}
