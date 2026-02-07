using MySpot.Api.Commands;
using MySpot.Api.DTO;
using MySpot.Api.Entities;
using MySpot.Api.Exceptions;
using MySpot.Api.ValueObjects;

namespace MySpot.Api.Services;

public class ReservationsService
{
    private static Clock Clock = new ();
    private static readonly List<WeeklyParkingSpot> WeeklyParkingSpots = new()
    {
        new WeeklyParkingSpot(Guid.Parse("00000000-0000-0000-0000-000000000001"), new Week(Clock.Current()), name:"P1" ),
        new WeeklyParkingSpot(Guid.Parse("00000000-0000-0000-0000-000000000002"), new Week(Clock.Current()), name:"P2" ),
        new WeeklyParkingSpot(Guid.Parse("00000000-0000-0000-0000-000000000003"), new Week(Clock.Current()), name:"P3" ),
        new WeeklyParkingSpot(Guid.Parse("00000000-0000-0000-0000-000000000004"), new Week(Clock.Current()), name:"P4" ),
        new WeeklyParkingSpot(Guid.Parse("00000000-0000-0000-0000-000000000005"), new Week(Clock.Current()), name:"P5" ),
    };

    public ReservationDto Get(Guid id)
        => GetAllWeekly().SingleOrDefault(x => x.Id == id);

    public IEnumerable<ReservationDto> GetAllWeekly()
        => WeeklyParkingSpots.SelectMany(x => x.Reservations)
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
        var weeklyParkingSpot = WeeklyParkingSpots.SingleOrDefault(x => x.Id == parkingSpotId);
        if(weeklyParkingSpot is null)
            return null;
        
        var reservation = new Reservation(command.ReservationId, command.ParkingSpotId, command.EmployeeName,
        command.LicensePlate, new Date(command.Date));
        weeklyParkingSpot.AddReservation(reservation, new Date (Clock.Current()));

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

        if (exisitngReservation.Date.Value.Date < Clock.Current())
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
        =>WeeklyParkingSpots.SingleOrDefault(x => x.Reservations.Any(r =>r.Id == reservationId));
}