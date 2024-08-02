using Microsoft.AspNetCore.Identity;

namespace EduCraftAPI.Entities.User
{
    public class User
    {
        public int UserID { get; set; }
        public string FirstName { get; set; }   
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int RoleID { get; set; }
        public Role Role { get; set; }



    }
}
