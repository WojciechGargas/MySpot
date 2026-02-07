using MySpot.Api.Commands;
using MySpot.Api.Entities;
using MySpot.Api.Repositories;
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
        var weeklyparkingSpot = _weeklyParkingSpotRepository.GetAll().First();
        var command = new CreateReservation(weeklyparkingSpot.Id,
            Guid.NewGuid(), _clock.Current().AddMinutes(5), "John Doe", "XYZ123");

        // Act
        var reservationId = _reservationService.Create(command);

        // Assert
        reservationId.ShouldNotBeNull();
        reservationId.ShouldBe(command.ReservationId);
    }

    #region Arrange
    
    private readonly IClock _clock = new TestClock();
    private readonly IWeeklyParkingSpotRepository _weeklyParkingSpotRepository;
    private readonly IReservationsService _reservationService;

    public ReservationServiceTests()
    {
        _weeklyParkingSpotRepository = new InMemoryWeeklyParkingSpotRepository(_clock);
        _reservationService = new ReservationsService(_clock, _weeklyParkingSpotRepository);
    }

    #endregion
}
