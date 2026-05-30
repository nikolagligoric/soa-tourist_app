using Microsoft.EntityFrameworkCore;
using Purchase.Domain.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Purchase.Infrastructure.Database;

public class PurchaseContext : DbContext
{
    public PurchaseContext(DbContextOptions<PurchaseContext> options)
        : base(options)
    {
    }

    public DbSet<ShoppingCart> ShoppingCarts { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<TourPurchaseToken> TourPurchaseTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ShoppingCart>()
            .HasMany(cart => cart.Items)
            .WithOne(item => item.ShoppingCart)
            .HasForeignKey(item => item.ShoppingCartId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ShoppingCart>()
            .HasIndex(cart => cart.TouristUsername)
            .IsUnique();

        modelBuilder.Entity<TourPurchaseToken>()
            .HasIndex(token => new { token.TouristUsername, token.TourId })
            .IsUnique();
    }
}