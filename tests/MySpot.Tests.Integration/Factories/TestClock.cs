using MySpot.Api.Services;

namespace MySpot.Tests.Integration.Factories;

public sealed class TestClock : IClock
{
    private readonly DateTime _current;

    public TestClock(DateTime current)
    {
        _current = DateTime.SpecifyKind(current, DateTimeKind.Utc);
    }

    public DateTime Current() => _current;
}
