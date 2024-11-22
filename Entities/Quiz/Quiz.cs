namespace EduCraftAPI.Entities.Quiz
{
    using EduCraftAPI.Entities.User;
    using EduCraftAPI.Entities.Category;

    public class Quiz
    {
        public int QuizID { get; set; }
        public string Name { get; set; }
        public DateTime CreationDate { get; set; }
        public User User { get; set; }
        public int UserID { get; set; }
        public Boolean IsPublic { get; set; }
        public int CategoryID { get; set; }
        public Category Category { get; set; }
        public ICollection<Question> Questions { get; set; } 
    }
}
