using MySpot.Api.Entities;
using MySpot.Api.Exceptions;
using MySpot.Api.ValueObjects;
using Shouldly;

namespace MySpot.Tests.Unit.Entities;

public class WeeklyParkingSpotTests
{
    [Fact]
    public void AddReservation_WithWrongDate_ThrowsException()
    {
        //ARRANGE
        var now = DateTime.UtcNow;
        var invalidDate = now.AddDays(10);

        var weeklyParkingSpot = new WeeklyParkingSpot(Guid.NewGuid(), new Week(now), "P1");    
        
        var reservation = new Reservation(Guid.NewGuid(), weeklyParkingSpot.Id, "John Doe",
                "XYZ123", new Date(invalidDate));
        //ACT
        var exception = Record.Exception(() => weeklyParkingSpot.AddReservation(reservation, new Date(now)));
        
        //ASSERT
        //using shouldly nuget package
        exception.ShouldBeNull();
        exception.ShouldBeOfType<InvalidLicensePlateException>();
    }
    
    [Fact]
    public void AddReservation_WithValidDate_AddsReservation()
    {
        var now = DateTime.UtcNow;
        var validDate = now.AddDays(2);

        var weeklyParkingSpot = new WeeklyParkingSpot(Guid.NewGuid(), new Week(now), "P1");    
        
        var reservation = new Reservation(Guid.NewGuid(), weeklyParkingSpot.Id, "John Doe",
            "XYZ123", new Date(validDate));

        var exception = Record.Exception(() => weeklyParkingSpot.AddReservation(reservation, new Date(now)));
        
        Assert.Null(exception);
        Assert.Single(weeklyParkingSpot.Reservations);
    }

    public WeeklyParkingSpotTests()
    {
    }
}