using MySpot.Application.Abstractions;
using MySpot.Application.Exceptions;
using MySpot.Core.Abstractions;
using MySpot.Core.DamainServices;
using MySpot.Core.Entities;
using MySpot.Core.Repositories;
using MySpot.Core.ValueObjects;

namespace MySpot.Application.Commands.Handlers;

public class ChangeReservationLicensePlateHandler : ICommandHandler<ChangeReservationLicensePlate>
{
    private readonly IWeeklyParkingSpotRepository _repository;

    public ChangeReservationLicensePlateHandler(IWeeklyParkingSpotRepository repository)
        => _repository = repository;
    
    
    public async Task HandleAsync(ChangeReservationLicensePlate command)
    {
        var weeklyParkingSpot = await GetWeeklyParkingSpotByReservationAsync(command.ReservationId);
        if (weeklyParkingSpot is null)
        {
            throw new WeeklyParkingSpotNotFoundException();
        }

        var reservationId = new ReservationId(command.ReservationId);
        var exisitngReservation = weeklyParkingSpot.Reservations
            .OfType<VehicleReservation>()
            .SingleOrDefault(x => x.Id == reservationId);
        if (exisitngReservation is null)
        {
            throw new ReservationNotFoundException(command.ReservationId);
        }

        exisitngReservation.ChangeLicensePlate(command.LicensePlate);
        await _repository.UpdateAsync(weeklyParkingSpot);
    }
    private async Task<WeeklyParkingSpot?> GetWeeklyParkingSpotByReservationAsync(ReservationId reservationId)
    {
        var weeklyParkingSpots = await _repository.GetAllAsync();
        
        return weeklyParkingSpots.SingleOrDefault(x => x.Reservations.Any(r =>r.Id == reservationId));
    }
}