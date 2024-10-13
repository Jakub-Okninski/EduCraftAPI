namespace EduCraftAPI.Entities.Presentation
{
using EduCraftAPI.Entities.User;
public class Presentations
    {
        public int PresentationsID { get; set; }
        public string Title { get; set; }
        public User User { get; set; }
        public int UserID { get; set; }
    }
}
