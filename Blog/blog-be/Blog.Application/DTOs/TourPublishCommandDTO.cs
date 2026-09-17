namespace Blog.Application.DTOs
{
    public class TourPublishCommandDTO
    {
        public long TourId { get; set; }
        public string TourName { get; set; } = string.Empty;
        public string TourDescription { get; set; } = string.Empty;
        public string AuthorUsername { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}
