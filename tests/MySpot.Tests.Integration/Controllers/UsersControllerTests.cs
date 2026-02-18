using System.Net;
using System.Net.Http.Json;
using MySpot.Application.Commands;
using MySpot.Application.DTO;
using MySpot.Tests.Integration.Infrastructure;

namespace MySpot.Tests.Integration.Controllers;

public class UsersControllerTests : IClassFixture<ApplicationWebFactory>, IAsyncLifetime
{
    private readonly ApplicationWebFactory _factory;
    private HttpClient _backend = null!;
    private TestClock _clock = null!;

    public UsersControllerTests(ApplicationWebFactory factory)
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
    public async Task Post_CreatesUser_AndGetByIdReturnsIt()
    {
        var created = await UsersApiHelper.SignUpAsync(_backend, "Jane Doe");

        var response = await _backend.GetAsync($"users/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(user);
        Assert.Equal(created.Id, user!.Id);
        Assert.Equal(created.Username, user.Username);
        Assert.Equal(created.FullName, user.FullName);
    }

    [Fact]
    public async Task SignIn_ReturnsJwt_ForValidCredentials()
    {
        var created = await UsersApiHelper.SignUpAsync(_backend, "John Doe", password: "secret123");
        var command = new SignIn(created.Email, created.Password);

        var response = await _backend.PostAsJsonAsync("users/sign-in", command);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var jwt = await response.Content.ReadFromJsonAsync<JwtDto>();
        Assert.NotNull(jwt);
        Assert.False(string.IsNullOrWhiteSpace(jwt!.AccessToken));
    }

    [Fact]
    public async Task SignIn_ReturnsBadRequest_ForInvalidCredentials()
    {
        var created = await UsersApiHelper.SignUpAsync(_backend, "John Doe", password: "secret123");
        var command = new SignIn(created.Email, "invalid-password");

        var response = await _backend.PostAsJsonAsync("users/sign-in", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
