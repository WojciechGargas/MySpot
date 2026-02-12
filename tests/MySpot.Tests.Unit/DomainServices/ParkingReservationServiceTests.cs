using MySpot.Core.Abstractions;
using MySpot.Core.DamainServices;
using MySpot.Core.Entities;
using MySpot.Core.Policies;
using MySpot.Core.ValueObjects;
using MySpot.Tests.Shared;
using Shouldly;

namespace MySpot.Tests.Unit.DomainServices;

public class ParkingReservationServiceTests
{
    [Fact]
    public void ReserveParkingForCleaning_RemovesReservationsForSameDate_AndAddsCleaningReservation()
    {
        // Arrange
        var date = new Date(_clock.Current());
        var weeklyParkingSpots = new List<WeeklyParkingSpot>
        {
            new(Guid.NewGuid(), new Week(date), "P1"),
            new(Guid.NewGuid(), new Week(date), "P2"),
        };

        foreach (var spot in weeklyParkingSpots)
        {
            var vehicleReservation = new VehicleReservation(ReservationId.Create(), spot.Id, "John Doe", "XYZ123", date);
            spot.AddReservation(vehicleReservation, date);
        }

        // Act
        _parkingReservationService.ReserveParkingForCleaning(weeklyParkingSpots, date);

        // Assert
        foreach (var spot in weeklyParkingSpots)
        {
            spot.Reservations.Count().ShouldBe(1);
            spot.Reservations.Single().ShouldBeOfType<CleaningReservation>();
            spot.Reservations.Single().Date.ShouldBe(date);
        }
    }

    #region Arrange

    private readonly IClock _clock = new TestClock();
    private readonly IParkingReservationService _parkingReservationService;

    public ParkingReservationServiceTests()
    {
        var policies = Array.Empty<IReservationPolicy>();
        _parkingReservationService = new ParkingReservationService(policies, _clock);
    }

    #endregion
}
