using MySpot.Application.Abstractions;
using MySpot.Application.Exceptions;
using MySpot.Core.Abstractions;
using MySpot.Core.DamainServices;
using MySpot.Core.Entities;
using MySpot.Core.Repositories;
using MySpot.Core.ValueObjects;

namespace MySpot.Application.Commands.Handlers;

internal sealed class ReserveParkingSpotForVehicleHandler :ICommandHandler<ReserveParkingSpotForVehicle>
{
    private readonly IClock _clock;
    private readonly IWeeklyParkingSpotRepository _weeklyParkingSpotRepository;
    private readonly IParkingReservationService _parkingReservationService;
    private  readonly IUserRepository _userRepository;

    public ReserveParkingSpotForVehicleHandler(IClock clock, IWeeklyParkingSpotRepository weeklyParkingSpotRepository,
        IReservationsRepository reservationsRepository, IParkingReservationService parkingReservationService,
        IUserRepository userRepository)
    {
        _clock = clock;
        _weeklyParkingSpotRepository = weeklyParkingSpotRepository;
        _parkingReservationService = parkingReservationService;
        _userRepository = userRepository;
    }
    
    public async Task HandleAsync(ReserveParkingSpotForVehicle command)
    {
        var (spotId, reservationId, userId, date,  capacity, licensePlate) = command;   
        var parkingSpotId = new ParkingSpotId(command.ParkingSpotId);
        var week = new Week(_clock.Current());
        
        var weeklyParkingSpots = (await _weeklyParkingSpotRepository.GetByWeekAsync(week)).ToList();
        var parkingSpotToReserve = weeklyParkingSpots.SingleOrDefault(x => x.Id == parkingSpotId);
        
        if (parkingSpotToReserve is null)
        {
            throw new WeeklyParkingSpotNotFoundException(parkingSpotId);
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            throw new UserNotFoundException(userId);
        }
        
        var reservation = new VehicleReservation(command.ReservationId, command.ParkingSpotId, new EmployeeName(user.FullName),
            command.LicensePlate, new Date(command.Date), ParkingSpotCapacityValue.Full);
        
        _parkingReservationService.ReserveSpotForVehicle(weeklyParkingSpots, JobTitle.Employee,
            parkingSpotToReserve, reservation);
        
        await _weeklyParkingSpotRepository.UpdateAsync(parkingSpotToReserve);
    }
}
