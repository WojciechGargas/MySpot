using MySpot.Core.Entities;
using MySpot.Core.ValueObjects;

namespace MySpot.Core.DamainServices;

public interface IParkingReservationService
{
    void ReserveSpotForVehicle(IEnumerable<WeeklyParkingSpot> allParkingSpots, JobTitle jobTitle,
        WeeklyParkingSpot parkingSpotToReserve, VehicleReservation reservation);

    void ReserveParkingForCleaning(IEnumerable<WeeklyParkingSpot> allParkingSpots, Date date);
}