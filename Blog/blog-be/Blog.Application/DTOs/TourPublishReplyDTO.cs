namespace Blog.Application.DTOs
{
    public class TourPublishReplyDTO
    {
        public long TourId { get; set; }
        public string? BlogId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? FailureReason { get; set; }
    }
}
