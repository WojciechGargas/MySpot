using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MySpot.Application.Abstractions;
using MySpot.Infrastructure.DAL.Decorators;
using Serilog;

namespace MySpot.Infrastructure.DAL.Logging.Decorators;

public static class Extensions
{
    internal static IServiceCollection AddCustomLogging(this IServiceCollection services)
    {
        services.TryDecorate(typeof(ICommandHandler<>), typeof(LoggingCommandHandlerDecorator<>));
        
        return services;
    }

    public static WebApplicationBuilder UseSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration
                .WriteTo
                .Console()
                .WriteTo
                .File("logs/logs.txt")
                .WriteTo
                .Seq("http://localhost:5341");
        });
        
        return builder;
    }
}