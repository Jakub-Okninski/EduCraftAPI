using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Office.SpreadSheetML.Y2023.MsForms;
using EduCraftAPI.Data;
using EduCraftAPI.Entities;
using EduCraftAPI.Entities.Flashcards;
using EduCraftAPI.Entities.Quiz;
using EduCraftAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Question = EduCraftAPI.Entities.Quiz.Question;

namespace EduCraftAPI.Controllers
{
    [Authorize]
    public class QuizController : Controller
    {
        private readonly DataDbContext _context;
        public QuizController(DataDbContext context) {
            _context = context;
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


        [HttpGet("/quiz")]
        public IActionResult quiz([FromQuery] int QuizID)
        {
            var quiz = _context.Quizzes
              .Include(p => p.Questions).ThenInclude(q => q.Answers)
                .FirstOrDefault(p => p.QuizID == QuizID);
                
            if (quiz == null)
            {
                return NoContent();
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
        [HttpPost("/question/crerate")]
        public IActionResult questionGenerate([FromBody] newQuestionDTO questionNew)
        {
            if (questionNew == null)
            {
                return BadRequest("Nieprawidłowe dane.");
            }

            var quiz = _context.Quizzes.FirstOrDefault(u => u.QuizID == questionNew.QuizID);
            if (quiz == null)
            {
                return NoContent();
            }

            try
            {

                var answerList = new List<Answer>();
                foreach (var a in questionNew.Answers)
                {
                    answerList.Add(new Answer { Name = a.Name, IsCorrect = a.IsCorrect });
                }
                var newQuestion = new Question
                {
                    QuizID= quiz.QuizID,
                    Name = questionNew.Name,
                    Answers = answerList
                };

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
