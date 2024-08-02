namespace EduCraftAPI.Entities.Quiz
{
    public class Quiz
    {
        public int QuizID { get; set; }
        public string Name { get; set; }
        public ICollection<Question> Questions { get; set; }

    }
}
