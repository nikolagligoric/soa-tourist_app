using Microsoft.AspNetCore.Mvc;
using Purchase.Application.Services;
using Purchase.Domain.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Purchase.API.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public class ShoppingCartController : ControllerBase
{
    private readonly ShoppingCartService _shoppingCartService;

    public ShoppingCartController(ShoppingCartService shoppingCartService)
    {
        _shoppingCartService = shoppingCartService;
    }

    [HttpPost("add/{tourId}")]
    public async Task<ActionResult<ShoppingCart>> AddTourToCart(long tourId)
    {
        var username = GetUsername();
        var role = GetRole();

        if (role != "Tourist")
        {
            return Forbid("Only tourists can use shopping cart");
        }

        var cart = await _shoppingCartService.AddTourToCartAsync(username, tourId);

        return Ok(cart);
    }

    [HttpGet]
    public async Task<ActionResult<ShoppingCart>> GetCart()
    {
        var username = GetUsername();
        var role = GetRole();

        if (role != "Tourist")
        {
            return Forbid("Only tourists can view shopping cart");
        }

        var cart = await _shoppingCartService.GetCartAsync(username);

        return Ok(cart);
    }

    [HttpDelete("items/{itemId}")]
    public async Task<ActionResult<ShoppingCart>> RemoveItemFromCart(long itemId)
    {
        var username = GetUsername();
        var role = GetRole();

        if (role != "Tourist")
        {
            return Forbid("Only tourists can remove items from shopping cart");
        }

        var cart = await _shoppingCartService.RemoveItemFromCartAsync(username, itemId);

        return Ok(cart);
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<List<TourPurchaseToken>>> Checkout()
    {
        var username = GetUsername();
        var role = GetRole();

        if (role != "Tourist")
        {
            return Forbid("Only tourists can checkout");
        }

        try
        {
            var tokens = await _shoppingCartService.CheckoutAsync(username);

            return Ok(tokens);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private string GetUsername()
    {
        return User.FindFirst("username")?.Value
            ?? User.FindFirst(ClaimTypes.Name)?.Value
            ?? throw new Exception("Username not found in token");
    }

    private string GetRole()
    {
        return User.FindFirst("role")?.Value
            ?? User.FindFirst(ClaimTypes.Role)?.Value
            ?? throw new Exception("Role not found in token");
    }

    [HttpGet("has-purchased/{tourId}")]
    public async Task<ActionResult<bool>> HasPurchased(long tourId)
    {
        var username = GetUsername();
        var role = GetRole();

        if (role != "Tourist")
        {
            return Forbid("Only tourists can check purchased tours");
        }

        var hasPurchased = await _shoppingCartService.HasPurchasedAsync(username, tourId);

        return Ok(hasPurchased);
    }

    [HttpGet("purchased/{touristUsername}")]
    public async Task<IActionResult> GetPurchasedTours(string touristUsername)
    {
        try
        {
            var purchasedTours = await _shoppingCartService.GetPurchasedToursAsync(touristUsername);

            return Ok(purchasedTours);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
