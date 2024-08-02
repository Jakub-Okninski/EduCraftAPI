namespace EduCraftAPI.Entities.Quiz
{ 
    public class Question
    {
        public int QuestionID { get; set; }
        public string Name {  get; set; }
        public Quiz Quiz { get; set; }
        public int QuizID { get; set; }
        public ICollection<Answer> Answers { get; set; }


    }
}
