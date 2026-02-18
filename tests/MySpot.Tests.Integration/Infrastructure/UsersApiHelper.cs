using System.Net;
using System.Net.Http.Json;
using MySpot.Application.Commands;
using MySpot.Application.DTO;

namespace MySpot.Tests.Integration.Infrastructure;

internal static class UsersApiHelper
{
    internal sealed record CreatedUser(Guid Id, string Email, string Username, string Password, string FullName);

    public static async Task<CreatedUser> SignUpAsync(HttpClient backend, string fullName, string? email = null,
        string? username = null, string password = "secret123")
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var uniqueUsername = username ?? $"user_{suffix}";
        var uniqueEmail = email ?? $"{uniqueUsername}@example.com";

        var command = new SignUp(
            Guid.Empty,
            uniqueEmail,
            uniqueUsername,
            password,
            fullName,
            "user");

        var response = await backend.PostAsJsonAsync("users", command);
        if (response.StatusCode != HttpStatusCode.NoContent)
        {
            var body = await response.Content.ReadAsStringAsync();
            Assert.Fail($"Expected NoContent from sign-up but got {response.StatusCode}. Body: {body}");
        }

        var users = await backend.GetFromJsonAsync<List<UserDto>>("users");
        var user = users?.SingleOrDefault(x => x.Username == uniqueUsername);
        Assert.NotNull(user);

        return new CreatedUser(user!.Id, uniqueEmail, uniqueUsername, password, fullName);
    }
}
