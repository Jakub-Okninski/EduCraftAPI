namespace EduCraftAPI.Models
{
    public class SearchDto
    {
        public int ItemID { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CategoryName { get; set; }
        public DateTime CreationDate { get; set; }
        public string Type { get; set; }
    }
}
