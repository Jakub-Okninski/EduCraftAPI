namespace EduCraftAPI.Models
{
    public class ImageResponse
    {
        public List<ImageData> Data { get; set; }
    }

    public class ImageData
    {
        public string Url { get; set; }
    }

}
