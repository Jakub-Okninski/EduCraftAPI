using System.ComponentModel.DataAnnotations;

namespace EduCraftAPI.Models
{
    public class RegisterUserDto
    {
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
