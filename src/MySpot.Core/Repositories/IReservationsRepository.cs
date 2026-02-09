using MySpot.Core.Entities;
using MySpot.Core.ValueObjects;

namespace MySpot.Core.Repositories;

public interface IReservationsRepository
{
    Reservation Get(ReservationId id);
    IEnumerable<Reservation> GetAll();
    IEnumerable<Reservation> GetByParkingSpot(ParkingSpotId parkingSpotId);
    IEnumerable<Reservation> GetByWeek(Week week);
    void Add(Reservation reservation);
    void Update(Reservation reservation);
    void Delete(Reservation reservation);
}