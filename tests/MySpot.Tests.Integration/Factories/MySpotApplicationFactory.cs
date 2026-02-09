using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MySpot.Api;
using MySpot.Application.Services;
using MySpot.Core.Entities;
using MySpot.Core.ValueObjects;
using MySpot.Infrastructure.DAL;
using MySpot.Tests.Unit;
using Testcontainers.PostgreSql;

namespace MySpot.Tests.Integration.Factories;

public class MySpotApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .WithDatabase("myspot_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private string? _connectionString;
    
    public static DateTime FixedNow => TestClock.FixedNow;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IClock>();
            services.AddSingleton<IClock>(_ => new TestClock());
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
                options
                    .UseNpgsql(_connectionString ?? throw new InvalidOperationException("Test database was not initialized."));
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MySpotDbContext>();
        dbContext.Database.Migrate();

        dbContext.Reservations.ExecuteDelete();
        dbContext.WeeklyParkingSpots.ExecuteDelete();

        var week = new Week(new DateTimeOffset(FixedNow));
        var weeklyParkingSpots = new List<WeeklyParkingSpot>()
        {
            new(Guid.Parse("00000000-0000-0000-0000-000000000001"), week, name: "P1"),
            new(Guid.Parse("00000000-0000-0000-0000-000000000002"), week, name: "P2"),
            new(Guid.Parse("00000000-0000-0000-0000-000000000003"), week, name: "P3"),
            new(Guid.Parse("00000000-0000-0000-0000-000000000004"), week, name: "P4"),
            new(Guid.Parse("00000000-0000-0000-0000-000000000005"), week, name: "P5"),
        };
        dbContext.WeeklyParkingSpots.AddRange(weeklyParkingSpots);
        dbContext.SaveChanges();

        return host;
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        _connectionString = _dbContainer.GetConnectionString();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
