using MySpot.Application.Abstractions;
using MySpot.Core.ValueObjects;

namespace MySpot.Application.Commands;

public record ReserveParkingSpotForVehicle(Guid ParkingSpotId, Guid ReservationId, ParkingSpotCapacityValue  Capacity, 
    DateTime Date, string EmployeeName, string LicensePlate) : ICommand;
