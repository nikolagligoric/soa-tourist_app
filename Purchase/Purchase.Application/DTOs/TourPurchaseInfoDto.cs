namespace Purchase.Application.DTOs;

public class TourPurchaseInfoDto
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public double Price { get; set; }

    public string Status { get; set; } = string.Empty;
}