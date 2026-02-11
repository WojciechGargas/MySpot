using MySpot.Core.Entities;
using MySpot.Core.ValueObjects;

namespace MySpot.Core.DamainServices;

public interface IParkingreservationService
{
    void ReserveSpotForVehicle(IEnumerable<WeeklyParkingSpot> allParkingSpots, JobTitle jobTitle,
        WeeklyParkingSpot parkingSpotToReserve, Reservation reservation);
}