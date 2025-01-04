using EduCraftAPI.Entities.Flashcards;
using EduCraftAPI.Entities.Quiz;
using EduCraftAPI.Entities.User;
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
        public Task<Models.Presentation> GeneratePresentationDataText(string descriptionPresentation, string titleMain, int countElements, User user);
        public Task<Presentation> GeneratePresentationDataImage(Presentation presentation, int UserID, int fileID, string titleMain);
        public Task<String> GenerateAnswer(string prompt);
        public Task<List<String>> GeneratePicture(string prompt, int userID);
        public Task<Quiz> GenerateQuizDataText(string descriptionQuiz, string titleMain, Quiz quiz, int countElements);
        public Task<Quiz> generateQuizDataImage(Quiz quiz, int UserID, int fileID, string titleMain);
        public Task<Flashcards> generateFlashcardsDataText(string descriptionFlashcards, string titleMain, Flashcards flashcards, int countElements);
        public Task<Flashcards> generateFlashcardsDataImage(Flashcards flashcards, int UserID, int fileID, string titleMain);
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
        public async Task<String> GenerateAnswer(string prompt)
        {

            string apiUrl = "https://api.openai.com/v1/chat/completions";
            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                new { role = "user", content = $"Jesteś asystentem. Odpowiedz w max 1 zdaniu. Tresc: {prompt}" }
            },
                max_tokens = 150,
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
                }
            }
            return input;
        }
        public async Task<List<String>> GeneratePicture(string prompt, int userID)
        {
            string apiUrl = "https://api.openai.com/v1/images/generations";

            var requestBody = new
            {
                model = "dall-e-2",
                prompt = "Temat zdjecia: " + prompt,
                n = 1,
                size = "256x256"
            };
            List<String> imageUrls = new List<String>();


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
                        imageUrls.Add(await _fileService.SaveGeneratedImage(userID, image.Url));
                        Console.WriteLine($"Generated image URL: {image.Url}");
                    }
                }
                else
                {
                    return null;
                }
            }
            return imageUrls;
        }
        public async Task<Presentation> GeneratePresentationDataImage(Presentation presentation, int UserID, int fileID, string titleMain)
        {
               
            List<String> imageUrls = new List<String>();

          
            string apiUrl = "https://api.openai.com/v1/images/generations";

            var requestBody = new
            {
                model = "dall-e-2",
                prompt = "Zdjecie na temat: "+ titleMain,
                n = 2,
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

            int actualAddImage = 0;

            foreach (var slide in presentation.Slides)
            {
                if (slide.Id % 2 == 0 && imageUrls.Count>=1 && presentation.Slides.Count!=slide.Id) {

                    Element element = new Element();
                    if(actualAddImage >= imageUrls.Count)
                    {
                        actualAddImage = 0;
                    }
                    element= await _fileService.SaveGeneratedImagePresentation(UserID, fileID, imageUrls[actualAddImage],element);
                    actualAddImage++;

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
                            el.Size.Height = 350;

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
        public async Task<Models.Presentation> GeneratePresentationDataText(string descriptionPresentation, string titleMain, int countElements, User user)
        {
            Presentation presentationData = new Presentation();
            presentationData.Slides = new List<Slide>();

            if(countElements <1 || countElements > 15)
            {
                countElements = 10;
            }
            string apiUrl = "https://api.openai.com/v1/chat/completions";
            Debug.WriteLine(descriptionPresentation);

            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                new { role = "user", content = $"Utwórz slajdy na podany temat, temat to: {descriptionPresentation}. Maksymalna liczna slajdów to:{countElements}. Wymagana struktura to: tytuł slajdu $$$ zawartość slajdu ###. Zawartość slajdu niech składa sie z 3 zdań złożonych. tytuł slajdu od zawartość slajdu oddziel $$$ a poszczególne slajdy ###, nic poza tym. Przykład schematu: Java $$$ To najpopularniejszy język programowania ### " }
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
            opMain.Insert = CapitalizeFirstLetter(titleMain);

            opMain2.Attributes = new Attributes() { Align = "center", Header = 1};

            elementMain.Ops.Add(opMain);
            elementMain.Ops.Add(opMain2);


            mainSlide.Elements.Add(elementMain);


            Element elementMain2 = new Element();
            elementMain2.Id = 2;
            elementMain2.Type = "text";
            elementMain2.Position = new Position()
            {
                Top = 480,
                Left = 600,
            };
            elementMain2.Size = new Size()
            {
                Width = 375,
                Height = 40,
            };

            elementMain2.Ops = new List<Op>();
            Op opmainSlide = new Op();
            Op opmainSlide2 = new Op();


            opmainSlide.Insert = "Autor: " + user?.FirstName + " " + user?.LastName;
            opmainSlide2.Insert = "\n";

            opmainSlide2.Attributes = new Attributes() { Align = "center", Bold = true };

            elementMain2.Ops.Add(opmainSlide);
            elementMain2.Ops.Add(opmainSlide2);

            mainSlide.Elements.Add(elementMain2);



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
                                Height = 350,
                            };

                            elementData.Type = "text";
                            elementData.Id = 2;
                            elementData.Ops = new List<Op>();
                            Op op2 = new Op();
                            op2.Insert = content;
                            op2.Attributes = new Attributes() { Size = "large" };
                            elementData.Ops.Add(op2);
                            newSlide.Elements.Add(elementTitle);
                            newSlide.Elements.Add(elementData);
                            presentationData.Slides.Add(newSlide);
                        }
                }
            }



            Slide endSlide = new Slide();
            endSlide.Id = slideID;
            endSlide.Title = "Slajd " + slideID;
            endSlide.Elements = new List<Element>();

            Element elementEnd = new Element();
            elementEnd.Id = 1;
            elementEnd.Type = "text";
            elementEnd.Position = new Position()
            {
                Top = 210,
                Left = 337,
            };
            elementEnd.Size = new Size()
            {
                Width = 325,
                Height = 91,
            };

            elementEnd.Ops = new List<Op>();
            Op opEnd = new Op();
            Op opEnd2 = new Op();


            opEnd.Insert = "Dziękuję za uwagę!";
            opEnd2.Insert = "\n";

            opEnd2.Attributes = new Attributes() { Align = "center", Header = 1, Bold = true };

            elementEnd.Ops.Add(opEnd);
            elementEnd.Ops.Add(opEnd2);

            endSlide.Elements.Add(elementEnd);


            Element elementEnd2 = new Element();
            elementEnd2.Id = 2;
            elementEnd2.Type = "text";
            elementEnd2.Position = new Position()
            {
                Top = 480,
                Left = 600,
            };
            elementEnd2.Size = new Size()
            {
                Width = 375,
                Height = 40,
            };

            elementEnd2.Ops = new List<Op>();
            Op opEndAuthoEnd = new Op();
            Op opEnd2AuthoEnd = new Op();


            opEndAuthoEnd.Insert = "Autor: "+user?.FirstName+ " "+user?.LastName;
            opEnd2AuthoEnd.Insert = "\n";

            opEnd2AuthoEnd.Attributes = new Attributes() { Align = "center", Bold = true };

            elementEnd2.Ops.Add(opEndAuthoEnd);
            elementEnd2.Ops.Add(opEnd2AuthoEnd);

            endSlide.Elements.Add(elementEnd2);

            presentationData.Slides.Add(endSlide);

            return presentationData;
         
        }
        public async Task<Quiz> GenerateQuizDataText(string descriptionQuiz, string titleMain,  Quiz quiz, int countElements)
        {
            string apiUrl = "https://api.openai.com/v1/chat/completions";
            Debug.WriteLine(descriptionQuiz);
            if (countElements < 1 || countElements > 15)
            {
                countElements = 10;
            }

            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                new { role = "user", content = $"Utwórz pytania do quizu na podany temat, temat to: {descriptionQuiz}. Maksymalna liczba pytań to: {countElements}. Maksymalnie po jednym zdaniu na pytanie i odpowiedzi. pytanie od odpowiedzi oddziel $$$, poszczególne pytania ### a poszczególne odpowiedzi oddziel &&&. Jeśli jest poprawna dodaj odp+, nic poza tym. Przykład schematu:  Ile to 2+2? $$$ 3 &&&odp+ 4  &&& 5 ###. niczego nie numeruj" }
            },
                max_tokens = 300,
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

            string[] questions = input.Split(new string[] { "###" }, StringSplitOptions.RemoveEmptyEntries);
            int number = 1;
            foreach (string q in questions)
            {
                string[] parts = q.Split(new string[] { "$$$" }, StringSplitOptions.RemoveEmptyEntries);
                Question newQuestion = new Question();

                if (parts.Length == 2)
                {

                    string question = parts[0].Trim();
                    string answers = parts[1].Trim();
                    Console.WriteLine($"Pytanie: {question}");
                    Console.WriteLine($"odp: {answers}");

                    string[] options = answers.Split(new string[] { "&&&" }, StringSplitOptions.RemoveEmptyEntries);
                    newQuestion.Name = question;
                    newQuestion.Answers = new List<Answer>();
               
                    for (int i = 0; i < options.Length; i++)
                    {
                        string option = options[i].Trim();
                        bool isCorrect = option.Contains("odp+");
                        Console.WriteLine($"({i + 1}) {option.Trim()} {(isCorrect ? "[Poprawna]" : "")}");
                        option = option.Replace("odp+", "");

                        if(option != "")
                        {
                            Answer newAnswer = new Answer();
                            newAnswer.Name = option.Trim();
                            newAnswer.IsCorrect = isCorrect;
                            newQuestion.Answers.Add(newAnswer);
                        }

                    }
                    if(newQuestion.Answers.Count > 0)
                    {
                        quiz.Questions.Add(newQuestion);
                        quiz.CountQuestions = number;
                        number++;
                    }
                }
            }
            return quiz;
        }
        public async Task<Quiz> generateQuizDataImage(Quiz quiz, int UserID, int fileID, string titleMain)
        {
            var imageUrls = new List<string>();
            string apiUrl = "https://api.openai.com/v1/images/generations";

            var requestBody = new
            {
                model = "dall-e-2",
                prompt = "Zdjecie do quizu na temat: " + titleMain,
                n = 2,
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
            }
            int index = 1;
            int actualAddImage = 0;
            foreach (var question in quiz.Questions)
            {
                if (index % 2 == 0 && imageUrls.Count >= 1)
                {

                    if (actualAddImage >= imageUrls.Count)
                    {
                        actualAddImage = 0;
                    }
                    string filename = await _fileService.SaveGeneratedImageQuizAndFlashcard(UserID, "Quiz"+ fileID, imageUrls[actualAddImage]);
                    actualAddImage++;
                    question.FileName = filename;
                }
                index++;
            }
            return quiz;
        }
        public async Task<Flashcards> generateFlashcardsDataText(string descriptionFlashcards, string titleMain, Flashcards flashcards, int countElements)
        {
            string apiUrl = "https://api.openai.com/v1/chat/completions";
            Debug.WriteLine(descriptionFlashcards);
            if (countElements < 1 || countElements > 15)
            {
                countElements = 10;
            }
            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                new { role = "user", content = $"Utwórz fiszki na podany temat, temat to: {descriptionFlashcards}. Maksymalna liczba fiszek to: {countElements}. Hasło od Opisu oddziel &&& a poszczególne fiszki oddziel ###, nic poza tym. Maksymalnie po jednym zdaniu na hasło i opis. Przykład: Java &&& Język programowania ###" }
            },
                max_tokens = 200,
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

            string[] flashcard = input.Split(new string[] { "###" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string flash in flashcard)
            {
                string[] parts = flash.Split(new string[] { "&&&" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    Flashcard newFlashcard = new Flashcard();
                    string title = parts[0].Trim();
                    string description = parts[1].Trim();
                    if(title!="" && description != "")
                    {
                        newFlashcard.Title = title;
                        newFlashcard.Description = description;
                        flashcards.Flashcard.Add(newFlashcard);
                    }     
                }
            }
            return flashcards;
        }
        public async Task<Flashcards> generateFlashcardsDataImage(Flashcards flashcards, int UserID, int fileID, string titleMain)
        {
            var imageUrls = new List<string>();
            string apiUrl = "https://api.openai.com/v1/images/generations";

            var requestBody = new
            {
                model = "dall-e-2",
                prompt = "Zdjecie do fiszki na temat: " + titleMain,
                n = 2,
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
            }
            int index = 1;
            int actualAddImage = 0;
            foreach (var card in flashcards.Flashcard)
            {
                if (index % 2 == 0 && imageUrls.Count >= 1)
                {
                    if (actualAddImage >= imageUrls.Count)
                    {
                        actualAddImage = 0;
                    }
                    string filename = await _fileService.SaveGeneratedImageQuizAndFlashcard(UserID, "Flashcard" + fileID, imageUrls[actualAddImage]);
                    actualAddImage++;
                    card.FileName = filename;
                }
                index++;
            }
            return flashcards;
        }
        public string CapitalizeFirstLetter(string sentence)
        {
            try
            {
                if (string.IsNullOrEmpty(sentence))
                    return sentence;
                return sentence.Substring(0, 1).ToUpper() + sentence.Substring(1);
            }
            catch (Exception ex) {
                return sentence;
            }  
        }

    }
}
