using Microsoft.EntityFrameworkCore;
using Purchase.Application.Interfaces;
using Purchase.Domain.Entities;
using Purchase.Infrastructure.Database;

namespace Purchase.Infrastructure.Repositories;

public class TourPurchaseTokenRepository : ITourPurchaseTokenRepository
{
    private readonly PurchaseContext _context;

    public TourPurchaseTokenRepository(PurchaseContext context)
    {
        _context = context;
    }

    public async Task<List<TourPurchaseToken>> GetByTouristUsernameAsync(string touristUsername)
    {
        return await _context.TourPurchaseTokens
            .Where(token => token.TouristUsername == touristUsername)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string touristUsername, long tourId)
    {
        return await _context.TourPurchaseTokens
            .AnyAsync(token =>
                token.TouristUsername == touristUsername &&
                token.TourId == tourId);
    }

    public async Task<TourPurchaseToken> AddAsync(TourPurchaseToken token)
    {
        _context.TourPurchaseTokens.Add(token);
        await _context.SaveChangesAsync();
        return token;
    }
}