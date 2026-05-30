using Purchase.Application.Interfaces;
using Purchase.Domain.Entities;

namespace Purchase.Application.Services;

public class ShoppingCartService
{
    private readonly IShoppingCartRepository _shoppingCartRepository;
    private readonly ITourPurchaseTokenRepository _tourPurchaseTokenRepository;
    private readonly ITourClient _tourClient;

    public ShoppingCartService(
        IShoppingCartRepository shoppingCartRepository,
        ITourPurchaseTokenRepository tourPurchaseTokenRepository,
        ITourClient tourClient)
    {
        _shoppingCartRepository = shoppingCartRepository;
        _tourPurchaseTokenRepository = tourPurchaseTokenRepository;
        _tourClient = tourClient;
    }

    public async Task<ShoppingCart> AddTourToCartAsync(string touristUsername, long tourId)
    {
        var tour = await _tourClient.GetTourPurchaseInfoAsync(tourId);

        if (tour.Status != "PUBLISHED")
        {
            throw new Exception("Only published tours can be added to cart");
        }

        var cart = await _shoppingCartRepository.GetByTouristUsernameAsync(touristUsername);

        if (cart == null)
        {
            cart = new ShoppingCart
            {
                TouristUsername = touristUsername,
                TotalPrice = 0
            };

            cart = await _shoppingCartRepository.AddAsync(cart);
        }

        bool alreadyInCart = cart.Items.Any(item => item.TourId == tourId);

        if (alreadyInCart)
        {
            throw new Exception("Tour is already in cart");
        }

        var item = new OrderItem
        {
            TourId = tour.Id,
            TourName = tour.Name,
            Price = tour.Price,
            ShoppingCart = cart
        };

        cart.Items.Add(item);

        RecalculateTotalPrice(cart);

        return await _shoppingCartRepository.UpdateAsync(cart);
    }

    public async Task<ShoppingCart> GetCartAsync(string touristUsername)
    {
        var cart = await _shoppingCartRepository.GetByTouristUsernameAsync(touristUsername);

        if (cart == null)
        {
            cart = new ShoppingCart
            {
                TouristUsername = touristUsername,
                TotalPrice = 0
            };

            return await _shoppingCartRepository.AddAsync(cart);
        }

        return cart;
    }

    public async Task<ShoppingCart> RemoveItemFromCartAsync(string touristUsername, long itemId)
    {
        var cart = await _shoppingCartRepository.GetByTouristUsernameAsync(touristUsername);

        if (cart == null)
        {
            throw new Exception("Shopping cart not found");
        }

        var itemToRemove = cart.Items
            .FirstOrDefault(item => item.Id == itemId);

        if (itemToRemove == null)
        {
            throw new Exception("Item not found in cart");
        }

        cart.Items.Remove(itemToRemove);

        RecalculateTotalPrice(cart);

        return await _shoppingCartRepository.UpdateAsync(cart);
    }

    public async Task<List<TourPurchaseToken>> CheckoutAsync(string touristUsername)
    {
        var cart = await _shoppingCartRepository.GetByTouristUsernameAsync(touristUsername);

        if (cart == null)
        {
            throw new Exception("Shopping cart not found");
        }

        if (!cart.Items.Any())
        {
            throw new Exception("Shopping cart is empty");
        }

        var tokens = new List<TourPurchaseToken>();

        foreach (var item in cart.Items)
        {
            bool alreadyPurchased = await _tourPurchaseTokenRepository
                .ExistsAsync(touristUsername, item.TourId);

            if (!alreadyPurchased)
            {
                var purchaseToken = new TourPurchaseToken
                {
                    TouristUsername = touristUsername,
                    TourId = item.TourId,
                    Token = Guid.NewGuid().ToString(),
                    PurchasedAt = DateTime.UtcNow
                };

                tokens.Add(await _tourPurchaseTokenRepository.AddAsync(purchaseToken));
            }
        }

        cart.Items.Clear();
        cart.TotalPrice = 0;

        await _shoppingCartRepository.UpdateAsync(cart);

        return tokens;
    }

    public async Task<bool> HasPurchasedAsync(string touristUsername, long tourId)
    {
        return await _tourPurchaseTokenRepository.ExistsAsync(touristUsername, tourId);
    }

    private void RecalculateTotalPrice(ShoppingCart cart)
    {
        cart.TotalPrice = cart.Items.Sum(item => item.Price);
    }
}