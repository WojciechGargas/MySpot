using System.Net;
using System.Net.Http.Json;
using MySpot.Application.Commands;
using MySpot.Application.DTO;
using MySpot.Tests.Integration.Infrastructure;

namespace MySpot.Tests.Integration.Controllers;

public class ReservationsControllerTests : IClassFixture<ApplicationWebFactory>, IAsyncLifetime
{
    private static readonly Guid ParkingSpotId1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid ParkingSpotId2 = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly Guid ParkingSpotId3 = Guid.Parse("00000000-0000-0000-0000-000000000003");

    private readonly ApplicationWebFactory _factory;
    private HttpClient _backend = null!;
    private TestClock _clock = null!;

    public ReservationsControllerTests(ApplicationWebFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _clock = _factory.Clock;
        _clock.CurrentTime = new DateTime(2022, 8, 10);
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
        var response = await _backend.GetAsync("reservations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var reservations = await response.Content.ReadFromJsonAsync<List<ReservationDto>>();
        Assert.NotNull(reservations);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_ForMissingReservation()
    {
        var response = await _backend.GetAsync($"reservations/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_CreatesReservation_AndGetByIdReturnsOk()
    {
        var reservationId = await CreateReservationAsync(ParkingSpotId1, _clock.Current());
        try
        {
            var response = await _backend.GetAsync($"reservations/{reservationId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var reservation = await response.Content.ReadFromJsonAsync<ReservationDto>();
            Assert.NotNull(reservation);
            Assert.Equal(reservationId, reservation!.Id);
        }
        finally
        {
            await DeleteReservationAsync(reservationId);
        }
    }

    [Fact]
    public async Task Post_ReturnsBadRequest_ForUnknownParkingSpot()
    {
        var command = new CreateReservation(
            Guid.NewGuid(),
            Guid.Empty,
            _clock.Current(),
            "Employee",
            "ABC123");

        var response = await _backend.PostAsJsonAsync("reservations", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_ReturnsNoContent_ForExistingReservation()
    {
        var reservationId = await CreateReservationAsync(ParkingSpotId2, _clock.Current());
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
    public async Task Put_ReturnsNotFound_ForMissingReservation()
    {
        var response = await UpdateLicensePlateAsync(Guid.NewGuid(), "XYZ987");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_ForExistingReservation()
    {
        var reservationId = await CreateReservationAsync(ParkingSpotId3, _clock.Current());

        var response = await DeleteReservationAsync(reservationId);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_ForMissingReservation()
    {
        var response = await DeleteReservationAsync(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<Guid> CreateReservationAsync(Guid parkingSpotId, DateTime date)
    {
        var command = new CreateReservation(
            parkingSpotId,
            Guid.Empty,
            date,
            "Employee",
            "ABC123");

        var response = await _backend.PostAsJsonAsync("reservations", command);
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var body = await response.Content.ReadAsStringAsync();
            Assert.Fail($"Expected Created but got {response.StatusCode}. Body: {body}");
        }

        var location = response.Headers.Location?.ToString();
        Assert.False(string.IsNullOrWhiteSpace(location));
        var idSegment = location!.Split('/')[^1];
        return Guid.Parse(idSegment);
    }

    private Task<HttpResponseMessage> UpdateLicensePlateAsync(Guid reservationId, string licensePlate)
    {
        var command = new ChangeReservationLicensePlate(reservationId, licensePlate);

        return _backend.PutAsJsonAsync($"reservations/{reservationId}", command);
    }

    private Task<HttpResponseMessage> DeleteReservationAsync(Guid reservationId)
        => _backend.DeleteAsync($"reservations/{reservationId}");
}
