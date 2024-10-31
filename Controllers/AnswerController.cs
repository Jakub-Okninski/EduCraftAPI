using EduCraftAPI.Data;
using EduCraftAPI.Entities.Quiz;
using EduCraftAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCraftAPI.Controllers
{
    [Authorize]
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
            var existingAnswer = await _context.Answers.FindAsync(newAnswer.AnswerID);

            if (existingAnswer == null)
            {
                return NotFound("Answer not found.");
            }
            existingAnswer.Name = newAnswer.Name; 
            existingAnswer.IsCorrect = newAnswer.IsCorrect; 

            await _context.SaveChangesAsync();

            return Ok(existingAnswer);
        }

        [HttpPost("/question/edit")]
        public async Task<IActionResult> UpdateQuestion([FromBody] QuestionDTO questionDTO)
        {

            var question = _context.Questions.FirstOrDefault(q => q.QuestionID == questionDTO.QuestionID);
            if (question == null)
            {
                return NotFound("Question not found.");
            }
            question.Name = questionDTO.Name;
        
            await _context.SaveChangesAsync();

            return Ok(question);
        }

        [HttpDelete("answer/remove")]
        public async Task<IActionResult> RemoveAnswer([FromQuery]int answerId)
        {
            // Find the answer by ID
            var answer = await _context.Answers.FindAsync(answerId);

            if (answer == null)
            {
                return NotFound("Answer not found.");
            }

            // Remove the answer from the context
            _context.Answers.Remove(answer);
            await _context.SaveChangesAsync();

            // Return a 204 No Content response
            return Ok();
        }


        [HttpPost("/answer/create")]
        public async Task<IActionResult> AddAnswer([FromBody] AnswerDTO newAnswer)
        {
            if (newAnswer == null || string.IsNullOrWhiteSpace(newAnswer.Name))
            {
                return BadRequest("Invalid answer data.");
            }
            var question = _context.Questions.FirstOrDefault(q => q.QuestionID == newAnswer.QuestionID);

            if (question == null)
            {
                return NotFound("Question not found.");
            }
            Answer a = new Answer
            {
                Name = newAnswer.Name,
                IsCorrect = newAnswer.IsCorrect,
                QuestionID=newAnswer.QuestionID

            };
            await _context.Answers.AddAsync(a);
            await _context.SaveChangesAsync();

            return Ok(a);
        }
    }
}