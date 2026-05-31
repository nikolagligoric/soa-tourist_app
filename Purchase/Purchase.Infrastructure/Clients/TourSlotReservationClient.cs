using System.Text.Json;
using NATS.Client;
using Purchase.Application.DTOs;
using Purchase.Application.Interfaces;

namespace Purchase.Infrastructure.Clients;

public class TourSlotReservationClient : ITourSlotReservationClient
{
    private readonly string _natsUrl;
    private readonly string _commandSubject;
    private readonly JsonSerializerOptions _jsonOptions;

    public TourSlotReservationClient(string natsUrl, string commandSubject)
    {
        _natsUrl = natsUrl;
        _commandSubject = commandSubject;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public Task ReserveSlotAsync(long tourId, string touristUsername)
    {
        return SendCommandAsync(tourId, touristUsername, "ReserveTourSlot", "TourSlotReserved");
    }

    public Task RollbackSlotReservationAsync(long tourId, string touristUsername)
    {
        return SendCommandAsync(tourId, touristUsername, "RollbackTourSlotReservation", "TourSlotReservationRolledBack");
    }

    private Task SendCommandAsync(long tourId, string touristUsername, string commandType, string expectedReplyType)
    {
        var command = new TourSlotCommandDto
        {
            TourId = tourId,
            TouristUsername = touristUsername,
            Type = commandType
        };

        try
        {
            using var connection = new ConnectionFactory().CreateConnection(_natsUrl);
            var payload = JsonSerializer.SerializeToUtf8Bytes(command, _jsonOptions);
            var response = connection.Request(_commandSubject, payload, 5000);

            var reply = JsonSerializer.Deserialize<TourSlotReplyDto>(response.Data, _jsonOptions);

            if (reply == null)
            {
                throw new Exception("Empty tour slot reply.");
            }

            if (reply.Type != expectedReplyType)
            {
                throw new Exception(reply.FailureReason ?? $"Unexpected tour slot reply: {reply.Type}");
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new Exception($"Tour slot reservation command failed: {ex.Message}", ex);
        }
    }
}
