using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySpot.Application.Abstractions;
using MySpot.Application.DTO;
using MySpot.Application.Queries;
using MySpot.Core.Abstractions;
using MySpot.Core.Repositories;
using MySpot.Infrastructure.DAL;
using MySpot.Infrastructure.DAL.Handlers;
using MySpot.Infrastructure.DAL.Repositories;
using MySpot.Infrastructure.Exceptions;
using MySpot.Infrastructure.Time;

namespace MySpot.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("app");
        services.Configure<AppOptions>(section);

        services.AddSingleton<ExceptionMiddleware>();
        
        services
            .AddPostgres(configuration)
            .AddSingleton<IClock, Clock>();

        services.AddScoped<IQueryHandler<GetWeeklyParkingSpots, IEnumerable<WeeklyParkingSpotDto>>, GetWeeklyParkingSpotsHandler>();
        
        return services;
    }

    public static WebApplication UseInfrastructure(this WebApplication app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
        app.MapControllers();
        
        return app;
    }
}
