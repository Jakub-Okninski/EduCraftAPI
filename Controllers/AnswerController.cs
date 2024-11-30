using EduCraftAPI.Data;
using EduCraftAPI.Entities.Quiz;
using EduCraftAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCraftAPI.Controllers
{
    [Authorize(Policy = "IsBlock")]
    public class AnswerController : Controller
    {
        private readonly DataDbContext _context;
        public AnswerController(DataDbContext context)
        {
            _context = context;
        }



        [HttpPut("/answer/edit")]
        public async Task<IActionResult> UpdateAnswer([FromBody] Answer newAnswer)
        {
            var existingAnswer = _context.Answers.FirstOrDefault(a=>a.AnswerID == newAnswer.AnswerID);

            if (existingAnswer == null)
            {
                return NotFound();
            }
            existingAnswer.Name = newAnswer.Name; 
            existingAnswer.IsCorrect = newAnswer.IsCorrect; 

             _context.SaveChanges();

            return Ok(existingAnswer);
        }

        [HttpPost("/question/edit")]
        public async Task<IActionResult> UpdateQuestion([FromBody] QuestionDTO questionDTO)
        {
            var question = _context.Questions.FirstOrDefault(q => q.QuestionID == questionDTO.QuestionID);
            if (question == null)
            {
                return NotFound();
            }
            question.Name = questionDTO.Name;
        
            _context.SaveChanges();

            return Ok(question);
        }

        [HttpDelete("answer/remove")]
        public async Task<IActionResult> RemoveAnswer([FromQuery]int answerId)
        {
            var answer = _context.Answers.FirstOrDefault(a => a.AnswerID == answerId);

            if (answer == null)
            {
                return NotFound();
            }
            _context.Answers.Remove(answer);
            _context.SaveChanges();
            return Ok();
        }


        [HttpPost("/answer/create")]
        public async Task<IActionResult> AddAnswer([FromBody] AnswerDTO newAnswer)
        {
            if (newAnswer == null || string.IsNullOrWhiteSpace(newAnswer.Name))
            {
                return BadRequest();
            }
            var question = _context.Questions.FirstOrDefault(q => q.QuestionID == newAnswer.QuestionID);

            if (question == null)
            {
                return NotFound("Question not found.");
            }
            Answer answer = new Answer
            {
                Name = newAnswer.Name,
                IsCorrect = newAnswer.IsCorrect,
                QuestionID=newAnswer.QuestionID

            };
            _context.Answers.Add(answer);
            _context.SaveChanges();

            return Ok(answer);
        }
    }
}