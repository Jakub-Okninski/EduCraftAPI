namespace EduCraftAPI.Models
{
    public class CountQuestionDTO
    {
        public int QuizID { get; set; }
        public int CountQuestions { get; set; }
    }
    public class RandomDTO
    {
        public int QuizID { get; set; }
        public Boolean RandomQuestion { get; set; } 
    }
}
