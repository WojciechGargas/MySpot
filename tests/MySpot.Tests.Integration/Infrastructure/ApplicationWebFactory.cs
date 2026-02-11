using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MySpot.Api;
using MySpot.Core.Abstractions;
using MySpot.Core.Entities;
using MySpot.Core.ValueObjects;
using MySpot.Infrastructure.DAL;
using Testcontainers.PostgreSql;

namespace MySpot.Tests.Integration.Infrastructure;

public sealed class ApplicationWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:17")
        .WithDatabase("myspot_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private string? _connectionString;

    public TestClock Clock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IClock>();
            services.AddSingleton<IClock>(Clock);
        });
        builder.ConfigureTestServices(testServices =>
        {
            var descriptor =
                testServices.SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<MySpotDbContext>));

            if (descriptor is not null)
            {
                testServices.Remove(descriptor);
            }

            testServices.AddDbContext<MySpotDbContext>(options =>
            {
                options.UseNpgsql(_connectionString ?? throw new InvalidOperationException("Test database was not initialized."));
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        _connectionString = _dbContainer.GetConnectionString();

        await ResetDatabaseAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    private async Task ResetDatabaseAsync()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var options = new DbContextOptionsBuilder<MySpotDbContext>()
            .UseNpgsql(_connectionString ?? throw new InvalidOperationException("Test database was not initialized."))
            .Options;

        await using var dbContext = new MySpotDbContext(options);
        await dbContext.Database.MigrateAsync();

        await dbContext.Reservations.ExecuteDeleteAsync();
        await dbContext.WeeklyParkingSpots.ExecuteDeleteAsync();

        var week = new Week(new DateTimeOffset(Clock.CurrentTime));
        var weeklyParkingSpots = new List<WeeklyParkingSpot>
        {
            new(Guid.Parse("00000000-0000-0000-0000-000000000001"), week, name: "P1"),
            new(Guid.Parse("00000000-0000-0000-0000-000000000002"), week, name: "P2"),
            new(Guid.Parse("00000000-0000-0000-0000-000000000003"), week, name: "P3"),
            new(Guid.Parse("00000000-0000-0000-0000-000000000004"), week, name: "P4"),
            new(Guid.Parse("00000000-0000-0000-0000-000000000005"), week, name: "P5"),
        };

        await dbContext.WeeklyParkingSpots.AddRangeAsync(weeklyParkingSpots);
        await dbContext.SaveChangesAsync();
    }
}

