using EduCraftAPI.Entities.Quiz;
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
        private readonly IUserContextService _userContextService;

        public ChatController(IGenerateService generateService, IUserContextService userContextService)
        {
            _generateService = generateService;
            _userContextService = userContextService;
        }



        [HttpPost("/chat/talk")]
        public async Task<IActionResult> chatTalk([FromBody] Massage massage)
        {


            if (massage.data.IsNullOrEmpty()) {
                return NoContent();
            }
            if (massage.isPicture)
            {
                try
                {
                    var answer = await _generateService.GeneratePicture(massage.data, (int)_userContextService.GetUserID);
                    if (answer == null || answer.Count == 0)
                    {
                        return NoContent();
                    }
                    Debug.WriteLine(answer);

                    return Ok(new { type = true, value = answer });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return StatusCode(500, "Wewnętrzny błąd serwera." + ex);
                }
            }
            else
            {
                try
                {
                    var answer = await _generateService.GenerateAnswer(massage.data);
                    return Ok(new { type = false, value = answer });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return StatusCode(500, "Wewnętrzny błąd serwera." + ex);
                }
            }         
        }       
    }
}
