using MySpot.Application.Services;

namespace MySpot.Tests.Integration.Infrastructure;

public sealed class TestClock : IClock
{
    public DateTime CurrentTime { get; set; } = new(2022, 8, 10);

    public DateTime Current() => CurrentTime;
}
