using MySpot.Application.Commands;
using MySpot.Application.DTO;
using MySpot.Core.Entities;
using MySpot.Core.Repositories;
using MySpot.Core.ValueObjects;

namespace MySpot.Application.Services;

public class ReservationsService : IReservationsService
{
    private readonly IClock _clock;
    private readonly IWeeklyParkingSpotRepository _weeklyParkingSpotsRepository;
    private readonly IReservationsRepository _reservationsRepository;

    public ReservationsService(
        IClock clock,
        IWeeklyParkingSpotRepository weeklyParkingSpotsRepository,
        IReservationsRepository reservationsRepository)
    {
        _clock = clock;
        _weeklyParkingSpotsRepository = weeklyParkingSpotsRepository;
        _reservationsRepository = reservationsRepository;
    }

    public async Task<ReservationDto> GetAsync(Guid id)
    {
        var reservations = await GetAllWeeklyAsync();
        return reservations.SingleOrDefault(x => x.Id == id);
    }

    public async Task<IEnumerable<ReservationDto>> GetAllWeeklyAsync()
    {
        var weeklyParkingSpots = await _weeklyParkingSpotsRepository.GetAllAsync();
            return  weeklyParkingSpots
                .SelectMany(x => x.Reservations)
                .Select(x => new ReservationDto()
                {
                    Id = x.Id,
                    ParkingSpotId = x.ParkingSpotId,
                    EmployeeName = x.EmployeeName,
                    Date = x.Date.Value.Date,
                });
    }

    public async Task<Guid?> CreateAsync(CreateReservation command)
    {
        var parkingSpotId = new ParkingSpotId(command.ParkingSpotId);
        var weeklyParkingSpot = await _weeklyParkingSpotsRepository.GetAsync(parkingSpotId);
        if(weeklyParkingSpot is null)
            return null;
        
        var reservation = new Reservation(command.ReservationId, command.ParkingSpotId, command.EmployeeName,
        command.LicensePlate, new Date(command.Date));
        weeklyParkingSpot.AddReservation(reservation, new Date (_clock.Current()));
        await _weeklyParkingSpotsRepository.UpdateAsync(weeklyParkingSpot);
        
        return reservation.Id;
    }

    public async Task<bool> UpdateAsync(ChangeReservationLicensePlate command)
    {
        var weeklyParkingSpot = await GetWeeklyParkingSpotByReservationAsync(command.ReservationId);
        if (weeklyParkingSpot is null)
            return false;

        var parkingSpotId = new ReservationId(command.ReservationId);
        var exisitngReservation = weeklyParkingSpot.Reservations.SingleOrDefault(x => x.Id == parkingSpotId);
        if (exisitngReservation is null)
            return false;

        if (exisitngReservation.Date.Value.Date < _clock.Current())
            return false;

        exisitngReservation.ChangeLicensePlate(command.LicensePlate);
        await _weeklyParkingSpotsRepository.UpdateAsync(weeklyParkingSpot);
        
        return true;
    }

    public async Task<bool> DeleteAsync(DeleteReservation command)
    {
        var reservationId = new ReservationId(command.ReservationId);
        var exisitngReservation = await _reservationsRepository.GetAsync(reservationId);
        if (exisitngReservation is null)
            return false;

        await _reservationsRepository.DeleteAsync(exisitngReservation);
        return true;
    }

    private async Task<WeeklyParkingSpot> GetWeeklyParkingSpotByReservationAsync(ReservationId reservationId)
    {
        var weeklyParkingSpots = await _weeklyParkingSpotsRepository.GetAllAsync();
        
        return weeklyParkingSpots.SingleOrDefault(x => x.Reservations.Any(r =>r.Id == reservationId));
    }
}

