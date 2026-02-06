using MySpot.Api.ValueObjects;

namespace MySpot.Api.Exceptions;

public sealed class ParkingSpotCapacityExceededException : CustomException
{
    public ParkingSpotId ParkingSpotId { get; }

    public ParkingSpotCapacityExceededException(ParkingSpotId parkingSpotId) 
        : base($"Parking spot with ID: {parkingSpotId} exceeded its reservation capacity.")
    {
        ParkingSpotId = parkingSpotId;
    }
}