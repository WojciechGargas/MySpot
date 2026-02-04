using MySpot.Api.Models;

namespace MySpot.Api.Services;

public class ReservationsService
{
    private static int _id = 1;
    private static readonly List<Reservation> _reservations = new();

    private static readonly List<string> _parkingSpotNames = new()
    {
        "P1", "P2", "P3", "P4", "P5"
    };

    public Reservation Get(int id) 
        => _reservations.SingleOrDefault(x => x.Id == id);
    
    public IEnumerable<Reservation> GetAll()
        => _reservations;

    public int? Create(Reservation reservation)
    {
        if (_parkingSpotNames.All(x => x != reservation.ParkingSpotName))
        {
            return null;
        }
        
        reservation.Date = DateTime.UtcNow.AddDays(1).Date;
        var reservationAlreadyExists = _reservations.Any(x => 
            x.ParkingSpotName == reservation.ParkingSpotName
            && x.Date.Date == reservation.Date.Date );

        if (reservationAlreadyExists)
        {
            return null;
        }
        
        reservation.Id = _id;
        _id++;
        _reservations.Add(reservation);

        return reservation.Id;
    }

    public bool Update( Reservation reservation)
    {
        var exisitngReservation = _reservations.SingleOrDefault(x => x.Id == reservation.Id);
        if (exisitngReservation == null)
            return false;


        exisitngReservation.LicensePlate = reservation.LicensePlate;
        return true;
    }

    public bool Delete(int id)
    {
        var exisitngReservation = _reservations.SingleOrDefault(x => x.Id == id);
        if (exisitngReservation == null)
            return false;

        _reservations.Remove(exisitngReservation);
        return true;
    }
}