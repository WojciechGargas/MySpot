using MySpot.Core.Exceptions;
using MySpot.Core.ValueObjects;

namespace MySpot.Core.Entities;

public class WeeklyParkingSpot
{
    private readonly HashSet<Reservation> _reservations = new();
    
    public ParkingSpotId Id { get; private set;  }
    public Week Week { get; private set;  }
    public string Name { get; private set;  }
    public IEnumerable<Reservation> Reservations => _reservations;
    
    private WeeklyParkingSpot()
    {
    }

    public WeeklyParkingSpot(Guid id, Week week, string name)
    {
        Id = id;
        Week = week;
        Name = name;
    }

    internal void AddReservation(Reservation reservation, Date now)
    {
        var isInvalidDate = reservation.Date < Week.From || 
                            reservation.Date > Week.To ||
                            reservation.Date < now;
        if(isInvalidDate)
        {
            throw new InvalidReservationDateException(reservation.Date.Value.Date);
        }
        
        var reservationAlreadyExists = Reservations.Any(x => 
             x.Date == reservation.Date );
        if (reservationAlreadyExists)
        {
            throw new ParkingSpotAlreadyReservedException(Name, reservation.Date.Value.Date);
        }
        
        _reservations.Add(reservation);
    }

    public void RemoveReservation(ReservationId reservationId)
    {
        _reservations.RemoveWhere(x => x.Id == reservationId);
    }
}
