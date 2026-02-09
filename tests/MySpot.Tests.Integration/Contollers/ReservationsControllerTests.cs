using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MySpot.Application.Commands;
using MySpot.Tests.Integration.Factories;

namespace MySpot.Tests.Integration.Contollers;

public class ReservationsControllerTests 
    : IClassFixture<MySpotApplicationFactory>
{
    private readonly HttpClient _client;

    public ReservationsControllerTests(MySpotApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_ValidReservation_ShouldReturnCreated()
    {
        var command = new CreateReservation(
            ParkingSpotId: Guid.Parse("00000000-0000-0000-0000-000000000001"),
            ReservationId: Guid.NewGuid(),
            Date: MySpotApplicationFactory.FixedNow.AddDays(1),
            EmployeeName: "John Doe",
            LicensePlate: "XYZ123"
        );
        
        var response = await _client.PostAsJsonAsync(
            "/reservations", command);
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
    
    [Fact]
    public async Task Post_ReservationOnUnexistingSpot_ShouldReturnBadRequest()
    {
        var command = new CreateReservation(
            ParkingSpotId: Guid.NewGuid(),
            ReservationId: Guid.NewGuid(),
            Date: MySpotApplicationFactory.FixedNow.AddDays(1),
            EmployeeName: "John Doe",
            LicensePlate: "XYZ123"
        );
        
        var response = await _client.PostAsJsonAsync(
            "/reservations", command);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_ReservationInThePast_ShouldReturnBadRequest()
    {
        var command = new CreateReservation(
            ParkingSpotId: Guid.Parse("00000000-0000-0000-0000-000000000001"),
            ReservationId: Guid.NewGuid(),
            Date: MySpotApplicationFactory.FixedNow.AddDays(-1),
            EmployeeName: "John Doe",
            LicensePlate: "XYZ123"
        );

        var response = await _client.PostAsJsonAsync(
            "/reservations", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
