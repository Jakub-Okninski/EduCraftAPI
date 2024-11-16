using DocumentFormat.OpenXml.Wordprocessing;
using EduCraftAPI.Data;
using EduCraftAPI.Entities.Quiz;
using EduCraftAPI.Migrations;
using EduCraftAPI.Models;
using EduCraftAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Question = EduCraftAPI.Entities.Quiz.Question;

namespace EduCraftAPI.Controllers
{
    [Authorize]
    public class QuizController : Controller
    {
        private readonly DataDbContext _context;
        private readonly IUserContextService _userContextService;

        public QuizController(DataDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        [HttpGet("/MyQuiz")]
        public IActionResult GetQuiz([FromQuery] int UserID, int QuizID)
        {
            var quiz = _context.Quizzes
                .FirstOrDefault(p => p.UserID == UserID && p.QuizID == QuizID);
            if (quiz == null)
            {
                return NoContent();
            }
            return Ok(quiz);
        }


        [HttpDelete("/remove/quiz/image")]
        public IActionResult removeImageQuiz([FromQuery] int QuestionID)
        {
            var question = _context.Questions
                .FirstOrDefault(p => p.QuestionID == QuestionID);
            if (question == null)
            {
                return NoContent();
            }
            var quiz = _context.Quizzes.FirstOrDefault(q => q.QuizID == question.QuizID);
         // Skonstruowanie pełnej ścieżki pliku
        var filePath = Path.Combine("UserDataImage", "User" + quiz.UserID, "Quiz" + quiz.QuizID, question.FileName);

            // Sprawdzanie, czy plik istnieje
            if (System.IO.File.Exists(filePath))
            {
                // Usuwanie pliku
                System.IO.File.Delete(filePath);
                question.FileName = null;
                _context.SaveChanges();
                // Zwracamy odpowiedź o sukcesie
                return Ok(new { message = "Plik został usunięty." });
            }
            else
            {
                return NoContent();
            }

           
            return Ok(question);
        }
        [HttpPut("/edit/quiz/image")]
        public IActionResult eduitImageQuiz([FromForm] FileDTO fileDTO)
        {
            var question = _context.Questions
                .FirstOrDefault(p => p.QuestionID == fileDTO.ID);
            if (question == null)
            {
                return NoContent();
            }
            var quiz = _context.Quizzes.FirstOrDefault(q => q.QuizID == question.QuizID);
            // Skonstruowanie pełnej ścieżki pliku
            if (question.FileName != null)
            {
                var filePath = Path.Combine("UserDataImage", "User" + quiz.UserID, "Quiz" + quiz.QuizID, question.FileName);

                // Sprawdzanie, czy plik istnieje
                if (System.IO.File.Exists(filePath))
                {
                    // Usuwanie pliku
                    System.IO.File.Delete(filePath);


                }
            }
         
        

            if (fileDTO.File != null && fileDTO.File.Length != 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "UserDataImage", "User" + quiz.UserID,"Quiz"+quiz.QuizID);
            



                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(fileDTO.File.FileName);
                var filePath2 = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath2, FileMode.Create))
                {
                    fileDTO.File.CopyTo(stream);

                }
                using (var memoryStream = new MemoryStream())
                {
                    // Kopiujemy plik do MemoryStream
                    fileDTO.File.CopyTo(memoryStream);

                    // Konwertujemy zawartość MemoryStream na tablicę bajtów
                    byte[] fileBytes = memoryStream.ToArray();

                    // Konwertujemy tablicę bajtów na Base64
                    string base64String = Convert.ToBase64String(fileBytes);

                    // Tworzymy pełny ciąg Base64 z odpowiednim prefiksem
                    question.FileContent = $"data:image/jpeg;base64,{base64String}";
                }



                question.FileName = fileName;
                _context.SaveChanges();
            }





            return Ok(question);
        }

        [HttpGet("/quiz")]
        public IActionResult quiz([FromQuery] int QuizID)
        {
            var quiz = _context.Quizzes
               .Include(q => q.Questions)
               .ThenInclude(q => q.Answers)
               .Where(q => q.QuizID == QuizID).FirstOrDefault();

            if (quiz == null)
            {
                return NoContent();
            }
            foreach (Question question in quiz.Questions)
            {
                if (!string.IsNullOrEmpty(question.FileName))
                {
                 
                    var filePath = Path.Combine("UserDataImage","User"+ _userContextService.GetUserID, "Quiz"+QuizID, question.FileName);

                
                    if (System.IO.File.Exists(filePath))
                    {
                        var fileBytes = System.IO.File.ReadAllBytes(filePath);
                        string base64String = Convert.ToBase64String(fileBytes);
                        question.FileContent = $"data:image/jpeg;base64,{base64String}";
                    }
                    else
                    {
                        question.FileContent = null; 
                    }
                }
             
            }
            return Ok(quiz);
        }


        [HttpGet("/MyQuizs")]
        public IActionResult GetQuizs([FromQuery] int UserID)
        {
            var quiz = _context.Quizzes.Where(p => p.UserID == UserID);
            if (quiz == null)
            {
                return NoContent();
            }
            return Ok(quiz);
        }
        [AllowAnonymous]
        [HttpPost("/check/quiz")]
        public IActionResult checkQuiz([FromBody] CheckQuizDTO checkQuizDTO)
        {

            var quiz = _context.Quizzes
                 .Include(q => q.Questions)
                 .ThenInclude(q => q.Answers)
                 .Where(q => q.QuizID == checkQuizDTO.QuizID).FirstOrDefault();
            if (quiz == null)
            {
                return NotFound();
            }

            var correctAnswerIds = quiz.Questions
                .SelectMany(q => q.Answers)
                .Where(a => a.IsCorrect)
                .Select(a => a.AnswerID)
                .ToList();

            var selectedAnswerIds = checkQuizDTO.Answers?.Select(a => a.AnswerID) ?? new List<int>();

            var correctCount = selectedAnswerIds.Intersect(correctAnswerIds).Count();

            var incorrectCount = selectedAnswerIds.Count(a => !correctAnswerIds.Contains(a));

            return Ok(new { correctCount, incorrectCount, totalCorrect = correctAnswerIds.Count, correctAnswer = correctAnswerIds });
        }

        [AllowAnonymous]
        [HttpGet("/Quiz/Look")]
        public async Task<IActionResult> QuizGet([FromQuery] int quizID)
        {

            var quiz = _context.Quizzes
           .Where(p => p.QuizID == quizID)
           .Select(p => new
           {
               p.QuizID,
               p.Name,
               p.UserID,
               Questions = p.Questions.Select(q => new Question
               {
                   QuestionID = q.QuestionID,
                   Name = q.Name,
                   FileName = q.FileName,
                   Answers = q.Answers.Select(a => new Answer
                   {
                       AnswerID = a.AnswerID,
                       Name = a.Name
                   }).ToList()
               }).ToList()
           })
           .FirstOrDefault();

            if (quiz == null)
            {
                return NoContent();
            }
            foreach (var question in quiz.Questions)
            {
                if (!string.IsNullOrEmpty(question.FileName))
                {

                    var filePath = Path.Combine("UserDataImage", "User" + quiz.UserID, "Quiz" + quiz.QuizID, question.FileName);

                  
                    if (System.IO.File.Exists(filePath))
                    {
                        var fileBytes = System.IO.File.ReadAllBytes(filePath);
                        string base64String = Convert.ToBase64String(fileBytes);
                        question.FileContent = $"data:image/jpeg;base64,{base64String}";
                       
                    }
                    else
                    {
                        question.FileContent = null;
                    }
                }

            }
            return Ok(quiz);
        }


        [HttpDelete("/question/remove")]
        public async Task<IActionResult> Removequestion([FromQuery] int questionID)
        {
            var question = await _context.Questions
                .Where(q => q.QuestionID == questionID)
                .FirstOrDefaultAsync();

            if (question == null)
            {
                return NotFound("Answer not found.");
            }
            var quiz = _context.Quizzes.FirstOrDefault(q => q.QuizID == question.QuizID);

            var filePath = Path.Combine("UserDataImage", "User" + quiz.UserID, "Quiz" + question.QuizID, question.FileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
       
            }


            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("/question/crerate")]
        public IActionResult questionGenerate([FromForm] newQuestionDTO questionNew)
        {
            if (questionNew == null)
            {
                return BadRequest("Nieprawidłowe dane.");
            }

            var quiz = _context.Quizzes.Include(u=>u.User).FirstOrDefault(u => u.QuizID == questionNew.QuizID);
            if (quiz == null)
            {
                return NoContent();
            }

         

            Question newQuestion = new Question();


            if (questionNew.File != null && questionNew.File.Length != 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "UserDataImage", "User" + _userContextService.GetUserID);
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                uploadsFolder = Path.Combine(uploadsFolder, "Quiz" + quiz.QuizID);
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }




                // Tworzymy pełny ciąg Base64 z prefiksem


                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(questionNew.File.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    questionNew.File.CopyTo(stream);

                }
                using (var memoryStream = new MemoryStream())
                {
                    // Kopiujemy plik do MemoryStream
                    questionNew.File.CopyTo(memoryStream);

                    // Konwertujemy zawartość MemoryStream na tablicę bajtów
                    byte[] fileBytes = memoryStream.ToArray();

                    // Konwertujemy tablicę bajtów na Base64
                    string base64String = Convert.ToBase64String(fileBytes);

                    // Tworzymy pełny ciąg Base64 z odpowiednim prefiksem
                    newQuestion.FileContent = $"data:image/jpeg;base64,{base64String}";
                }



                newQuestion.FileName = fileName;

            }




            try
            {
                var answerList = new List<Answer>();
                foreach (var a in questionNew.Answers)
                {
                    answerList.Add(new Answer { Name = a.Name, IsCorrect = a.IsCorrect });
                }


                newQuestion.QuizID = quiz.QuizID;
                newQuestion.Name = questionNew.Name;
                newQuestion.Answers = answerList;
               

                _context.Questions.Add(newQuestion);

                _context.SaveChanges();

                return Ok(newQuestion);
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Internal server error: {e.Message}");
            }
        }
        [HttpPost("/quiz/generate")]
        public IActionResult quizGenerate([FromBody] TitleUserDTO quizRequest)
        {
            if (quizRequest == null)
            {
                return BadRequest("Nieprawidłowe dane.");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserID == quizRequest.UserId);
            if (user == null)
            {
                return NoContent();
            }

            try
            {
                var quiz = new Quiz
                {
                    User = user,
                    Name = quizRequest.Title,
                    Questions = new List<Question>
                    {
                        new Question
                        {
                            Name = "What is the capital of France?",
                            Answers = new List<Answer>
                            {
                                new Answer { Name = "Paris", IsCorrect = true },
                                new Answer { Name = "Berlin", IsCorrect = false },
                                new Answer { Name = "Madrid", IsCorrect = false }
                            }
                        }
                    }
                };

                _context.Quizzes.Add(quiz);
                _context.SaveChanges();
                return Ok(quiz);
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Internal server error: {e.Message}");
            }
        }
    }
}
