namespace Purchase.Domain.Entities;

public class ShoppingCart
{
    public long Id { get; set; }

    public string TouristUsername { get; set; } = string.Empty;

    public double TotalPrice { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}