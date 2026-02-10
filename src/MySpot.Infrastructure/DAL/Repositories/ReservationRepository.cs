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

    public async Task<Reservation?> GetAsync(ReservationId id)
        => await _dbContext.Reservations
            .SingleOrDefaultAsync(x => x.Id == id);

    public async Task<IEnumerable<Reservation>> GetAllAsync()
        => await _dbContext.Reservations.ToListAsync();

    public async Task<IEnumerable<Reservation>> GetByParkingSpotAsync(ParkingSpotId parkingSpotId)
        => await _dbContext.Reservations
            .Where(x => x.ParkingSpotId == parkingSpotId)
            .ToListAsync();

    public async Task<IEnumerable<Reservation>> GetByWeekAsync(Week week)
        => await _dbContext.Reservations
            .Where(x => x.Date >= week.From && x.Date <= week.To)
            .ToListAsync();

    public async Task AddAsync(Reservation reservation)
    {
        await _dbContext.Reservations.AddAsync(reservation);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Reservation reservation)
    {
        _dbContext.Reservations.Update(reservation);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Reservation reservation)
    {
        _dbContext.Reservations.Remove(reservation);
        await _dbContext.SaveChangesAsync();
    }
}
