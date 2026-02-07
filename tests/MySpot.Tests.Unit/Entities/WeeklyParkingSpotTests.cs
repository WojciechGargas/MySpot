using System.Data;
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
        //ARRANGE
        var now = new DateTime(2026, 1, 1);
        var invalidDate = DateTime.Parse(dateString);

        var weeklyParkingSpot = new WeeklyParkingSpot(Guid.NewGuid(), new Week(now), "P1");    
        
        var reservation = new Reservation(Guid.NewGuid(), weeklyParkingSpot.Id, "John Doe",
                "XYZ123", new Date(invalidDate));
        //ACT
        var exception = Record.Exception(() => weeklyParkingSpot.AddReservation(reservation, new Date(now)));
        
        //ASSERT
        //using shouldly nuget package
        exception.ShouldNotBeNull();
        exception.ShouldBeOfType<InvalidReservationDateException>();
    }
    
    [Theory]
    [InlineData("2026-01-01")]
    [InlineData("2026-01-02")]
    public void AddReservation_WithValidDate_AddsReservation(string dateString)
    {
        var now = new DateTime(2026, 1, 1);
        var validDate = DateTime.Parse(dateString);

        var weeklyParkingSpot = new WeeklyParkingSpot(Guid.NewGuid(), new Week(now), "P1");    
        
        var reservation = new Reservation(Guid.NewGuid(), weeklyParkingSpot.Id, "John Doe",
            "XYZ123", new Date(validDate));

        var exception = Record.Exception(() => weeklyParkingSpot.AddReservation(reservation, new Date(now)));
        
        Assert.Null(exception);
        Assert.Single(weeklyParkingSpot.Reservations);
    }
    
    [Fact]
    public void AddReservation_AlreadyReservedForDate_ThrowsException()
    {
        var now = new DateTime(2026, 1, 1);
        var validDate = now.AddDays(2);

        var weeklyParkingSpot = new WeeklyParkingSpot(Guid.NewGuid(), new Week(now), "P1");    
        
        var reservation1 = new Reservation(Guid.NewGuid(), weeklyParkingSpot.Id, "John Doe",
            "XYZ123", new Date(validDate));
        
        var reservation2 = new Reservation(Guid.NewGuid(), weeklyParkingSpot.Id, "Jack Black",
            "XYZ321", new Date(validDate));

        var firstException = Record.Exception(() => weeklyParkingSpot.AddReservation(reservation1, new Date(now)));
        
        Assert.Null(firstException);
        Assert.Single(weeklyParkingSpot.Reservations);
        
        var secondException = Record.Exception(() => weeklyParkingSpot.AddReservation(reservation2, new Date(now)));
        Assert.NotNull(secondException);
        Assert.IsType<ParkingSpotAlreadyReservedException>(secondException);
    }
    
    [Fact]
    public void RemoveReservation_ExistingReservation_RemovesIt()
    {
        var now = DateTime.UtcNow;
        var weeklyParkingSpot = new WeeklyParkingSpot(Guid.NewGuid(), new Week(now), "P1");

        var reservation = new Reservation(Guid.NewGuid(), weeklyParkingSpot.Id, "John Doe",
            "XYZ123", new Date(now.AddDays(1)));

        weeklyParkingSpot.AddReservation(reservation, new Date(now));

        Assert.Single(weeklyParkingSpot.Reservations);
        
        weeklyParkingSpot.RemoveReservation(reservation.Id);
        
        Assert.Empty(weeklyParkingSpot.Reservations);
    }
    

    public WeeklyParkingSpotTests()
    {
    }
}