using Purchase.Domain.Entities;

namespace Purchase.Application.Interfaces;

public interface IShoppingCartRepository
{
    Task<ShoppingCart?> GetByTouristUsernameAsync(string touristUsername);
    Task<ShoppingCart> AddAsync(ShoppingCart cart);
    Task<ShoppingCart> UpdateAsync(ShoppingCart cart);
}