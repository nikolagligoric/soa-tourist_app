using System.Text.Json.Serialization;

namespace Purchase.Domain.Entities;

public class OrderItem
{
    public long Id { get; set; }

    public long TourId { get; set; }

    public string TourName { get; set; } = string.Empty;

    public double Price { get; set; }

    public long ShoppingCartId { get; set; }

    [JsonIgnore]
    public ShoppingCart ShoppingCart { get; set; } = null!;
}