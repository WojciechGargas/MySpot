using MySpot.Api.Models;

namespace MySpot.Api.Services;

public class ReservationsService
{
    private static int _id = 1;
    private static readonly List<Reservation> Reservations = new();

    private static readonly List<string> ParkingSpotNames = new()
    {
        "P1", "P2", "P3", "P4", "P5"
    };

    public Reservation Get(int id) 
        => Reservations.SingleOrDefault(x => x.Id == id);
    
    public IEnumerable<Reservation> GetAll()
        => Reservations;

    public int? Create(Reservation reservation)
    {
        var now = DateTime.UtcNow.Date;
        var pastDays = now.DayOfWeek is DayOfWeek.Sunday ? 7 : (int) now.DayOfWeek;
        var remainingDays = 7 - pastDays;
        
        if(!(reservation.Date.Date >= now.Date && reservation.Date.Date <= now.Date.AddDays(remainingDays)))
        {
            return null;
        }
        
        if (ParkingSpotNames.All(x => x != reservation.ParkingSpotName))
        {
            return null;
        }
        
        var reservationAlreadyExists = Reservations.Any(x => 
            x.ParkingSpotName == reservation.ParkingSpotName
            && x.Date.Date == reservation.Date.Date );

        if (reservationAlreadyExists)
        {
            return null;
        }
        
        reservation.Id = _id;
        _id++;
        Reservations.Add(reservation);

        return reservation.Id;
    }

    public bool Update(Reservation reservation)
    {
        var exisitngReservation = Reservations.SingleOrDefault(x => x.Id == reservation.Id);
        if (exisitngReservation == null)
            return false;

        if (exisitngReservation.Date <= reservation.Date)
            return false;

        exisitngReservation.LicensePlate = reservation.LicensePlate;
        return true;
    }

    public bool Delete(int id)
    {
        var exisitngReservation = Reservations.SingleOrDefault(x => x.Id == id);
        if (exisitngReservation == null)
            return false;

        Reservations.Remove(exisitngReservation);
        return true;
    }
}