using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MySpot.Application.Commands;
using MySpot.Application.DTO;

namespace MySpot.Tests.Integration.Infrastructure;

internal static class UsersApiHelper
{
    internal sealed record CreatedUser(
        Guid Id,
        string Email,
        string Username,
        string Password,
        string FullName,
        string AccessToken);

    public static async Task<CreatedUser> SignUpAsync(HttpClient backend, string fullName, string? email = null,
        string? username = null, string password = "secret123", string role = "user")
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
            role);

        var signUpResponse = await backend.PostAsJsonAsync("users", command);
        if (signUpResponse.StatusCode != HttpStatusCode.NoContent)
        {
            var body = await signUpResponse.Content.ReadAsStringAsync();
            Assert.Fail($"Expected NoContent from sign-up but got {signUpResponse.StatusCode}. Body: {body}");
        }

        var signInResponse = await backend.PostAsJsonAsync("users/sign-in", new SignIn(uniqueEmail, password));
        if (signInResponse.StatusCode != HttpStatusCode.OK)
        {
            var body = await signInResponse.Content.ReadAsStringAsync();
            Assert.Fail($"Expected OK from sign-in but got {signInResponse.StatusCode}. Body: {body}");
        }

        var jwt = await signInResponse.Content.ReadFromJsonAsync<JwtDto>();
        Assert.NotNull(jwt);
        Assert.False(string.IsNullOrWhiteSpace(jwt!.AccessToken));

        using var meRequest = new HttpRequestMessage(HttpMethod.Get, "users/me");
        meRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt.AccessToken);
        var meResponse = await backend.SendAsync(meRequest);
        if (meResponse.StatusCode != HttpStatusCode.OK)
        {
            var body = await meResponse.Content.ReadAsStringAsync();
            Assert.Fail($"Expected OK from users/me but got {meResponse.StatusCode}. Body: {body}");
        }

        var user = await meResponse.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(user);

        return new CreatedUser(user!.Id, uniqueEmail, uniqueUsername, password, fullName, jwt.AccessToken);
    }
}
