using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySpot.Core.Repositories;
using MySpot.Infrastructure.DAL.Repositories;

namespace MySpot.Infrastructure.DAL;

internal static class Extensions
{
    private const string SectionName = "postgres";
    public static IServiceCollection AddPostgres(this IServiceCollection services, IConfiguration configuration)
    {
        var section =  configuration.GetSection(SectionName);
        services.Configure<PostgresOptions>(section);
        var options = configuration.GetOptions<PostgresOptions>(SectionName);
        
        services.AddDbContext<MySpotDbContext>(x => x.UseNpgsql(options.ConnectionString));
        services.AddScoped<IWeeklyParkingSpotRepository, PostgresWeeklyParkingSpotRepository>();
        services.AddScoped<IReservationsRepository, ReservationRepository>();
        services.AddHostedService<DataBaseInitializer>();
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        
        return services;
    }

    public static T GetOptions<T>(this IConfiguration configuration, string sectionName) where T : class, new()
    {
        var options = new T();
        var section = configuration.GetSection(sectionName);
        section.Bind(options);
        
        return options;
    }
}
