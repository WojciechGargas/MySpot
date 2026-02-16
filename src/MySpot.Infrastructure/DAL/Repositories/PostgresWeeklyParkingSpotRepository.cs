using Microsoft.EntityFrameworkCore;
using MySpot.Core.Entities;
using MySpot.Core.Repositories;
using MySpot.Core.ValueObjects;

namespace MySpot.Infrastructure.DAL.Repositories;

internal sealed class PostgresWeeklyParkingSpotRepository : IWeeklyParkingSpotRepository
{
    private readonly MySpotDbContext _dbContext;

    public PostgresWeeklyParkingSpotRepository(MySpotDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public Task<WeeklyParkingSpot?> GetAsync(ParkingSpotId id) 
        => _dbContext.WeeklyParkingSpots
            .Include(x => x.Reservations)
            .SingleOrDefaultAsync(x => x.Id == id);

    public async Task<IEnumerable<WeeklyParkingSpot>> GetByWeekAsync(Week week)
    {
        return await _dbContext.WeeklyParkingSpots
            .Include(spot => spot.Reservations)
            .Where(x => x.Week == week)
            .ToListAsync();
    }


    public async Task<IEnumerable<WeeklyParkingSpot>> GetAllAsync()
    {
        var result = await _dbContext.WeeklyParkingSpots
            .Include(x => x.Reservations)
            .ToListAsync();
        
        return result.AsEnumerable();
    }

    public Task AddAsync(WeeklyParkingSpot weeklyParkingSpot)
    {
         _dbContext.WeeklyParkingSpots.AddAsync(weeklyParkingSpot);
         return Task.CompletedTask;
    }

    public Task UpdateAsync(WeeklyParkingSpot weeklyParkingSpot)
    { 
        _dbContext.WeeklyParkingSpots.Update(weeklyParkingSpot);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(WeeklyParkingSpot weeklyParkingSpot)
    {
        _dbContext.WeeklyParkingSpots.Remove(weeklyParkingSpot);
        return Task.CompletedTask;
    }
}
