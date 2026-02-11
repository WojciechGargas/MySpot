using MySpot.Core.Abstractions;

namespace MySpot.Tests.Integration.Infrastructure;

public sealed class TestClock : IClock
{
    public DateTime CurrentTime { get; set; } = new(2022, 8, 10, 5, 0, 0, DateTimeKind.Utc);

    public DateTime Current() => CurrentTime;
}

