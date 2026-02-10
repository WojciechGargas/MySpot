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
    
    public WeeklyParkingSpot Get(ParkingSpotId id) 
        => _dbContext.WeeklyParkingSpots
            .Include(x => x.Reservations)
            .SingleOrDefault(x => x.Id == id);


    public IEnumerable<WeeklyParkingSpot> GetAll()
        => _dbContext.WeeklyParkingSpots
            .Include(x => x.Reservations)
            .ToList();

    public void Add(WeeklyParkingSpot weeklyParkingSpot)
    {
        _dbContext.WeeklyParkingSpots.Add(weeklyParkingSpot);
        _dbContext.SaveChanges();
    }

    public void Update(WeeklyParkingSpot weeklyParkingSpot)
    {
        _dbContext.WeeklyParkingSpots.Update(weeklyParkingSpot);
        _dbContext.SaveChanges();
    }

    public void Delete(WeeklyParkingSpot weeklyParkingSpot)
    {
        _dbContext.WeeklyParkingSpots.Remove(weeklyParkingSpot);
        _dbContext.SaveChanges();
    }
}