using System.Net.Http.Json;
using Purchase.Application.DTOs;
using Purchase.Application.Interfaces;

namespace Purchase.Infrastructure.Clients;

public class TourClient : ITourClient
{
    private readonly HttpClient _httpClient;

    public TourClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<TourPurchaseInfoDto> GetTourPurchaseInfoAsync(long tourId)
    {
        TourPurchaseInfoDto? tour = await _httpClient
            .GetFromJsonAsync<TourPurchaseInfoDto>($"/api/tours/{tourId}/purchase-info");

        if (tour == null)
        {
            throw new Exception("Tour not found");
        }

        return tour;
    }
}