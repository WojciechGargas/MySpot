using MySpot.Api.ValueObjects;

namespace MySpot.Api.Exceptions;


public sealed class CannotReserveParkingSpotException : CustomException
{
    public ParkingSpotId ParkingSpotId { get; }

    public CannotReserveParkingSpotException(ParkingSpotId parkingSpotId) 
        : base($"Cannot reserve parking spot with ID: {parkingSpotId} due to reservation policy.")
    {
        ParkingSpotId = parkingSpotId;
    }
}