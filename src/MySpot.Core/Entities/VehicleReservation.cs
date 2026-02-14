using MySpot.Core.ValueObjects;

namespace MySpot.Core.Entities;

public sealed class VehicleReservation : Reservation
{
    public EmployeeName EmployeeName { get; private set; }
    public LicensePlate LicensePlate { get; private set; }

    private VehicleReservation()
    {
    }

    public VehicleReservation(ReservationId id, ParkingSpotId parkingSpotId,
        EmployeeName employeeName, LicensePlate licensePlate, Date date, ParkingSpotCapacity capasity)
        :  base(id, parkingSpotId, date, capasity)
    {
        EmployeeName = employeeName;
        ChangeLicensePlate(licensePlate);
    }


    public void ChangeLicensePlate(LicensePlate newLicensePlate)
    {
        LicensePlate = newLicensePlate;
    }
}