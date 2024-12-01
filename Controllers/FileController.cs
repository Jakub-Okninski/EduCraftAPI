using EduCraftAPI.Data;
using EduCraftAPI.Entities.Quiz;
using EduCraftAPI.Models;
using EduCraftAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace EduCraftAPI.Controllers
{
    [Authorize(Policy = "IsBlock")]
    public class FileController : Controller
    {
        private readonly IUserContextService _userContextService;
        private readonly DataDbContext _context;
        private readonly IFileService _fileService;

        public FileController(DataDbContext context, IFileService fileService, IUserContextService userContextService)
        {
            _context = context;
            _fileService = fileService;
            _userContextService = userContextService;   
        }
        [HttpGet("/file/list")]
        public IActionResult GetList()
        {
            var documents = _context.Flashcards
              .Where(f => f.UserID == _userContextService.GetUserID)
              .Select(f => new DocDTO()
              {
                  ID = f.FlashcardsID,
                  Name = f.Title,
                  Type = "Flashcards"
              })
              .Union(
                  _context.Quizzes
                      .Where(q => q.UserID == _userContextService.GetUserID)
                      .Select(q => new DocDTO()
                      {
                          ID = q.QuizID,
                          Name = q.Name,
                          Type="Quiz"
                      })
              )
              .Union(
                  _context.Presentation
                      .Where(p => p.UserID == _userContextService.GetUserID)
                      .Select(p => new DocDTO()
                      {
                          ID = p.PresentationsID,
                          Name = p.Title,
                          Type = "Presentation"
                      })
              )
            .ToList();

            if (documents == null)
            {
                return NoContent();
            }
            return Ok(documents);
        }

        [HttpGet("/file/item")]
        public IActionResult GetItem([FromQuery] string Type, int ID)
        {

            if(Type == "generated" || ID < 0)
            {
                var item = _fileService.getAllFIle((int)_userContextService.GetUserID, "Generated");
                if (item != null)
                {
                    return Ok(item);
                }
            }
            else
            {
                var item = _fileService.getAllFIle((int)_userContextService.GetUserID, Type+ID);
                if (item != null)
                {
                    return Ok(item);
                }
            }
          
            return NoContent();       
        }

    }

}
