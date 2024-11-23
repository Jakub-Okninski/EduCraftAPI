using DocumentFormat.OpenXml.Office.SpreadSheetML.Y2023.MsForms;
using EduCraftAPI.Data;
using EduCraftAPI.Entities.Quiz;
using EduCraftAPI.Models;
using EduCraftAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.XSSF.Streaming.Values;
using System.Diagnostics;
using Question = EduCraftAPI.Entities.Quiz.Question;

namespace EduCraftAPI.Controllers
{
    [Authorize]
    public class QuizController : Controller
    {
        private readonly DataDbContext _context;
        private readonly IUserContextService _userContextService;
        private readonly IFileService _fileServices;
        private readonly IDocumentService _documentService;

        public QuizController(DataDbContext context, IUserContextService userContextService, IFileService fileServices, IDocumentService documentService)
        {
            _context = context;
            _userContextService = userContextService;
            _fileServices = fileServices;
            _documentService = documentService;
        }

        [HttpGet("/quizs/info")]
        public IActionResult GetQuizs()
        {
            var quiz = _context.Quizzes.Where(p => p.UserID == _userContextService.GetUserID);
            if (quiz == null)
            {
                return NoContent();
            }
            return Ok(quiz);
        }

        [HttpGet("/quiz/info")]
        public IActionResult GetQuiz([FromQuery] int QuizID)
        {
            var quiz = _context.Quizzes
                .FirstOrDefault(p => p.UserID == _userContextService.GetUserID && p.QuizID == QuizID);
            if (quiz == null)
            {
                return NoContent();
            }
            return Ok(quiz);
        }


        [AllowAnonymous]
        [HttpGet("/quiz/generate")]
        public IActionResult GenerateQuizOnID([FromQuery] int QuizID, [FromQuery] bool withCorrect = false)
        {
            var quiz = _context.Quizzes
                .Include(p => p.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefault(p => p.QuizID == QuizID);

            if (quiz == null)
            {
                return NoContent();
            }

            if (!withCorrect)
            {
                if (quiz.RandomQuestion)
                {
                    Random random = new Random();
                    foreach (var question in quiz.Questions)
                    {
                        question.Answers = question.Answers.OrderBy(q => random.Next()).ToList();
                    }
                    quiz.Questions = quiz.Questions.OrderBy(q => random.Next()).ToList();
                }

                if (quiz.Questions.Count >= quiz.CountQuestions)
                {
                    // Remove excess questions
                    quiz.Questions = quiz.Questions.Take(quiz.CountQuestions).ToList();
                }
            }
            

            var stream = _documentService.GenerateQuiz(quiz, withCorrect);
            return File(stream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "quiz_" + quiz.Name + ".docx");

        }


        [HttpDelete("/quiz/remove/image")]
        public IActionResult removeImageQuiz([FromQuery] int QuestionID)
        {
            var question = _context.Questions
                .FirstOrDefault(p => p.QuestionID == QuestionID);
            if (question == null)
            {
                return NoContent();
            }
            try
            {
                if(question.FileName!=null)
                {
                    _fileServices.RemoveImageQuiz((int)_userContextService.GetUserID, question.QuizID, question.FileName);
                    question.FileName = null;
                }
              

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wewnętrzny błąd serwera." + ex);
            }
            _context.SaveChanges();
            return Ok(question);
        }

        [HttpPut("/quiz/edit/image")]
        public IActionResult eduitImageQuiz([FromForm] FileDTO fileDTO)
        {
            var question = _context.Questions
                .FirstOrDefault(p => p.QuestionID == fileDTO.ID);
            if (question == null)
            {
                return NoContent();
            }

            if (question.FileName != null)
            {
                try
                {
                    _fileServices.RemoveImageQuiz((int)_userContextService.GetUserID, question.QuizID, question.FileName);
                    question.FileName = null;
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Wewnętrzny błąd serwera." + ex);
                }
            }
            if (fileDTO.File != null && fileDTO.File.Length != 0)
            {
                try
                {
                    question.FileName=_fileServices.SaveFileImgQuiz((int)_userContextService.GetUserID, question.QuizID, fileDTO.File);
                    question.FileContent = _fileServices.getBase64(fileDTO.File);

                }
                catch (Exception ex)
                {
                    if (question.FileName != null)
                    {
                        _fileServices.RemoveImageQuiz((int)_userContextService.GetUserID, question.QuizID, question.FileName);
                    }
                    _context.SaveChanges();
                    return StatusCode(500, "Wewnętrzny błąd serwera." + ex);
                }
            }
                _context.SaveChanges();
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
           
            return Ok(_fileServices.AddQuestionImg(quiz));
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
           .Select(p => new Quiz
           {
               QuizID = p.QuizID,
               Name = p.Name,
               UserID = p.UserID,
               RandomQuestion = p.RandomQuestion,
               CountQuestions = p.CountQuestions,
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
            if (quiz.RandomQuestion)
            {
                Random random = new Random();
                foreach (var question in quiz.Questions)
                {
                    question.Answers = question.Answers.OrderBy(q => random.Next()).ToList();
                }
                quiz.Questions = quiz.Questions.OrderBy(q => random.Next()).ToList();
            }

            if (quiz.Questions.Count >= quiz.CountQuestions)
            {
                // Remove excess questions
                quiz.Questions = quiz.Questions.Take(quiz.CountQuestions).ToList();
            }

            return Ok(_fileServices.AddQuestionImg(quiz));
        }


        [HttpDelete("/quiz/delete")]
        public async Task<IActionResult> RemoveQuiz([FromQuery] int id)
        {
            var quiz = _context.Quizzes
                .Include(q=>q.Questions)
                .ThenInclude(p=>p.Answers)
                .FirstOrDefault(p => p.QuizID == id);

            if (quiz == null)
            {
                return NotFound();
            }
            try
            {
                _fileServices.RemoveImgDirectory(quiz.UserID, "Quiz", quiz.QuizID);
                foreach (var question in quiz.Questions)
                {
                    _context.Answers.RemoveRange(question.Answers);
                }
                _context.Questions.RemoveRange(quiz.Questions);
                _context.Quizzes.Remove(quiz);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wewnętrzny błąd serwera." + ex);
            }
            _context.SaveChanges();
            return Ok();
        }


        [HttpPost("/quiz/update/isPublic")]
        public IActionResult updatePresentation([FromBody] IsPublicDTO isPublicDTO)
        {
            var quiz = _context.Quizzes.FirstOrDefault(u => u.QuizID == isPublicDTO.ItemID);
            if (quiz == null)
            {
                return NoContent();
            }
            try
            {
                quiz.IsPublic = isPublicDTO.IsPublic;
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wewnętrzny błąd serwera.");
            }
            return Ok(quiz);
        }
        [HttpPost("/quiz/update/random")]
        public IActionResult updateRandom([FromBody] RandomDTO randomDTO)
        {
            var quiz = _context.Quizzes.FirstOrDefault(u => u.QuizID == randomDTO.QuizID);
            if (quiz == null)
            {
                return NoContent();
            }
            try
            {
                quiz.RandomQuestion = randomDTO.RandomQuestion;
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wewnętrzny błąd serwera.");
            }
            return Ok(quiz);
        }

        [HttpPost("/quiz/update/countQuestion")]
        public IActionResult updateCountQuestion([FromBody] CountQuestionDTO countQuestionDTO)
        {
            var quiz = _context.Quizzes.Include(q=>q.Questions).FirstOrDefault(u => u.QuizID == countQuestionDTO.QuizID);
            if (quiz == null)
            {
                return NoContent();
            }
            if (countQuestionDTO.CountQuestions < 0)
            {
                return StatusCode(304, "Błedna liczba pytań.");
            }
            try
            {
                if (quiz.Questions.Count >= countQuestionDTO.CountQuestions)
                {
                    quiz.CountQuestions = countQuestionDTO.CountQuestions;
                    _context.SaveChanges();
                }
                else
                {
                    return StatusCode(304, "Błedna liczba pytań.");
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wewnętrzny błąd serwera.");
            }
            return Ok(countQuestionDTO.CountQuestions);
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

            var quiz = _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefault(q => q.QuizID == question.QuizID);

            if (quiz == null)
            {
                return NotFound("Answer not found.");
            }
            try
            {
                if (question.FileName != null)
                {
                    _fileServices.RemoveImageQuiz((int)_userContextService.GetUserID, question.QuizID, question.FileName);
                }
            }
            catch (Exception ex) {
                return StatusCode(500, "Wewnętrzny błąd serwera." + ex);
            }
            Debug.WriteLine("...");
            Debug.WriteLine(quiz.CountQuestions); 
            Debug.WriteLine(quiz.Questions.Count);

            Debug.WriteLine("...");

            if (quiz.CountQuestions > quiz.Questions.Count-1)
            {
                quiz.CountQuestions=quiz.Questions.Count-1;
            }
            _context.Questions.Remove(question);
            _context.SaveChanges();
            return Ok();
        }

        [HttpPost("/question/crerate")]
        public IActionResult questionGenerate([FromForm] newQuestionDTO questionNew)
        {
            if (questionNew == null)
            {
                return BadRequest("Nieprawidłowe dane.");
            }

            var quiz = _context.Quizzes
                .Include(u=>u.User)
                .Include(u => u.Questions)
                .FirstOrDefault(u => u.QuizID == questionNew.QuizID);
            if (quiz == null)
            {
                return NoContent();
            }

            Question newQuestion = new Question();
            if (questionNew.File != null && questionNew.File.Length != 0)
            {
                try
                {
                    newQuestion.FileName = _fileServices.SaveFileImgQuiz((int)_userContextService.GetUserID, quiz.QuizID, questionNew.File);
                    newQuestion.FileContent = _fileServices.getBase64(questionNew.File);

                }
                catch (Exception ex)
                {
                    if (newQuestion.FileName != null)
                    {
                        _fileServices.RemoveImageQuiz((int)_userContextService.GetUserID, quiz.QuizID, newQuestion.FileName);
                    }
                    return StatusCode(500, "Wewnętrzny błąd serwera." + ex);
                }
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

                Debug.WriteLine("...");
                Debug.WriteLine(quiz.CountQuestions);
                Debug.WriteLine(quiz.Questions.Count);

                Debug.WriteLine("...");

                if (quiz.CountQuestions <= quiz.Questions.Count)
                {
                    quiz.CountQuestions = quiz.Questions.Count + 1;
                }


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
            var catrgory = _context.Category.FirstOrDefault(u => u.CategoryID == quizRequest.CategoryID);
            if (catrgory == null)
            {
                return NoContent();
            }

            try
            {
                var quiz = new Quiz
                {
                    UserID = (int)_userContextService.GetUserID,
                    Name = quizRequest.Title,
                    IsPublic = quizRequest.IsPublic,
                    CategoryID = catrgory.CategoryID,
                    CreationDate = DateTime.Now,
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
                    },
                    CountQuestions = 1
                
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
