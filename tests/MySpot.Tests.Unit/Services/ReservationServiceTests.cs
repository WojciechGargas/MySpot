using MySpot.Api.Commands;
using MySpot.Api.Entities;
using MySpot.Api.Services;
using MySpot.Api.ValueObjects;
using MySpot.Tests.Shared;
using Shouldly;

namespace MySpot.Tests.Unit.Services;

public class ReservationServiceTests
{
    [Fact]
    public void Create_WithCorrectDate_ShouldSucceed()
    {
        // Arrange
        var weeklyparkingSpot = _weeklyParkingSpots.First();
        var command = new CreateReservation(weeklyparkingSpot.Id,
            Guid.NewGuid(), _clock.Current().AddMinutes(5), "John Doe", "XYZ123");

        // Act
        var reservationId = _reservationService.Create(command);

        // Assert
        reservationId.ShouldNotBeNull();
        reservationId.ShouldBe(command.ReservationId);
    }

    #region Arrange

    private static readonly DateTime FixedNow = TestClock.FixedNow;
    private readonly IClock _clock = new TestClock();
    private readonly ReservationsService _reservationService;
    private readonly List<WeeklyParkingSpot> _weeklyParkingSpots;

    public ReservationServiceTests()
    {
        _weeklyParkingSpots = new List<WeeklyParkingSpot>()
        {
            new WeeklyParkingSpot(Guid.Parse("00000000-0000-0000-0000-000000000001"), new Week(_clock.Current()), name:"P1" ),
            new WeeklyParkingSpot(Guid.Parse("00000000-0000-0000-0000-000000000002"), new Week(_clock.Current()), name:"P2" ),
            new WeeklyParkingSpot(Guid.Parse("00000000-0000-0000-0000-000000000003"), new Week(_clock.Current()), name:"P3" ),
            new WeeklyParkingSpot(Guid.Parse("00000000-0000-0000-0000-000000000004"), new Week(_clock.Current()), name:"P4" ),
            new WeeklyParkingSpot(Guid.Parse("00000000-0000-0000-0000-000000000005"), new Week(_clock.Current()), name:"P5" ),
        };

        _reservationService = new ReservationsService(_clock, _weeklyParkingSpots);
    }

    #endregion
}
