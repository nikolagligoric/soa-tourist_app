namespace Purchase.Application.DTOs;

public class TourSlotReplyDto
{
    public long? TourId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
}
