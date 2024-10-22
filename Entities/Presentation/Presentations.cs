namespace EduCraftAPI.Entities.Presentation
{
using EduCraftAPI.Entities.User;
using EduCraftAPI.Entities.Category;
    public class Presentations
    {
        public int PresentationsID { get; set; }
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }
        public Boolean IsPublic { get; set; }
        public int CategoryID { get; set; }
        public Category Category { get; set; }
        public User User { get; set; }
        public int UserID { get; set; }
    }
}
