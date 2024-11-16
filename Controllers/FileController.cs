using EduCraftAPI.Data;
using EduCraftAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduCraftAPI.Controllers
{
    public class FileController : Controller
    {

        private readonly DataDbContext _context;
        private readonly IFileService _fileService;

        public FileController(DataDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }


    }

}
