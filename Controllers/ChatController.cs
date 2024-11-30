using EduCraftAPI.Models;
using EduCraftAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;

namespace EduCraftAPI.Controllers
{
    [Authorize(Policy = "IsBlock")]
    public class ChatController : Controller
    {

        private readonly IGenerateService _generateService;

        public ChatController(IGenerateService generateService)
        {
            _generateService = generateService;
        }



        [HttpPost("/chat/talk")]
        public async Task<IActionResult> chatTalk([FromBody] Massage massage)
        {


            if (massage.data.IsNullOrEmpty()) {
                return NoContent();
            }
            try
            {
                var answer = await _generateService.generateAnswer(massage.data);
                return Ok(answer);
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message);
                return StatusCode(500, "Wewnętrzny błąd serwera." + ex);
            }
        }       
    }
}
