namespace EduCraftAPI.Entities.Flashcards
{
    using EduCraftAPI.Entities.User;
    using EduCraftAPI.Entities.Category;

    public class Flashcards
    {
        public int FlashcardsID { get; set; }
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }
        public User User { get; set; }
        public int UserID { get; set; }
        public Boolean IsPublic { get; set; }
        public int CategoryID { get; set; }
        public Category Category { get; set; }
        public ICollection<Flashcard> Flashcard { get; set; } = new List<Flashcard>();

    }
}
