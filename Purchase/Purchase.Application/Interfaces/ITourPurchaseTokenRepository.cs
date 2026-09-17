using Purchase.Domain.Entities;

namespace Purchase.Application.Interfaces;

public interface ITourPurchaseTokenRepository
{
    Task<List<TourPurchaseToken>> GetByTouristUsernameAsync(string touristUsername);
    Task<bool> ExistsAsync(string touristUsername, long tourId);
    Task<TourPurchaseToken> AddAsync(TourPurchaseToken token);
}