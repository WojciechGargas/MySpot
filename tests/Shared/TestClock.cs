using MySpot.Core.Abstractions;

namespace MySpot.Tests.Shared;

public sealed class TestClock : IClock
{
    public static readonly DateTime FixedNow = new(2026, 1, 1, 5, 0, 0, DateTimeKind.Utc);

    public DateTime Current() => FixedNow;
}
