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
    public ActionResult<IEnumerable<ReservationDto>> Get() => Ok(_reservationsService.GetAllWeekly());

    [HttpGet("{id:Guid}")]
    public ActionResult<ReservationDto> Get(Guid id)
    {
        var reservationDto = _reservationsService.Get(id);
        if (reservationDto == null)
            return NotFound();

        return Ok(reservationDto);
    }

    [HttpPost]
    public ActionResult Post([FromBody] CreateReservation command)
    {
        try
        {
            var id = _reservationsService.Create((command with {ReservationId =  Guid.NewGuid()}));
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
    public ActionResult Put(Guid id, [FromBody] ChangeReservationLicensePlate command)
    {
        if(_reservationsService.Update(command with {ReservationId = id}))
            return NoContent();

        return NotFound();
    }

    [HttpDelete("{id:Guid}")]
    public ActionResult Delete(Guid id)
    {
        if(_reservationsService.Delete(new DeleteReservation(id)))
            return NoContent();

        return NotFound();
    }
}
