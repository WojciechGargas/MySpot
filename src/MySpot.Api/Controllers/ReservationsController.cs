using Microsoft.AspNetCore.Mvc;
using MySpot.Application.Commands;
using MySpot.Application.DTO;
using MySpot.Application.Services;
using MySpot.Core.Exceptions;

namespace MySpot.Api.Controllers;

[ApiController]
[Route("reservations")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationsService _reservationsService;
    public ReservationsController(IReservationsService reservationsService)
    {
        _reservationsService  = reservationsService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> Get()
    {
        var reservations = await _reservationsService.GetAllWeeklyAsync();
        return Ok(reservations);
    }

    [HttpGet("{id:Guid}")]
    public async Task<ActionResult<ReservationDto>> Get(Guid id)
    {
        var reservationDto = await _reservationsService.GetAsync(id);
        if (reservationDto == null)
            return NotFound();

        return Ok(reservationDto);
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] CreateReservation command)
    {
        try
        {
            var id = await _reservationsService.CreateAsync((command with {ReservationId =  Guid.NewGuid()}));
            if (id == null)
                return BadRequest();

            return CreatedAtAction(nameof(Get), new { id }, null);
        }
        catch (InvalidReservationDateException)
        {
            return BadRequest();
        }
    }

    [HttpPut("{id:Guid}")]
    public async Task<ActionResult> Put(Guid id, [FromBody] ChangeReservationLicensePlate command)
    {
        if(await _reservationsService.UpdateAsync(command with {ReservationId = id}))
            return NoContent();

        return NotFound();
    }

    [HttpDelete("{id:Guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        if(await _reservationsService.DeleteAsync(new DeleteReservation(id)))
            return NoContent();

        return NotFound();
    }
}
