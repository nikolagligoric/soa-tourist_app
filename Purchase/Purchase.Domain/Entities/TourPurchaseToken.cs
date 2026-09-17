namespace Purchase.Domain.Entities;

public class TourPurchaseToken
{
    public long Id { get; set; }

    public string TouristUsername { get; set; } = string.Empty;

    public long TourId { get; set; }

    public string Token { get; set; } = string.Empty;

    public DateTime PurchasedAt { get; set; }
}