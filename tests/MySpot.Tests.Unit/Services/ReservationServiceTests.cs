using MySpot.Application.Commands;
using MySpot.Application.Services;
using MySpot.Core.Entities;
using MySpot.Core.Repositories;
using MySpot.Core.ValueObjects;
using MySpot.Infrastructure.DAL.Repositories;
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
    private readonly IReservationsRepository _reservationsRepository = new InMemoryReservationsRepository();
    private readonly IReservationsService _reservationService;

    public ReservationServiceTests()
    {
        _weeklyParkingSpotRepository = new InMemoryWeeklyParkingSpotRepository(_clock);
        _reservationService = new ReservationsService(_clock, _weeklyParkingSpotRepository, _reservationsRepository);
    }

    #endregion

    private sealed class InMemoryReservationsRepository : IReservationsRepository
    {
        private readonly List<Reservation> _reservations = new();

        public Reservation Get(ReservationId id)
            => _reservations.SingleOrDefault(x => x.Id == id);

        public IEnumerable<Reservation> GetAll()
            => _reservations;

        public IEnumerable<Reservation> GetByParkingSpot(ParkingSpotId parkingSpotId)
            => _reservations.Where(x => x.ParkingSpotId == parkingSpotId);

        public IEnumerable<Reservation> GetByWeek(Week week)
            => _reservations.Where(x => x.Date >= week.From && x.Date <= week.To);

        public void Add(Reservation reservation)
            => _reservations.Add(reservation);

        public void Update(Reservation reservation)
        {
        }

        public void Delete(Reservation reservation)
            => _reservations.Remove(reservation);
    }
}
