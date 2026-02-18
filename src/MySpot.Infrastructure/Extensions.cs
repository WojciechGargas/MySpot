using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySpot.Application.Abstractions;
using MySpot.Application.DTO;
using MySpot.Application.Queries;
using MySpot.Core.Abstractions;
using MySpot.Core.Repositories;
using MySpot.Infrastructure.Auth;
using MySpot.Infrastructure.DAL;
using MySpot.Infrastructure.DAL.Handlers;
using MySpot.Infrastructure.DAL.Logging.Decorators;
using MySpot.Infrastructure.DAL.Repositories;
using MySpot.Infrastructure.Exceptions;
using MySpot.Infrastructure.Security;
using MySpot.Infrastructure.Time;

namespace MySpot.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("app");
        
        var infrastructureAssembly = typeof(AppOptions).Assembly;
        
        services.Configure<AppOptions>(section)
            .AddSingleton<ExceptionMiddleware>()
            .AddSecurity()
            .AddAuth(configuration)
            .AddCustomLogging()
            .AddPostgres(configuration)
            .AddSingleton<IClock, Clock>()
            .Scan(s => s.FromAssemblies(infrastructureAssembly)
            .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime())
            .AddHttpContextAccessor();


        services.AddScoped<IQueryHandler<GetWeeklyParkingSpots, IEnumerable<WeeklyParkingSpotDto>>, GetWeeklyParkingSpotsHandler>();
        
        return services;
    }

    public static WebApplication UseInfrastructure(this WebApplication app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        
        return app;
    }
    
    public static T GetOptions<T>(this IConfiguration configuration, string sectionName) where T : class, new()
    {
        var options = new T();
        var section = configuration.GetSection(sectionName);
        section.Bind(options);
        
        return options;
    }
}
