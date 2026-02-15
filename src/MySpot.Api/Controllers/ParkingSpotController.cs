﻿using Microsoft.AspNetCore.Mvc;
 using MySpot.Application.Abstractions;
 using MySpot.Application.Commands;
using MySpot.Application.DTO;
using MySpot.Application.Queries;
using MySpot.Core.Exceptions;

namespace MySpot.Api.Controllers;

[ApiController]
[Route("parking-spots")]
public class ParkingSpotController(
    ICommandHandler<ReserveParkingSpotForVehicle> reserveParkingSpotForVehicleHandler,
    ICommandHandler<ReserveParkingSpotForCleaning> reserveParkingSpotForCleaningHandler,
    ICommandHandler<ChangeReservationLicensePlate> changeReservationLicensePlateHandler,
    ICommandHandler<DeleteReservation> deleteReservationHandler,
    IQueryHandler<GetWeeklyParkingSpots, IEnumerable<WeeklyParkingSpotDto>> weeklyParkingSpotQueryHandler)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WeeklyParkingSpotDto>>> Get([FromQuery] GetWeeklyParkingSpots query)
        => Ok(await weeklyParkingSpotQueryHandler.HandleAsync(query));
    
    [HttpPost("{parkingSpotId:guid}/reservations/vehicle")]
    public async Task<ActionResult> Post(Guid parkingSpotId, ReserveParkingSpotForVehicle command)
    {
        await reserveParkingSpotForVehicleHandler.HandleAsync(command with
        {
            ReservationId = Guid.NewGuid(),
            ParkingSpotId = parkingSpotId
        });
        return NoContent();
    }

    [HttpPost("reservations/cleaning")]
    public async Task<ActionResult> Post(ReserveParkingSpotForCleaning command)
    {
        await reserveParkingSpotForCleaningHandler.HandleAsync(command);
        return NoContent();
    }

    [HttpPut("reservations/{reservationId:guid}")]
    public async Task<ActionResult> Put(Guid reservationId, ChangeReservationLicensePlate command)
    {
        await changeReservationLicensePlateHandler.HandleAsync(command with {ReservationId = reservationId});
        return NoContent();
    }

    [HttpDelete("reservations/{reservationId:guid}")]
    public async Task<ActionResult> Delete(Guid reservationId)
    {
        await deleteReservationHandler.HandleAsync(new DeleteReservation(reservationId));
        return NoContent();
    }
}
 
