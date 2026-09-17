using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Purchase.API.Controllers;
using Purchase.Application.DTOs;
using Purchase.Application.Interfaces;
using Purchase.Application.Services;
using Purchase.Domain.Entities;

namespace Purchase.Tests;

public class ShoppingCartControllerIntegrationTests
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

        public Task ReserveSlotAsync(long tourId, string touristUsername)
        {
            ReservedTourIds.Add(tourId);
            return Task.CompletedTask;
        }

        public Task RollbackSlotReservationAsync(long tourId, string touristUsername)
        {
            return Task.CompletedTask;
        }
    }

    private static ShoppingCartController CreateController(
        ShoppingCartRepositoryStub cartRepository,
        TourPurchaseTokenRepositoryStub tokenRepository,
        TourClientStub tourClient,
        TourSlotReservationClientStub slotClient,
        string username = "tourist1",
        string role = "Tourist")
    {
        var service = new ShoppingCartService(
            cartRepository,
            tokenRepository,
            tourClient,
            slotClient);

        var controller = new ShoppingCartController(service);
        var claims = new List<Claim>
        {
            new("username", username),
            new("role", role)
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
            }
        };

        return controller;
    }

    [Fact]
    public async Task AddTourToCart_returns_ok_with_updated_cart()
    {
        var cartRepository = new ShoppingCartRepositoryStub();
        var tokenRepository = new TourPurchaseTokenRepositoryStub();
        var tourClient = new TourClientStub();
        var slotClient = new TourSlotReservationClientStub();
        tourClient.AddTour(1, "City tour", 1000, "PUBLISHED");
        var controller = CreateController(cartRepository, tokenRepository, tourClient, slotClient);

        var response = await controller.AddTourToCart(1);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var cart = Assert.IsType<ShoppingCart>(okResult.Value);
        Assert.Single(cart.Items);
        Assert.Equal(1000, cart.TotalPrice);
    }

    [Fact]
    public async Task GetCart_returns_existing_cart()
    {
        var cartRepository = new ShoppingCartRepositoryStub
        {
            Cart = new ShoppingCart
            {
                TouristUsername = "tourist1",
                TotalPrice = 500,
                Items = new List<OrderItem>
                {
                    new() { Id = 1, TourId = 2, TourName = "Museum tour", Price = 500 }
                }
            }
        };
        var controller = CreateController(
            cartRepository,
            new TourPurchaseTokenRepositoryStub(),
            new TourClientStub(),
            new TourSlotReservationClientStub());

        var response = await controller.GetCart();

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var cart = Assert.IsType<ShoppingCart>(okResult.Value);
        Assert.Equal("tourist1", cart.TouristUsername);
        Assert.Single(cart.Items);
    }

    [Fact]
    public async Task RemoveItemFromCart_returns_cart_without_removed_item()
    {
        var cartRepository = new ShoppingCartRepositoryStub
        {
            Cart = new ShoppingCart
            {
                TouristUsername = "tourist1",
                TotalPrice = 1500,
                Items = new List<OrderItem>
                {
                    new() { Id = 1, TourId = 3, TourName = "Tour one", Price = 1000 },
                    new() { Id = 2, TourId = 4, TourName = "Tour two", Price = 500 }
                }
            }
        };
        var controller = CreateController(
            cartRepository,
            new TourPurchaseTokenRepositoryStub(),
            new TourClientStub(),
            new TourSlotReservationClientStub());

        var response = await controller.RemoveItemFromCart(2);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var cart = Assert.IsType<ShoppingCart>(okResult.Value);
        Assert.Single(cart.Items);
        Assert.Equal(1000, cart.TotalPrice);
    }

    [Fact]
    public async Task Checkout_returns_purchase_tokens()
    {
        var cartRepository = new ShoppingCartRepositoryStub
        {
            Cart = new ShoppingCart
            {
                TouristUsername = "tourist1",
                TotalPrice = 1200,
                Items = new List<OrderItem>
                {
                    new() { Id = 1, TourId = 5, TourName = "Fortress tour", Price = 1200 }
                }
            }
        };
        var tokenRepository = new TourPurchaseTokenRepositoryStub();
        var slotClient = new TourSlotReservationClientStub();
        var controller = CreateController(
            cartRepository,
            tokenRepository,
            new TourClientStub(),
            slotClient);

        var response = await controller.Checkout();

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var tokens = Assert.IsType<List<TourPurchaseToken>>(okResult.Value);
        Assert.Single(tokens);
        Assert.Single(slotClient.ReservedTourIds);
        Assert.Empty(cartRepository.Cart.Items);
    }

    [Fact]
    public async Task Checkout_returns_bad_request_when_cart_is_empty()
    {
        var cartRepository = new ShoppingCartRepositoryStub
        {
            Cart = new ShoppingCart
            {
                TouristUsername = "tourist1"
            }
        };
        var controller = CreateController(
            cartRepository,
            new TourPurchaseTokenRepositoryStub(),
            new TourClientStub(),
            new TourSlotReservationClientStub());

        var response = await controller.Checkout();

        var badRequest = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.NotNull(badRequest.Value);
    }
}
