using DocumentFormat.OpenXml.Drawing.Charts;
using EduCraftAPI.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using Position = EduCraftAPI.Models.Position;
using Presentation = EduCraftAPI.Models.Presentation;
using Size = EduCraftAPI.Models.Size;
using Slide = EduCraftAPI.Models.Slide;

namespace EduCraftAPI.Services
{
    public interface IGenerateService
    {
        public Task<Models.Presentation> generatePresentationDataText(string descriptionPresentation, string titleMain);
        public Task<Presentation> generatePresentationDataImage(Presentation presentation, int UserID, int fileID, string titleMain);
        public Task<String> generateAnswer(string prompt);
    }
    public class GenerateService : IGenerateService
    {

        private readonly IFileService _fileService;
        private readonly string _apiKey;

        public GenerateService(IFileService fileService, IUserContextService userContextService, IConfiguration configuration)
        {
            _fileService = fileService;
            _apiKey = configuration["ApiKeys:myKey"]!;
        } 

        public async Task<String> generateAnswer(string prompt)
        {

       


            string apiUrl = "https://api.openai.com/v1/chat/completions";
            Debug.WriteLine(prompt);

            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                new { role = "user", content = $"Jesteś asystentem. Odpowiedz w max 3 zdaniach. Tresc: {prompt}" }
            },
                max_tokens = 200,
                temperature = 1.0,
                top_p = 1.0,
                frequency_penalty = 0.0,
                presence_penalty = 0.0
            };


            string jsonRequest = JsonConvert.SerializeObject(requestBody);
            string input = "Wystąpił błąd.";
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
                var response = await client.PostAsync(apiUrl, new StringContent(jsonRequest, Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    dynamic responseObject = JsonConvert.DeserializeObject(responseContent);
                    input = responseObject.choices[0].message.content;

                    Debug.WriteLine("Wygenerowane dane:");
                    Debug.WriteLine(input);
               
                }
                else
                {
                    Debug.WriteLine($"Błąd: {response.StatusCode}");
                }
            }
            return input;


        }

        public async Task<Presentation> generatePresentationDataImage(Presentation presentation, int UserID, int fileID, string titleMain)
        {
               
            List<String> imageUrls = new List<String>();

          
            string apiUrl = "https://api.openai.com/v1/images/generations";

            var requestBody = new
            {
                model = "dall-e-2",
                prompt = "Zdjecie na temat: "+ titleMain,
                n = 1,
                size = "256x256"
            };


            string jsonRequest = JsonConvert.SerializeObject(requestBody);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

                var response = await client.PostAsync(apiUrl, new StringContent(jsonRequest, Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<ImageResponse>(responseContent);
                    foreach (var image in responseObject.Data)
                    {
                        Console.WriteLine($"Generated image URL: {image.Url}");
                        imageUrls.Add(image.Url);

                    }

                
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                }
            }

            foreach (var slide in presentation.Slides)
            {
                if (slide.Id % 2 == 0 && imageUrls.Count>=1) {

                    Element element = new Element();

                    element= await _fileService.SaveGeneratedImagePresentation(UserID, fileID, imageUrls[0],element);

                    int newID = 1;
                    foreach (var el in slide.Elements)
                    {
                        if (el.Id >= newID)
                        {
                            newID = el.Id;
                        }
                        if (el.Id == 2)
                        {
                            el.Size.Width = 550;
                            el.Size.Height = 330;

                        }

                    }
                    element.Id = newID + 1;
                    element.Type = "image";
                
                    element.Size = new Size()
                    {
                        Width = 275,
                        Height = 275
                    };


                    element.Position = new Position()
                    {
                        Top = 160,
                        Left = 675,
                    };
                    slide.Elements.Add(element);

                }
            }
            return presentation;
        }
        public async Task<Models.Presentation> generatePresentationDataText(string descriptionPresentation, string titleMain)
        {
            Presentation presentationData = new Presentation();
            presentationData.Slides = new List<Slide>();

           
            string apiUrl = "https://api.openai.com/v1/chat/completions";
            Debug.WriteLine(descriptionPresentation);

            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                new { role = "user", content = $"Utwórz 4 slajdy na podany temat, temat to: {descriptionPresentation}. Wymagana struktura to: tytuł slajdu $$$ zawartość slajdu ###. Maksymalnie trzy zdania zawartości. tytuł slajdu od zawartość slajdu oddziel $$$ a poszczególne slajdy ###, nic poza tym" }
            },
                max_tokens = 580,
                temperature = 1.0,
                top_p = 1.0,
                frequency_penalty = 0.0,
                presence_penalty = 0.0
            };

           
            string jsonRequest = JsonConvert.SerializeObject(requestBody);
            string input = "";
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
                var response = await client.PostAsync(apiUrl, new StringContent(jsonRequest, Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    dynamic responseObject = JsonConvert.DeserializeObject(responseContent);
                    input = responseObject.choices[0].message.content;

                    Debug.WriteLine("Wygenerowane dane:");
                    Debug.WriteLine(input);
                }
                else
                {
                    Debug.WriteLine($"Błąd: {response.StatusCode}");
                }
            }


            Slide mainSlide = new Slide();
            mainSlide.Id = 1;
            mainSlide.Title = "Slajd " + 1;
            mainSlide.Elements = new List<Element>();

            Element elementMain= new Element();
            elementMain.Id = 1;
            elementMain.Type = "text";
            elementMain.Position = new Position()
            {
                Top = 210,
                Left = 62,
            };
            elementMain.Size = new Size()
            {
                Width = 900,
                Height = 100,
            };

            elementMain.Ops = new List<Op>();
            Op opMain = new Op();
            Op opMain2 = new Op();


            opMain2.Insert = "\n";
            opMain.Insert = titleMain;

            opMain2.Attributes = new Attributes() { Align = "center", Header = 1};

            elementMain.Ops.Add(opMain);
            elementMain.Ops.Add(opMain2);


            mainSlide.Elements.Add(elementMain);
            presentationData.Slides.Add(mainSlide);


            int slideID = 2;
            var slides = input.Split("###", StringSplitOptions.RemoveEmptyEntries);
            foreach (var slide in slides)
            {
                if (slide.Contains("$$$"))
                {         
                    var sections = slide.Split("$$$", StringSplitOptions.RemoveEmptyEntries);

                        if (sections.Length == 2)
                        {
                            string title = sections[0].Trim();
                            string content = sections[1].Trim();

                            Debug.WriteLine("...");
                            Debug.WriteLine("...");
                            Debug.WriteLine(title);
                            Debug.WriteLine(content);
                            Debug.WriteLine("...");
                            Debug.WriteLine("...");

                            Slide newSlide = new Slide();
                            newSlide.Id = slideID;
                            newSlide.Title = "Slajd " + slideID;
                            slideID++;

                            newSlide.Elements = new List<Element>();
                           
                            Element elementTitle = new Element();
                            elementTitle.Type = "text";
                            elementTitle.Position = new Position()
                            {
                                Top = 30,
                                Left = 62,
                            };
                            elementTitle.Size = new Size()
                            {
                                Width = 900,
                                Height = 100,
                            };

                            elementTitle.Id = 1;
                            elementTitle.Ops = new List<Op>();
                            Op op = new Op();

                            op.Attributes = new Attributes() { Bold = true };
                            op.Insert = title;
                            Op opp = new Op();

                            opp.Attributes = new Attributes() { Header = 1 };
                            opp.Insert = "\n";

                            elementTitle.Ops.Add(op);
                            elementTitle.Ops.Add(opp);

                            Element elementData = new Element();
                            elementData.Position = new Position()
                            {
                                Top = 130,
                                Left = 62,
                            };
                            elementData.Size = new Size()
                            {
                                Width = 900,
                                Height = 330,
                            };
                            elementData.Type = "text";
                            elementData.Id = 2;
                            elementData.Ops = new List<Op>();
                            Op op2 = new Op();
                            op2.Insert = content;
                            elementData.Ops.Add(op2);
                            newSlide.Elements.Add(elementTitle);
                            newSlide.Elements.Add(elementData);
                            presentationData.Slides.Add(newSlide);
                        }
                }
            }

            return presentationData;
         
        }
    }
}
