namespace EduCraftAPI.Models
{
    public class CheckAnswer
    { 
        public int AnswerID { get; set; }
    }
    public class CheckQuizDTO
    {
        public int QuizID { get; set; }
        public IEnumerable<CheckAnswer> Answers { get; set; }
    }
}
