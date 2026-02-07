using MySpot.Api.Commands;
using MySpot.Api.Entities;
using MySpot.Api.Services;
using Shouldly;

namespace MySpot.Tests.Unit.Services;

public class ReservationServiceTests
{
    [Fact]
    public void Create_WithCorrectDate_ShouldSucceed()
    {
        // Arrange
        var command = new CreateReservation(Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Guid.NewGuid(), DateTime.UtcNow.AddMinutes(5), "John Doe", "XYZ123");
        
        // Act
        var reservationId = _reservationService.Create(command);

        // Assert
        reservationId.ShouldNotBeNull();
        reservationId.ShouldBe(command.ReservationId);
    }
    
    #region Arrange
    
    private readonly ReservationsService _reservationService;

    public ReservationServiceTests()
    {
        _reservationService = new ReservationsService();
    }
    
    #endregion
}