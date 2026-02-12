using MySpot.Application.Commands;
using MySpot.Application.DTO;
using MySpot.Core.Abstractions;
using MySpot.Core.DamainServices;
using MySpot.Core.Entities;
using MySpot.Core.Repositories;
using MySpot.Core.ValueObjects;

namespace MySpot.Application.Services;

public class ReservationsService : IReservationsService
{
    private readonly IClock _clock;
    private readonly IWeeklyParkingSpotRepository _weeklyParkingSpotsRepository;
    private readonly IReservationsRepository _reservationsRepository;
    private readonly IParkingReservationService _parkingReservationService;

    public ReservationsService(
        IClock clock,
        IWeeklyParkingSpotRepository weeklyParkingSpotsRepository,
        IReservationsRepository reservationsRepository,
        IParkingReservationService parkingReservationService)
    {
        _clock = clock;
        _weeklyParkingSpotsRepository = weeklyParkingSpotsRepository;
        _reservationsRepository = reservationsRepository;
        _parkingReservationService = parkingReservationService;
    }

    public async Task<ReservationDto?> GetAsync(Guid id)
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
                    EmployeeName = x is VehicleReservation vr ? vr.EmployeeName : String.Empty,
                    Date = x.Date.Value.Date,
                });
    }

    public async Task<Guid?> ReserveForVehicleAsync(ReserveParkingSpotForVehicle command)
    {
        var parkingSpotId = new ParkingSpotId(command.ParkingSpotId);
        var week = new Week(_clock.Current());
        
        var weeklyParkingSpots = (await _weeklyParkingSpotsRepository.GetByWeekAsync(week)).ToList();
        var parkingSpotToReserve = weeklyParkingSpots.SingleOrDefault(x => x.Id == parkingSpotId);
        if(parkingSpotToReserve is null)
            return null;
        
        var reservation = new VehicleReservation(command.ReservationId, command.ParkingSpotId, command.EmployeeName,
        command.LicensePlate, new Date(command.Date));
        
        _parkingReservationService.ReserveSpotForVehicle(weeklyParkingSpots, JobTitle.Employee,
            parkingSpotToReserve, reservation);
        
        await _weeklyParkingSpotsRepository.UpdateAsync(parkingSpotToReserve);
        
        return reservation.Id;
    }

    public async Task ReserveForCleaningAsync(ReserveParkingSpotForCleaning command)
    {
        var week = new Week(command.Date);
        var weeklyParkingSpots = (await _weeklyParkingSpotsRepository.GetByWeekAsync(week)).ToList();
        
        _parkingReservationService.ReserveParkingForCleaning(weeklyParkingSpots, new Date(command.Date));

        foreach (var parkingSpot in weeklyParkingSpots)
        {
            await _weeklyParkingSpotsRepository.UpdateAsync(parkingSpot);
        }
    }

    public async Task<bool> ChangeReservationLicensePlateAsync(ChangeReservationLicensePlate command)
    {
        var weeklyParkingSpot = await GetWeeklyParkingSpotByReservationAsync(command.ReservationId);
        if (weeklyParkingSpot is null)
            return false;

        var parkingSpotId = new ReservationId(command.ReservationId);
        var exisitngReservation = weeklyParkingSpot.Reservations
            .OfType<VehicleReservation>()
            .SingleOrDefault(x => x.Id == parkingSpotId);
        if (exisitngReservation is null)
            return false;

        if (exisitngReservation.Date.Value.Date < _clock.Current().Date)
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

    private async Task<WeeklyParkingSpot?> GetWeeklyParkingSpotByReservationAsync(ReservationId reservationId)
    {
        var weeklyParkingSpots = await _weeklyParkingSpotsRepository.GetAllAsync();
        
        return weeklyParkingSpots.SingleOrDefault(x => x.Reservations.Any(r =>r.Id == reservationId));
    }
}

