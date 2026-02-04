using MySpot.Api.Commands;
using MySpot.Api.DTO;
using MySpot.Api.Entities;

namespace MySpot.Api.Services;

public class ReservationsService
{
    private static readonly List<WeeklyParkingSpot> WeeklyParkingSpots = new()
    {
        new WeeklyParkingSpot(Guid.Parse("00000000-0000-0000-0000-000000000001"), DateTime.UtcNow, DateTime.UtcNow.AddDays(7), name:"P1" ),
        new WeeklyParkingSpot(Guid.Parse("00000000-0000-0000-0000-000000000002"), DateTime.UtcNow, DateTime.UtcNow.AddDays(7), name:"P2" ),
        new WeeklyParkingSpot(Guid.Parse("00000000-0000-0000-0000-000000000003"), DateTime.UtcNow, DateTime.UtcNow.AddDays(7), name:"P3" ),
        new WeeklyParkingSpot(Guid.Parse("00000000-0000-0000-0000-000000000004"), DateTime.UtcNow, DateTime.UtcNow.AddDays(7), name:"P4" ),
        new WeeklyParkingSpot(Guid.Parse("00000000-0000-0000-0000-000000000005"), DateTime.UtcNow, DateTime.UtcNow.AddDays(7), name:"P5" ),
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
                Date = x.Date,
            });

    public Guid? Create(CreateReservation command)
    {
        var weeklyParkingSpot = WeeklyParkingSpots.SingleOrDefault(x => x.Id == command.ParkingSpotId);
        if(weeklyParkingSpot is null)
            return null;
        
        var reservation = new Reservation(command.ReservationId, command.ParkingSpotId, command.EmployeeName,
        command.LicensePlate, command.Date);
        weeklyParkingSpot.AddReservation(reservation);

        return reservation.Id;
    }

    public bool Update(ChangeReservationLicensePlate command)
    {
        var weeklyParkingSpot = GetWeeklyParkingSpotByReservation(command.ReservationId);
        if (weeklyParkingSpot is null)
            return false;
        
        var exisitngReservation = weeklyParkingSpot.Reservations.SingleOrDefault(x => x.Id == command.ReservationId);
        if (exisitngReservation is null)
            return false;

        if (exisitngReservation.Date < DateTime.UtcNow)
            return false;

        exisitngReservation.ChangeLicensePlate(command.LicensePlate);
        
        return true;
    }

    public bool Delete(DeleteReservation command)
    {
        var weeklyParkingSpot = GetWeeklyParkingSpotByReservation(command.ReservationId);
        if (weeklyParkingSpot is null)
            return false;
        
        var exisitngReservation = weeklyParkingSpot.Reservations.SingleOrDefault(x => x.Id == command.ReservationId);
        if (exisitngReservation is null)
            return false;
        
        weeklyParkingSpot.RemoveReservation(command.ReservationId);
        
        return true;
    }
    
    private WeeklyParkingSpot GetWeeklyParkingSpotByReservation(Guid reservationId)
        =>WeeklyParkingSpots.SingleOrDefault(x => x.Reservations.Any(r =>r.Id == reservationId));
}