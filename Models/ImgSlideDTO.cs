namespace EduCraftAPI.Models
{
    public class ImgSlideDTO
    {
        public int UserID { get; set; }
        public int PresentationID { get; set; }
        public int SlideID { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public IFormFile Image { get; set; }
    }
}
