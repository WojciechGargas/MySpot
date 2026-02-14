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
    public ParkingSpotCapacity Capacity { get; private set;  }
    
    private WeeklyParkingSpot()
    {
    }

    private WeeklyParkingSpot(Guid id, Week week, string name, ParkingSpotCapacity capacity)
    {
        Id = id;
        Week = week;
        Name = name;
        Capacity = capacity;
    }

    public static WeeklyParkingSpot Create(Guid id, Week week, string name)
        => new(id, week, name, ParkingSpotCapacityValue.Full);

    internal void AddReservation(Reservation reservation, Date now)
    {
        var isInvalidDate = reservation.Date < Week.From || 
                            reservation.Date > Week.To ||
                            reservation.Date < now;
        if(isInvalidDate)
        {
            throw new InvalidReservationDateException(reservation.Date.Value.Date);
        }

        var dateCapacity = _reservations
            .Where(x => x.Date == reservation.Date)
            .Sum(x => (int)x.Capacity.Value);

        if (dateCapacity + (int)reservation.Capacity.Value > (int)ParkingSpotCapacityValue.Full)
        {
            throw new ParkingSpotCapacityExceededException(Id);
        }
        
        _reservations.Add(reservation);
    }

    public void RemoveReservation(ReservationId reservationId)
    {
        _reservations.RemoveWhere(x => x.Id == reservationId);
    }
    
    public void RemoveReservations(IEnumerable<Reservation> reservations)
    {
        var idsToRemove = reservations.Select(r => r.Id).ToHashSet();
        _reservations.RemoveWhere(x => idsToRemove.Contains(x.Id));
    }
}
