using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EduCraftAPI.Entities.User
{
    public class User
    {
        public int UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
        public int RoleID { get; set; }
        public Role Role { get; set; }



    }
}
