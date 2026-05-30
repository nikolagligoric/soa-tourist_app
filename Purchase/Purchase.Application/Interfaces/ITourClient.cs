using Purchase.Application.DTOs;

namespace Purchase.Application.Interfaces;

public interface ITourClient
{
    Task<TourPurchaseInfoDto> GetTourPurchaseInfoAsync(long tourId);
}