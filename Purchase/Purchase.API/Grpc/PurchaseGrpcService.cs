using Grpc.Core;
using Purchase.Application.Services;
using PurchaseRpc;

namespace Purchase.API.Grpc;

public class PurchaseGrpcService : PurchaseRpcService.PurchaseRpcServiceBase
{
    private readonly ShoppingCartService _shoppingCartService;

    public PurchaseGrpcService(ShoppingCartService shoppingCartService)
    {
        _shoppingCartService = shoppingCartService;
    }

    public override async Task<HasPurchasedTourResponse> HasPurchasedTour(
        HasPurchasedTourRequest request,
        ServerCallContext context)
    {
        Console.WriteLine("GRPC METHOD CALLED");

        var hasPurchased = await _shoppingCartService.HasPurchasedAsync(
            request.Username,
            request.TourId
        );

        Console.WriteLine(
            $"GRPC HasPurchasedTour called: {request.Username} {request.TourId}"
        );

        return new HasPurchasedTourResponse
        {
            HasPurchased = hasPurchased
        };
    }
}