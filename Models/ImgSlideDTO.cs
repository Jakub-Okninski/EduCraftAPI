namespace EduCraftAPI.Models
{
    public class ImgSlideDTO
    {
        public int UserID { get; set; }
        public int PresentationID { get; set; }
        public int SlideID { get; set; }
        public Position Position { get; set; }
        public IFormFile Image { get; set; }
    }
}
