using MySpot.Application.Commands;
using MySpot.Application.Commands.Handlers;
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
    public async Task ReserveVehicle_WithCorrectDate_ShouldSucceed()
    {
        // Arrange
        var weeklyParkingSpot = (await _weeklyParkingSpotRepository.GetAllAsync()).First();
        var reservationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var employeeName = "John Doe";
        await _userRepository.AddAsync(new User(
            userId,
            "john.doe@example.com",
            "john-doe",
            "secret123",
            employeeName,
            "user",
            _clock.Current()));

        var command = new ReserveParkingSpotForVehicle(
            weeklyParkingSpot.Id,
            reservationId,
            userId,
            ParkingSpotCapacityValue.Full,
            _clock.Current().AddMinutes(5),
            "XYZ123"
        );

        // Act
        await _reserveParkingSpotForVehicleHandler.HandleAsync(command);

        // Assert
        var refreshedParkingSpot = (await _weeklyParkingSpotRepository.GetAllAsync())
            .Single(x => x.Id == weeklyParkingSpot.Id);
        var reservation = refreshedParkingSpot.Reservations.Single();
        reservation.Id.Value.ShouldBe(reservationId);
        reservation.ShouldBeOfType<VehicleReservation>();
        ((VehicleReservation)reservation).EmployeeName.Value.ShouldBe(employeeName);
    }

    #region Arrange
    
    private readonly IClock _clock = new TestClock();
    private readonly IWeeklyParkingSpotRepository _weeklyParkingSpotRepository;
    private readonly IReservationsRepository _reservationsRepository = new InMemoryReservationsRepository();
    private readonly IUserRepository _userRepository = new InMemoryUsersRepository();
    private readonly IParkingReservationService _parkingReservationService;
    private readonly ReserveParkingSpotForVehicleHandler _reserveParkingSpotForVehicleHandler;

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
        _reserveParkingSpotForVehicleHandler = new ReserveParkingSpotForVehicleHandler(
            _clock,
            _weeklyParkingSpotRepository,
            _reservationsRepository,
            _parkingReservationService,
            _userRepository);
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

        public Task DeleteRangeAsync(IEnumerable<Reservation> reservations)
        {
            var idsToRemove = reservations.Select(r => r.Id).ToHashSet();
            _reservations.RemoveAll(r => idsToRemove.Contains(r.Id));
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryUsersRepository : IUserRepository
    {
        private readonly List<User> _users = new();

        public Task<User?> GetByIdAsync(UserId id)
            => Task.FromResult(_users.SingleOrDefault(x => x.Id == id));

        public Task<User?> GetByEmailAsync(Email email)
            => Task.FromResult(_users.SingleOrDefault(x => x.Email == email));

        public Task<User?> GetByUsernameAsync(Username username)
            => Task.FromResult(_users.SingleOrDefault(x => x.Username == username));

        public Task AddAsync(User user)
        {
            _users.Add(user);
            return Task.CompletedTask;
        }
    }

}
