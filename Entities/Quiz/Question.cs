using System.ComponentModel.DataAnnotations.Schema;

namespace EduCraftAPI.Entities.Quiz
{ 
    public class Question
    {
        public int QuestionID { get; set; }
        public string Name {  get; set; }
        public string? FileName { get; set; }
        [NotMapped]
        public string? FileContent { get; set; }
        public ICollection<Answer> Answers { get; set; }
        public int QuizID { get; set; }
    }
}
