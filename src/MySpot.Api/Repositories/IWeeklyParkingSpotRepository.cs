using MySpot.Api.Entities;
using MySpot.Api.Services;
using MySpot.Api.ValueObjects;

namespace MySpot.Api.Repositories;

public interface IWeeklyParkingSpotRepository
{
    WeeklyParkingSpot Get(ParkingSpotId id);
    IEnumerable<WeeklyParkingSpot> GetAll();
    void Add(WeeklyParkingSpot weeklyParkingSpot);
    void Update(WeeklyParkingSpot weeklyParkingSpot);
    void Delete(WeeklyParkingSpot weeklyParkingSpot);
}

public class InMemoryWeeklyParkingSpotRepository : IWeeklyParkingSpotRepository
{
    private readonly IClock _clock;
    private readonly List<WeeklyParkingSpot> _weeklyParkingSpots;

    public InMemoryWeeklyParkingSpotRepository(IClock clock)
    {
        _clock = clock;
        _weeklyParkingSpots = new List<WeeklyParkingSpot>()
        {
            new(Guid.Parse("00000000-0000-0000-0000-000000000001"), new Week(clock.Current()), name: "P1"),
            new(Guid.Parse("00000000-0000-0000-0000-000000000002"), new Week(clock.Current()), name: "P2"),
            new(Guid.Parse("00000000-0000-0000-0000-000000000003"), new Week(clock.Current()), name: "P3"),
            new(Guid.Parse("00000000-0000-0000-0000-000000000004"), new Week(clock.Current()), name: "P4"),
            new(Guid.Parse("00000000-0000-0000-0000-000000000005"), new Week(clock.Current()), name: "P5"),
        };
    }
    
    public WeeklyParkingSpot Get(ParkingSpotId id)
        => _weeklyParkingSpots.SingleOrDefault(x => x.Id == id);

    public IEnumerable<WeeklyParkingSpot> GetAll()
        => _weeklyParkingSpots;

    public void Add(WeeklyParkingSpot weeklyParkingSpot)
        => _weeklyParkingSpots.Add(weeklyParkingSpot);

    public void Update(WeeklyParkingSpot weeklyParkingSpot)
    {
    }

    public void Delete(WeeklyParkingSpot weeklyParkingSpot)
        => _weeklyParkingSpots.Remove(weeklyParkingSpot);
}