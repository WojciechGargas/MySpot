using MySpot.Application.Abstractions;
using MySpot.Application.Exceptions;
using MySpot.Core.DamainServices;
using MySpot.Core.Entities;
using MySpot.Core.Repositories;
using MySpot.Core.ValueObjects;

namespace MySpot.Application.Commands.Handlers;

internal sealed class DeleteReservationHandler : ICommandHandler<DeleteReservation>
{
    private readonly IWeeklyParkingSpotRepository _repository;
    private readonly IReservationsRepository _reservationsRepository;

    public DeleteReservationHandler(IWeeklyParkingSpotRepository  repository,
        IReservationsRepository reservationsRepository)
    {
        _repository = repository;
        _reservationsRepository = reservationsRepository;
    }
    
    public async Task HandleAsync(DeleteReservation command)
    {
        var reservationId = new ReservationId(command.ReservationId);
        var exisitngReservation = await _reservationsRepository.GetAsync(reservationId);
        if (exisitngReservation is null)
        {
            throw new WeeklyParkingSpotNotFoundException();
        }

        await _reservationsRepository.DeleteAsync(exisitngReservation);
    }
}