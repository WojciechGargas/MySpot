using MySpot.Core.Entities;
using MySpot.Core.Exceptions;
using MySpot.Core.ValueObjects;
using Shouldly;

namespace MySpot.Tests.Unit.Entities;

public class WeeklyParkingSpotTests
{
    [Theory]
    [InlineData("2025-01-01")]
    [InlineData("2026-05-05")]
    [InlineData("2026-12-12")]
    public void AddVehicleReservation_WithWrongDate_ThrowsException(string dateString)
    {
        // Arrange
        var invalidDate = DateTime.Parse(dateString);
        var reservation = new VehicleReservation(Guid.NewGuid(), _weeklyParkingSpot.Id, "John Doe",
                "XYZ123", new Date(invalidDate), ParkingSpotCapacityValue.Full);
        // Act
        var exception = Record.Exception(() => _weeklyParkingSpot.AddReservation(reservation, _now));
        
        // Assert
        // using shouldly nuget package
        exception.ShouldNotBeNull();
        exception.ShouldBeOfType<InvalidReservationDateException>();
    }
    
    [Theory]
    [InlineData("2026-01-01")]
    [InlineData("2026-01-02")]
    public void AddVehicleReservation_WithValidDate_AddsReservation(string dateString)
    {
        // Arrange
        var validDate = DateTime.Parse(dateString);
        var reservation = new VehicleReservation(Guid.NewGuid(), _weeklyParkingSpot.Id, "John Doe",
            "XYZ123", new Date(validDate), ParkingSpotCapacityValue.Full);
        
        // Act
        var exception = Record.Exception(() => _weeklyParkingSpot.AddReservation(reservation, _now));
        
        // Assert
        Assert.Null(exception);
        Assert.Single(_weeklyParkingSpot.Reservations);
    }
    
    [Fact]
    public void AddVehicleReservation_WhenCapacityExceeded_ThrowsException()
    {
        // Arrange
        var reservation1 = new VehicleReservation(Guid.NewGuid(), _weeklyParkingSpot.Id, "John Doe",
            "XYZ123", new Date(_now), ParkingSpotCapacityValue.Full);

        var reservation2 = new VehicleReservation(Guid.NewGuid(), _weeklyParkingSpot.Id, "Jack Black",
            "XYZ321", new Date(_now), ParkingSpotCapacityValue.Half);

        _weeklyParkingSpot.AddReservation(reservation1, _now);

        // Act
        var exception = Record.Exception(() => _weeklyParkingSpot.AddReservation(reservation2, _now));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ParkingSpotCapacityExceededException>(exception);
    }
    
    [Fact]
    public void RemoveReservation_ExistingVehicleReservation_RemovesIt()
    {
        // Arrange
        var reservation = new VehicleReservation(Guid.NewGuid(), _weeklyParkingSpot.Id, "John Doe",
            "XYZ123", _now, ParkingSpotCapacityValue.Full);
        _weeklyParkingSpot.AddReservation(reservation, _now);
        
        // Act
        _weeklyParkingSpot.RemoveReservation(reservation.Id);

        // Assert
        Assert.Empty(_weeklyParkingSpot.Reservations);
    }

    #region Arrange 
    
    private readonly Date _now;
    private readonly WeeklyParkingSpot _weeklyParkingSpot;

    public WeeklyParkingSpotTests()
    {
        _now = new Date(new DateTime(2026, 1, 1));
        _weeklyParkingSpot = WeeklyParkingSpot.Create(Guid.NewGuid(), new Week(_now), "P1");
    }
    
    #endregion
}
