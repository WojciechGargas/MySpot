using System.Net;
using System.Net.Http.Json;
using MySpot.Application.Commands;
using MySpot.Application.DTO;
using MySpot.Core.ValueObjects;
using MySpot.Tests.Integration.Infrastructure;

namespace MySpot.Tests.Integration.Controllers;

public class ParkingSpotControllerTests : IClassFixture<ApplicationWebFactory>, IAsyncLifetime
{
    private static readonly Guid ParkingSpotId1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid ParkingSpotId2 = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly Guid ParkingSpotId3 = Guid.Parse("00000000-0000-0000-0000-000000000003");

    private readonly ApplicationWebFactory _factory;
    private HttpClient _backend = null!;
    private TestClock _clock = null!;

    public ParkingSpotControllerTests(ApplicationWebFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _clock = _factory.Clock;
        _clock.CurrentTime = new DateTime(2022, 8, 10, 5, 0, 0, DateTimeKind.Utc);
        await _factory.InitializeAsync();
        _backend = _factory.CreateClient();
    }

    public Task DisposeAsync()
    {
        _backend.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _backend.GetAsync("parking-spots");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var parkingSpots = await response.Content.ReadFromJsonAsync<List<WeeklyParkingSpotDto>>();
        Assert.NotNull(parkingSpots);
        Assert.Equal(5, parkingSpots!.Count);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_ForEmptyWeek()
    {
        var futureDate = _clock.Current().AddDays(30).ToString("yyyy-MM-dd");
        var response = await _backend.GetAsync($"parking-spots?date={futureDate}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var parkingSpots = await response.Content.ReadFromJsonAsync<List<WeeklyParkingSpotDto>>();
        Assert.NotNull(parkingSpots);
        Assert.Empty(parkingSpots!);
    }

    [Fact]
    public async Task PostVehicle_CreatesReservation_AndIsVisibleInWeeklySpots()
    {
        var reservationId = await CreateReservationAsync(ParkingSpotId1, _clock.Current(), "John Doe", "XYZ123");

        var parkingSpots = await GetWeeklyParkingSpotsAsync(_clock.Current());
        var spot = parkingSpots.Single(x => Guid.Parse(x.Id) == ParkingSpotId1);
        var reservation = spot.Reservations.Single(r => r.Id == reservationId);

        Assert.Equal(ParkingSpotId1, reservation.ParkingSpotId);
        Assert.Equal("John Doe", reservation.EmployeeName);

        await DeleteReservationAsync(reservationId);
    }

    [Fact]
    public async Task PostVehicle_ReturnsBadRequest_ForUnknownParkingSpot()
    {
        var command = new ReserveParkingSpotForVehicle(
            Guid.NewGuid(),
            Guid.Empty,
            ParkingSpotCapacityValue.Full,
            _clock.Current(),
            "Employee",
            "ABC123");

        var response = await _backend.PostAsJsonAsync($"parking-spots/{Guid.NewGuid()}/reservations/vehicle", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutVehicle_ReturnsNoContent_ForExistingReservation()
    {
        var reservationId = await CreateReservationAsync(ParkingSpotId2, _clock.Current(), "Alice", "XYZ987");
        try
        {
            var response = await UpdateLicensePlateAsync(reservationId, "XYZ987");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
        finally
        {
            await DeleteReservationAsync(reservationId);
        }
    }

    [Fact]
    public async Task PutVehicle_ReturnsNotFound_ForMissingReservation()
    {
        var response = await UpdateLicensePlateAsync(Guid.NewGuid(), "XYZ987");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteVehicle_ReturnsNoContent_ForExistingReservation()
    {
        var reservationId = await CreateReservationAsync(ParkingSpotId3, _clock.Current(), "Bob", "XYZ222");

        var response = await DeleteReservationAsync(reservationId);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteVehicle_ReturnsNotFound_ForMissingReservation()
    {
        var response = await DeleteReservationAsync(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostCleaning_ReturnsOk_AndCreatesCleaningReservations()
    {
        var command = new ReserveParkingSpotForCleaning(_clock.Current());

        var response = await _backend.PostAsJsonAsync("parking-spots/reservations/cleaning", command);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var parkingSpots = await GetWeeklyParkingSpotsAsync(_clock.Current());

        Assert.NotNull(parkingSpots);
        Assert.Equal(5, parkingSpots.Count);
        Assert.All(parkingSpots, spot => Assert.Single(spot.Reservations));
        Assert.All(parkingSpots.SelectMany(s => s.Reservations),
            r => Assert.True(string.IsNullOrWhiteSpace(r.EmployeeName)));
        Assert.All(parkingSpots.SelectMany(s => s.Reservations),
            r => Assert.Equal(_clock.Current().Date, r.Date));
    }

    private async Task<Guid> CreateReservationAsync(Guid parkingSpotId, DateTime date, string employeeName, string licensePlate)
    {
        var command = new ReserveParkingSpotForVehicle(
            parkingSpotId,
            Guid.Empty,
            ParkingSpotCapacityValue.Full,
            date,
            employeeName,
            licensePlate);

        var response = await _backend.PostAsJsonAsync($"parking-spots/{parkingSpotId}/reservations/vehicle", command);
        if (response.StatusCode != HttpStatusCode.NoContent)
        {
            var body = await response.Content.ReadAsStringAsync();
            Assert.Fail($"Expected NoContent but got {response.StatusCode}. Body: {body}");
        }

        var parkingSpots = await GetWeeklyParkingSpotsAsync(date);
        var spot = parkingSpots.Single(x => Guid.Parse(x.Id) == parkingSpotId);
        var reservation = spot.Reservations.Single(r =>
            r.EmployeeName == employeeName &&
            r.Date == date.Date);
        return reservation.Id;
    }

    private Task<HttpResponseMessage> UpdateLicensePlateAsync(Guid reservationId, string licensePlate)
    {
        var command = new ChangeReservationLicensePlate(reservationId, licensePlate);

        return _backend.PutAsJsonAsync($"parking-spots/reservations/{reservationId}", command);
    }

    private Task<HttpResponseMessage> DeleteReservationAsync(Guid reservationId)
    {
        return _backend.DeleteAsync($"parking-spots/reservations/{reservationId}");
    }

    private async Task<List<WeeklyParkingSpotDto>> GetWeeklyParkingSpotsAsync(DateTime? date = null)
    {
        var path = "parking-spots";
        if (date.HasValue)
        {
            path += $"?date={date.Value:yyyy-MM-dd}";
        }

        var response = await _backend.GetAsync(path);
        response.EnsureSuccessStatusCode();
        var parkingSpots = await response.Content.ReadFromJsonAsync<List<WeeklyParkingSpotDto>>();
        return parkingSpots ?? new List<WeeklyParkingSpotDto>();
    }

}
