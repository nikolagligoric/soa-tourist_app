namespace Purchase.Application.Interfaces;

public interface ITourSlotReservationClient
{
    Task ReserveSlotAsync(long tourId, string touristUsername);
    Task RollbackSlotReservationAsync(long tourId, string touristUsername);
}
