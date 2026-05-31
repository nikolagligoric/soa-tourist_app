namespace Purchase.Application.DTOs;

public class TourSlotCommandDto
{
    public long TourId { get; set; }
    public string TouristUsername { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}
