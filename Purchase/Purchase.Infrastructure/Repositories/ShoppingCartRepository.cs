using Microsoft.EntityFrameworkCore;
using Purchase.Application.Interfaces;
using Purchase.Domain.Entities;
using Purchase.Infrastructure.Database;

namespace Purchase.Infrastructure.Repositories;

public class ShoppingCartRepository : IShoppingCartRepository
{
    private readonly PurchaseContext _context;

    public ShoppingCartRepository(PurchaseContext context)
    {
        _context = context;
    }

    public async Task<ShoppingCart?> GetByTouristUsernameAsync(string touristUsername)
    {
        return await _context.ShoppingCarts
            .Include(cart => cart.Items)
            .FirstOrDefaultAsync(cart => cart.TouristUsername == touristUsername);
    }

    public async Task<ShoppingCart> AddAsync(ShoppingCart cart)
    {
        _context.ShoppingCarts.Add(cart);
        await _context.SaveChangesAsync();
        return cart;
    }

    public async Task<ShoppingCart> UpdateAsync(ShoppingCart cart)
    {
        _context.ShoppingCarts.Update(cart);
        await _context.SaveChangesAsync();
        return cart;
    }
}