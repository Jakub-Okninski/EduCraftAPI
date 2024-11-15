namespace EduCraftAPI.Models
{
    public class CreateFlashcardsDto
    {
        public int UserId { get; set; }
        public string Title { get; set; }
        public List<CreateFlashcardDto>? Flashcards { get; set; }
    }

    public class CreateFlashcardDto
    {
        public IFormFile? File { get; set; }
        public int FlashcardsId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
