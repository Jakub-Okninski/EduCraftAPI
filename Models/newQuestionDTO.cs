namespace EduCraftAPI.Models
{
    public class newAnswerDTO{
        public string Name { get; set; }
        public bool IsCorrect { get; set; }

    }
    public class newQuestionDTO
    {
        public int QuizID { get; set; }
        public string Name { get; set; }
        public IFormFile? File { get; set; }
        public IEnumerable<newAnswerDTO> Answers { get; set; }
    }
}
