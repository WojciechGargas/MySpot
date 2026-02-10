using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySpot.Core.Repositories;
using MySpot.Infrastructure.DAL.Repositories;

namespace MySpot.Infrastructure.DAL;

internal static class Extensions
{
    public static IServiceCollection AddPostgres(this IServiceCollection services)
    {
        const string connectionString = "Host=localhost;Port=5433;Database=postgres;Username=postgres;Password=postgres";
        services.AddDbContext<MySpotDbContext>(x => x.UseNpgsql(connectionString));
        services.AddScoped<IWeeklyParkingSpotRepository, PostgresWeeklyParkingSpotRepository>();
        services.AddScoped<IReservationsRepository, ReservationRepository>();
        services.AddHostedService<DataBaseInitializer>();
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        
        return services;
    }
}
