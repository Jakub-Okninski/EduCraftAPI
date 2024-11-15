using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EduCraftAPI.Entities.Flashcards
{
    public class Flashcard
    {
        public int FlashcardID { get; set; }
        public string Title { get; set; }
        public string? FileName { get; set; }
        [NotMapped]
        public string? FileContent { get; set; }
        public string Description { get; set; }
        public int FlashcardsID { get; set; }
        [JsonIgnore]
        public Flashcards Flashcards { get; set; }

    }
}
