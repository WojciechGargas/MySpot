using MySpot.Application.Commands;
using MySpot.Application.Services;
using MySpot.Core.Abstractions;
using MySpot.Core.DamainServices;
using MySpot.Core.Entities;
using MySpot.Core.Policies;
using MySpot.Core.Repositories;
using MySpot.Core.ValueObjects;
using MySpot.Infrastructure.DAL.Repositories;
using MySpot.Tests.Shared;
using Shouldly;

namespace MySpot.Tests.Unit.Services;

public class ReservationServiceTests
{
    [Fact]
    public async Task Create_WithCorrectDate_ShouldSucceed()
    {
        // Arrange
        var weeklyParkingSpot = (await _weeklyParkingSpotRepository.GetAllAsync()).First();

        var command = new ReserveParkingSpotForVehicle(
            weeklyParkingSpot.Id,
            Guid.NewGuid(),
            _clock.Current().AddMinutes(5),
            "John Doe",
            "XYZ123"
        );

        // Act
        var reservationId = await _reservationService.ReserveForVehicleAsync(command);

        // Assert
        reservationId.ShouldNotBeNull();
        reservationId.ShouldBe(command.ReservationId);
    }

    #region Arrange
    
    private readonly IClock _clock = new TestClock();
    private readonly IWeeklyParkingSpotRepository _weeklyParkingSpotRepository;
    private readonly IReservationsRepository _reservationsRepository = new InMemoryReservationsRepository();
    private readonly IReservationsService _reservationService;
    private readonly IParkingReservationService _parkingReservationService;

    public ReservationServiceTests()
    {
        _weeklyParkingSpotRepository = new InMemoryWeeklyParkingSpotRepository(_clock);
        var policies = new IReservationPolicy[]
        {
            new RegularEmployeeReservationPolicy(_clock),
            new ManagerReservationPolicy(),
            new BossReservationPolicy(),
        };
        _parkingReservationService = new ParkingReservationService(policies, _clock);
        _reservationService = new ReservationsService(_clock, _weeklyParkingSpotRepository, _reservationsRepository,
            _parkingReservationService);
    }
    #endregion

    private sealed class InMemoryReservationsRepository : IReservationsRepository
    {
        private readonly List<Reservation> _reservations = new();

        public Task<Reservation?> GetAsync(ReservationId id)
            => Task.FromResult(_reservations.SingleOrDefault(x => x.Id == id));

        public Task<IEnumerable<Reservation>> GetAllAsync()
            => Task.FromResult(_reservations.AsEnumerable());

        public Task<IEnumerable<Reservation>> GetByParkingSpotAsync(ParkingSpotId parkingSpotId)
            => Task.FromResult(_reservations.Where(x => x.ParkingSpotId == parkingSpotId));

        public Task<IEnumerable<Reservation>> GetByWeekAsync(Week week)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Reservation>> GetReservationByWeekAsync(Week week)
            => Task.FromResult(_reservations.Where(x => x.Date >= week.From && x.Date <= week.To));

        public Task AddAsync(Reservation reservation)
        {
            _reservations.Add(reservation);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Reservation reservation) => Task.CompletedTask;

        public Task DeleteAsync(Reservation reservation)
        {
            _reservations.Remove(reservation);
            return Task.CompletedTask;
    }
}

}
