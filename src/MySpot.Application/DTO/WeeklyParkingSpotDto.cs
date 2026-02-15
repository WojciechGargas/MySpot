using MySpot.Core.ValueObjects;

namespace MySpot.Application.DTO;

public class WeeklyParkingSpotDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public ParkingSpotCapacity Capacity { get; set; }
    public IEnumerable<ReservationDto> Reservations { get; set; }
}