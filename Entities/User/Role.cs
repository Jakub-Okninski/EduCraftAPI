using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EduCraftAPI.Entities.User
{
    public class Role 
    {
        public int RoleID { get; set; }
        public string Name { get; set; }

    }
}
