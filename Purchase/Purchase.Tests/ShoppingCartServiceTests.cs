using Purchase.Application.DTOs;
using Purchase.Application.Interfaces;
using Purchase.Application.Services;
using Purchase.Domain.Entities;

namespace Purchase.Tests;

public class ShoppingCartServiceTests
{
    private class ShoppingCartRepositoryStub : IShoppingCartRepository
    {
        public ShoppingCart? Cart { get; set; }

        public Task<ShoppingCart?> GetByTouristUsernameAsync(string touristUsername)
        {
            return Task.FromResult(
                Cart != null && Cart.TouristUsername == touristUsername
                    ? Cart
                    : null);
        }

        public Task<ShoppingCart> AddAsync(ShoppingCart cart)
        {
            Cart = cart;
            return Task.FromResult(cart);
        }

        public Task<ShoppingCart> UpdateAsync(ShoppingCart cart)
        {
            Cart = cart;
            return Task.FromResult(cart);
        }
    }

    private class TourPurchaseTokenRepositoryStub : ITourPurchaseTokenRepository
    {
        public List<TourPurchaseToken> Tokens { get; } = new();

        public Task<List<TourPurchaseToken>> GetByTouristUsernameAsync(string touristUsername)
        {
            return Task.FromResult(Tokens
                .Where(token => token.TouristUsername == touristUsername)
                .ToList());
        }

        public Task<bool> ExistsAsync(string touristUsername, long tourId)
        {
            return Task.FromResult(Tokens.Any(token =>
                token.TouristUsername == touristUsername &&
                token.TourId == tourId));
        }

        public Task<TourPurchaseToken> AddAsync(TourPurchaseToken token)
        {
            Tokens.Add(token);
            return Task.FromResult(token);
        }
    }

    private class TourClientStub : ITourClient
    {
        private readonly Dictionary<long, TourPurchaseInfoDto> _tours = new();

        public void AddTour(long id, string name, double price, string status)
        {
            _tours[id] = new TourPurchaseInfoDto
            {
                Id = id,
                Name = name,
                Price = price,
                Status = status
            };
        }

        public Task<TourPurchaseInfoDto> GetTourPurchaseInfoAsync(long tourId)
        {
            return Task.FromResult(_tours[tourId]);
        }
    }

    private class TourSlotReservationClientStub : ITourSlotReservationClient
    {
        public List<long> ReservedTourIds { get; } = new();
        public List<long> RolledBackTourIds { get; } = new();

        public Task ReserveSlotAsync(long tourId, string touristUsername)
        {
            ReservedTourIds.Add(tourId);
            return Task.CompletedTask;
        }

        public Task RollbackSlotReservationAsync(long tourId, string touristUsername)
        {
            RolledBackTourIds.Add(tourId);
            return Task.CompletedTask;
        }
    }

    private static ShoppingCartService CreateService(
        ShoppingCartRepositoryStub cartRepository,
        TourPurchaseTokenRepositoryStub tokenRepository,
        TourClientStub tourClient,
        TourSlotReservationClientStub slotClient)
    {
        return new ShoppingCartService(
            cartRepository,
            tokenRepository,
            tourClient,
            slotClient);
    }

    [Fact]
    public async Task AddTourToCart_creates_cart_and_adds_published_tour()
    {
        var cartRepository = new ShoppingCartRepositoryStub();
        var tokenRepository = new TourPurchaseTokenRepositoryStub();
        var tourClient = new TourClientStub();
        var slotClient = new TourSlotReservationClientStub();
        tourClient.AddTour(10, "Novi Sad tour", 1500, "PUBLISHED");
        var service = CreateService(cartRepository, tokenRepository, tourClient, slotClient);

        var cart = await service.AddTourToCartAsync("tourist1", 10);

        Assert.Equal("tourist1", cart.TouristUsername);
        Assert.Single(cart.Items);
        Assert.Equal(10, cart.Items.Single().TourId);
        Assert.Equal("Novi Sad tour", cart.Items.Single().TourName);
        Assert.Equal(1500, cart.TotalPrice);
    }

    [Fact]
    public async Task AddTourToCart_throws_when_tour_is_not_published()
    {
        var cartRepository = new ShoppingCartRepositoryStub();
        var tokenRepository = new TourPurchaseTokenRepositoryStub();
        var tourClient = new TourClientStub();
        var slotClient = new TourSlotReservationClientStub();
        tourClient.AddTour(11, "Archived tour", 900, "ARCHIVED");
        var service = CreateService(cartRepository, tokenRepository, tourClient, slotClient);

        var exception = await Assert.ThrowsAsync<Exception>(() =>
            service.AddTourToCartAsync("tourist1", 11));

        Assert.Equal("Only published tours can be added to cart", exception.Message);
        Assert.Null(cartRepository.Cart);
    }

    [Fact]
    public async Task AddTourToCart_throws_when_tour_is_already_in_cart()
    {
        var cartRepository = new ShoppingCartRepositoryStub
        {
            Cart = new ShoppingCart
            {
                TouristUsername = "tourist1",
                TotalPrice = 700,
                Items = new List<OrderItem>
                {
                    new() { TourId = 12, TourName = "Existing tour", Price = 700 }
                }
            }
        };
        var tokenRepository = new TourPurchaseTokenRepositoryStub();
        var tourClient = new TourClientStub();
        var slotClient = new TourSlotReservationClientStub();
        tourClient.AddTour(12, "Existing tour", 700, "PUBLISHED");
        var service = CreateService(cartRepository, tokenRepository, tourClient, slotClient);

        var exception = await Assert.ThrowsAsync<Exception>(() =>
            service.AddTourToCartAsync("tourist1", 12));

        Assert.Equal("Tour is already in cart", exception.Message);
        Assert.Single(cartRepository.Cart.Items);
    }

    [Fact]
    public async Task RemoveItemFromCart_removes_item_and_updates_total_price()
    {
        var cartRepository = new ShoppingCartRepositoryStub
        {
            Cart = new ShoppingCart
            {
                TouristUsername = "tourist1",
                TotalPrice = 1500,
                Items = new List<OrderItem>
                {
                    new() { Id = 1, TourId = 21, TourName = "Tour one", Price = 1000 },
                    new() { Id = 2, TourId = 22, TourName = "Tour two", Price = 500 }
                }
            }
        };
        var service = CreateService(
            cartRepository,
            new TourPurchaseTokenRepositoryStub(),
            new TourClientStub(),
            new TourSlotReservationClientStub());

        var cart = await service.RemoveItemFromCartAsync("tourist1", 2);

        Assert.Single(cart.Items);
        Assert.Equal(21, cart.Items.Single().TourId);
        Assert.Equal(1000, cart.TotalPrice);
    }

    [Fact]
    public async Task Checkout_throws_when_cart_is_empty()
    {
        var cartRepository = new ShoppingCartRepositoryStub
        {
            Cart = new ShoppingCart
            {
                TouristUsername = "tourist1"
            }
        };
        var tokenRepository = new TourPurchaseTokenRepositoryStub();
        var slotClient = new TourSlotReservationClientStub();
        var service = CreateService(
            cartRepository,
            tokenRepository,
            new TourClientStub(),
            slotClient);

        var exception = await Assert.ThrowsAsync<Exception>(() =>
            service.CheckoutAsync("tourist1"));

        Assert.Equal("Shopping cart is empty", exception.Message);
        Assert.Empty(tokenRepository.Tokens);
        Assert.Empty(slotClient.ReservedTourIds);
    }

}
