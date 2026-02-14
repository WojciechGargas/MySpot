using MySpot.Core.Exceptions;

namespace MySpot.Core.ValueObjects;
public enum ParkingSpotCapacityValue
{
    OneQuarter = 1,
    Half = 2,
    ThreeQuarter = 3,
    Full = 4,
}

public sealed record ParkingSpotCapacity
{
    public ParkingSpotCapacityValue Value { get; private set; }

    
    public ParkingSpotCapacity(ParkingSpotCapacityValue value)
    {
        if (!Enum.IsDefined(typeof(ParkingSpotCapacityValue), value))
        {
            throw new InvalidParkingSpotCapacityException(value);
        }
        
        Value = value;
    }
    
    public static implicit operator ParkingSpotCapacityValue(ParkingSpotCapacity capacity) 
        => capacity.Value;
    
    public static implicit operator ParkingSpotCapacity(ParkingSpotCapacityValue value)
        => new ParkingSpotCapacity(value);
}