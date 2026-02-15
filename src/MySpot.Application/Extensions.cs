using Microsoft.Extensions.DependencyInjection;
using MySpot.Application.Abstractions;
using MySpot.Application.Commands;
using MySpot.Application.Commands.Handlers;

namespace MySpot.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<ReserveParkingSpotForVehicle>, ReserveParkingSpotForVehicleHandler>();
        services.AddScoped<ICommandHandler<ReserveParkingSpotForCleaning>, ReserveParkingSpotForCleaningHandler>();
        services.AddScoped<ICommandHandler<ChangeReservationLicensePlate>, ChangeReservationLicensePlateHandler>();
        services.AddScoped<ICommandHandler<DeleteReservation>, DeleteReservationHandler>();

        return services;
    }
}
