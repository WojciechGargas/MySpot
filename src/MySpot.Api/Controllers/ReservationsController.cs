using Microsoft.AspNetCore.Mvc;
using MySpot.Api.Commands;
using MySpot.Api.DTO;
using MySpot.Api.Entities;
using MySpot.Api.Services;

namespace MySpot.Api.Controllers;

[ApiController]
[Route("reservations")]
public class ReservationsController : ControllerBase
{
    private readonly ReservationsService _service = new();
    [HttpGet]
    public ActionResult<IEnumerable<ReservationDto>> Get() => Ok(_service.GetAllWeekly());

    [HttpGet("{id:Guid}")]
    public ActionResult<ReservationDto> Get(Guid id)
    {
        var reservationDto = _service.Get(id);
        if (reservationDto == null)
            return NotFound();
        
        return Ok(reservationDto);
    }

    [HttpPost]
    public ActionResult Post([FromBody] CreateReservation command)
    {
        var id = _service.Create((command with {ReservationId =  Guid.NewGuid()}));
        if (id == null)
            return BadRequest();
        
        return CreatedAtAction(nameof(Get), new { id }, null);
    }

    [HttpPut("{id:Guid}")]
    public ActionResult Put(Guid id, [FromBody] ChangeReservationLicensePlate command)
    {
        if(_service.Update(command with {ReservationId = id}))
            return NoContent();
        
        return NotFound();
    }

    [HttpDelete("{id:Guid}")]
    public ActionResult Delete(Guid id)
    {
        if(_service.Delete(new DeleteReservation(id)))
            return NoContent();
        
        return NotFound();
    }
}