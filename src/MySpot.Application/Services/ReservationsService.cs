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

    public ReservationDto Get(Guid id)
        => GetAllWeekly().SingleOrDefault(x => x.Id == id);

    public IEnumerable<ReservationDto> GetAllWeekly()
        => _weeklyParkingSpotsRepository.GetAll().SelectMany(x => x.Reservations)
            .Select(x => new ReservationDto()
            {
                Id = x.Id,
                ParkingSpotId = x.ParkingSpotId,
                EmployeeName = x.EmployeeName,
                Date = x.Date.Value.Date,
            });

    public Guid? Create(CreateReservation command)
    {
        var parkingSpotId = new ParkingSpotId(command.ParkingSpotId);
        var weeklyParkingSpot = _weeklyParkingSpotsRepository.Get(parkingSpotId);
        if(weeklyParkingSpot is null)
            return null;
        
        var reservation = new Reservation(command.ReservationId, command.ParkingSpotId, command.EmployeeName,
        command.LicensePlate, new Date(command.Date));
        weeklyParkingSpot.AddReservation(reservation, new Date (_clock.Current()));
        _weeklyParkingSpotsRepository.Update(weeklyParkingSpot);
        
        return reservation.Id;
    }

    public bool Update(ChangeReservationLicensePlate command)
    {
        var weeklyParkingSpot = GetWeeklyParkingSpotByReservation(command.ReservationId);
        if (weeklyParkingSpot is null)
            return false;

        var parkingSpotId = new ReservationId(command.ReservationId);
        var exisitngReservation = weeklyParkingSpot.Reservations.SingleOrDefault(x => x.Id == parkingSpotId);
        if (exisitngReservation is null)
            return false;

        if (exisitngReservation.Date.Value.Date < _clock.Current())
            return false;

        exisitngReservation.ChangeLicensePlate(command.LicensePlate);
        _weeklyParkingSpotsRepository.Update(weeklyParkingSpot);
        
        return true;
    }

    public bool Delete(DeleteReservation command)
    {
        var reservationId = new ReservationId(command.ReservationId);
        var exisitngReservation = _reservationsRepository.Get(reservationId);
        if (exisitngReservation is null)
            return false;

        _reservationsRepository.Delete(exisitngReservation);
        return true;
    }
    
    private WeeklyParkingSpot GetWeeklyParkingSpotByReservation(ReservationId reservationId)
        =>_weeklyParkingSpotsRepository.GetAll().SingleOrDefault(x => x.Reservations.Any(r =>r.Id == reservationId));
}

