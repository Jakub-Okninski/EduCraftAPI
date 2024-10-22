namespace EduCraftAPI.Models
{
    public class SearchDto
    {
        public string? Phrase { get; set; }
        public DateTime? StartDate { get; set; } 
        public DateTime? EndDate { get; set; }   
        public int? Category { get; set; }
    }
}
