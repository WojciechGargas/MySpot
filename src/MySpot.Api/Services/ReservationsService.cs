using MySpot.Api.Commands;
using MySpot.Api.DTO;
using MySpot.Api.Entities;
using MySpot.Api.Exceptions;
using MySpot.Api.ValueObjects;

namespace MySpot.Api.Services;

public class ReservationsService : IReservationsService
{
    private readonly IClock _clock;
    private readonly IEnumerable<WeeklyParkingSpot> _weeklyParkingSpots;

    public ReservationsService(IClock clock, IEnumerable<WeeklyParkingSpot> weeklyParkingSpots)
    {
        _clock = clock;
        _weeklyParkingSpots = weeklyParkingSpots;
    }

    public ReservationDto Get(Guid id)
        => GetAllWeekly().SingleOrDefault(x => x.Id == id);

    public IEnumerable<ReservationDto> GetAllWeekly()
        => _weeklyParkingSpots.SelectMany(x => x.Reservations)
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
        var weeklyParkingSpot = _weeklyParkingSpots.SingleOrDefault(x => x.Id == parkingSpotId);
        if(weeklyParkingSpot is null)
            return null;
        
        var reservation = new Reservation(command.ReservationId, command.ParkingSpotId, command.EmployeeName,
        command.LicensePlate, new Date(command.Date));
        weeklyParkingSpot.AddReservation(reservation, new Date (_clock.Current()));

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
        
        return true;
    }

    public bool Delete(DeleteReservation command)
    {
        var weeklyParkingSpot = GetWeeklyParkingSpotByReservation(command.ReservationId);
        if (weeklyParkingSpot is null)
            return false;
        
        var reservationId = new ReservationId(command.ReservationId);
        var exisitngReservation = weeklyParkingSpot.Reservations.SingleOrDefault(x => x.Id == reservationId);
        if (exisitngReservation is null)
            throw new ReservationDoesNotExistException();
        
        weeklyParkingSpot.RemoveReservation(reservationId);
        
        return true;
    }
    
    private WeeklyParkingSpot GetWeeklyParkingSpotByReservation(ReservationId reservationId)
        =>_weeklyParkingSpots.SingleOrDefault(x => x.Reservations.Any(r =>r.Id == reservationId));
}
