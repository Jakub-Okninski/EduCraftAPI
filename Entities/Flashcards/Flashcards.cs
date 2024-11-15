namespace EduCraftAPI.Entities.Flashcards
{
    using EduCraftAPI.Entities.User;
    public class Flashcards
    {
        public int FlashcardsID { get; set; }
        public string Title { get; set; }
        public User User { get; set; }
        public int UserID { get; set; }
        public ICollection<Flashcard> Flashcard { get; set; } = new List<Flashcard>();

    }
}
