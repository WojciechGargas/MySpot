using MySpot.Core.ValueObjects;

namespace MySpot.Core.Exceptions;

public sealed class InvalidParkingSpotCapacityException : CustomException
{
    public ParkingSpotCapacityValue Capacity { get; set; }

    public InvalidParkingSpotCapacityException(ParkingSpotCapacityValue capacity) 
        : base($"Capacity {capacity} is invalid.")
    {
        Capacity = capacity;
    }
}