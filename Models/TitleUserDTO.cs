namespace EduCraftAPI.Models
{
    public class TitleUserDTO
    {
        public int UserId { get; set; }
        public string Title { get; set; }
        public int? CategoryID { get; set; }
        public bool IsPublic { get; set; }
        public string Description { get; set; }
        public int CountElements { get; set; } = 5; 

    }
}
