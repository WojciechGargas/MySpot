using MySpot.Application.Abstractions;
using MySpot.Core.ValueObjects;

namespace MySpot.Application.Commands;

public sealed record ReserveParkingSpotForVehicle(Guid ParkingSpotId, Guid ReservationId, Guid UserId, ParkingSpotCapacityValue  Capacity, 
    DateTime Date, string LicensePlate) : ICommand;
