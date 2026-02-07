using MySpot.Api.Entities;
using MySpot.Api.Exceptions;
using MySpot.Api.ValueObjects;
using Shouldly;

namespace MySpot.Tests.Unit.Entities;

public class WeeklyParkingSpotTests
{
    [Theory]
    [InlineData("2025-01-01")]
    [InlineData("2026-05-05")]
    [InlineData("2026-12-12")]
    public void AddReservation_WithWrongDate_ThrowsException(string dateString)
    {
        // Arrange
        var invalidDate = DateTime.Parse(dateString);
        var reservation = new Reservation(Guid.NewGuid(), _weeklyParkingSpot.Id, "John Doe",
                "XYZ123", new Date(invalidDate));
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
    public void AddReservation_WithValidDate_AddsReservation(string dateString)
    {
        // Arrange
        var validDate = DateTime.Parse(dateString);
        var reservation = new Reservation(Guid.NewGuid(), _weeklyParkingSpot.Id, "John Doe",
            "XYZ123", new Date(validDate));
        
        // Act
        var exception = Record.Exception(() => _weeklyParkingSpot.AddReservation(reservation, _now));
        
        // Assert
        Assert.Null(exception);
        Assert.Single(_weeklyParkingSpot.Reservations);
    }
    
    [Fact]
    public void AddReservation_AlreadyReservedForDate_ThrowsException()
    {
        // Arrange
        var reservation1 = new Reservation(Guid.NewGuid(), _weeklyParkingSpot.Id, "John Doe",
            "XYZ123", new Date(_now));
        
        var reservation2 = new Reservation(Guid.NewGuid(), _weeklyParkingSpot.Id, "Jack Black",
            "XYZ321", new Date(_now));
        _weeklyParkingSpot.AddReservation(reservation1, _now);

        // Act
        var exception = Record.Exception(() => _weeklyParkingSpot.AddReservation(reservation2, _now));
        
        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ParkingSpotAlreadyReservedException>(exception);
    }
    
    [Fact]
    public void RemoveReservation_ExistingReservation_RemovesIt()
    {
        // Arrange
        var reservation = new Reservation(Guid.NewGuid(), _weeklyParkingSpot.Id, "John Doe",
            "XYZ123", _now);
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
        _weeklyParkingSpot = new WeeklyParkingSpot(Guid.NewGuid(), new Week(_now), "P1");
    }
    
    #endregion
}