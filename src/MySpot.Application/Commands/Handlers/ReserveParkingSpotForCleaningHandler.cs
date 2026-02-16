using MySpot.Application.Abstractions;
using MySpot.Core.Abstractions;
using MySpot.Core.DamainServices;
using MySpot.Core.Entities;
using MySpot.Core.Repositories;
using MySpot.Core.ValueObjects;

namespace MySpot.Application.Commands.Handlers;

public class ReserveParkingSpotForCleaningHandler : ICommandHandler<ReserveParkingSpotForCleaning>
{
    private readonly IWeeklyParkingSpotRepository _repository;
    private readonly IParkingReservationService _reservationService;
    private readonly IReservationsRepository _reservationsRepository;

    public ReserveParkingSpotForCleaningHandler(IWeeklyParkingSpotRepository repository,
        IParkingReservationService reservationService, IReservationsRepository reservationsRepository)
    {
        _repository = repository;
        _reservationService = reservationService;
        _reservationsRepository = reservationsRepository;
    }

    public async Task HandleAsync(ReserveParkingSpotForCleaning command)
    {
        var week = new Week(command.Date);
        var weeklyParkingSpots = (await _repository.GetByWeekAsync(week)).ToList();
        var date = new Date(command.Date);
        
        var vehicleReservationsToDelete = weeklyParkingSpots
            .SelectMany(x => x.Reservations)
            .OfType<VehicleReservation>()
            .Where(x => x.Date == date)
            .ToList();
        
        _reservationService.ReserveParkingForCleaning(weeklyParkingSpots, date);

        if (vehicleReservationsToDelete.Count > 0)
        {
            await _reservationsRepository.DeleteRangeAsync(vehicleReservationsToDelete);
        }

        var tasks = weeklyParkingSpots.Select(x => _repository.UpdateAsync(x));
        await Task.WhenAll(tasks);
    }
}