namespace EduCraftAPI.Entities.Quiz

{
    using EduCraftAPI.Entities.User;

    public class Quiz
    {
        public int QuizID { get; set; }
        public string Name { get; set; }
        public User User { get; set; }
        public int UserID { get; set; }
        public ICollection<Question> Questions { get; set; } 
    }
}
