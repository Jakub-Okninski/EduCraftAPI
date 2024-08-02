using EduCraftAPI.Data;
using EduCraftAPI.Entities;
using EduCraftAPI.Entities.Quiz;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace EduCraftAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class QuizController : Controller
    {
        private readonly DataDbContext _context;
        public QuizController(DataDbContext context) {
            _context = context;
        }
        [HttpGet]
        public ActionResult<IEnumerable<Quiz>> Quizzes()
        {
            var quizzes = _context.Quizzes.ToList();
            return Ok(quizzes);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Quiz>> Quiz(int id)
        {
            var quiz = await _context.Quizzes
                          .Include(q => q.Questions) 
                              .ThenInclude(q => q.Answers) 
                          .FirstOrDefaultAsync(q => q.QuizID == id);
            if (quiz is null)
                return NotFound("Quiz not found");
            return Ok(quiz);
        }



        [HttpPost]
        public async Task<ActionResult<IEnumerable<Quiz>>> Quiz([FromBody] Quiz quiz)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _context.Quizzes.AddAsync(quiz);
            await _context.SaveChangesAsync();


            return Ok(await _context.Quizzes.ToListAsync());
        }
    }
}
