using Microsoft.EntityFrameworkCore;
using MySpot.Core.Entities;
using MySpot.Core.Repositories;
using MySpot.Core.ValueObjects;

namespace MySpot.Infrastructure.DAL.Repositories;

internal sealed class ReservationRepository : IReservationsRepository
{
    private readonly MySpotDbContext _dbContext;

    public ReservationRepository(MySpotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Reservation Get(ReservationId id)
        => _dbContext.Reservations.SingleOrDefault(x => x.Id == id);

    public IEnumerable<Reservation> GetAll()
        => _dbContext.Reservations.ToList();

    public IEnumerable<Reservation> GetByParkingSpot(ParkingSpotId parkingSpotId)
        => _dbContext.Reservations
            .Where(x => x.ParkingSpotId == parkingSpotId)
            .ToList();

    public IEnumerable<Reservation> GetByWeek(Week week)
        => _dbContext.Reservations
            .Where(x => x.Date >= week.From && x.Date <= week.To)
            .ToList();

    public void Add(Reservation reservation)
    {
        _dbContext.Reservations.Add(reservation);
        _dbContext.SaveChanges();
    }

    public void Update(Reservation reservation)
    {
        _dbContext.Reservations.Update(reservation);
        _dbContext.SaveChanges();
    }

    public void Delete(Reservation reservation)
    {
        _dbContext.Reservations.Remove(reservation);
        _dbContext.SaveChanges();
    }
}
