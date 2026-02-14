using MySpot.Core.Entities;
using MySpot.Core.ValueObjects;

namespace MySpot.Core.Repositories;

public interface IReservationsRepository
{
    Task<Reservation?> GetAsync(ReservationId id);
    Task<IEnumerable<Reservation>> GetAllAsync();
    Task<IEnumerable<Reservation>> GetByParkingSpotAsync(ParkingSpotId parkingSpotId);
    Task<IEnumerable<Reservation>> GetByWeekAsync(Week week);
    Task AddAsync(Reservation reservation);
    Task UpdateAsync(Reservation reservation);
    Task DeleteAsync(Reservation reservation);
    Task DeleteRangeAsync(IEnumerable<Reservation> reservations);
}
