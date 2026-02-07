using MySpot.Api.Services;

namespace MySpot.Tests.Shared;

public sealed class TestClock : IClock
{
    public static readonly DateTime FixedNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public DateTime Current() => FixedNow;
}
