namespace EduCraftAPI.Entities.Quiz
{
    public class Answer
    {
        public int AnswerID { get; set; }
        public string Name { get; set; }
        public bool IsCorrect {  get; set; }
        public int QuestionID { get; set; }

    }
}
