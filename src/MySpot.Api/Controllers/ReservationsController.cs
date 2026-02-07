using Microsoft.AspNetCore.Mvc;
using MySpot.Api.Commands;
using MySpot.Api.DTO;
using MySpot.Api.Entities;
using MySpot.Api.Services;
using MySpot.Api.ValueObjects;

namespace MySpot.Api.Controllers;

[ApiController]
[Route("reservations")]
public class ReservationsController : ControllerBase
{
    private static Clock Clock = new ();
    private static readonly ReservationsService _service = new(new List<WeeklyParkingSpot>()
        {
            new WeeklyParkingSpot(Guid.Parse("00000000-0000-0000-0000-000000000001"), new Week(Clock.Current()), name:"P1" ),
            new WeeklyParkingSpot(Guid.Parse("00000000-0000-0000-0000-000000000002"), new Week(Clock.Current()), name:"P2" ),
            new WeeklyParkingSpot(Guid.Parse("00000000-0000-0000-0000-000000000003"), new Week(Clock.Current()), name:"P3" ),
            new WeeklyParkingSpot(Guid.Parse("00000000-0000-0000-0000-000000000004"), new Week(Clock.Current()), name:"P4" ),
            new WeeklyParkingSpot(Guid.Parse("00000000-0000-0000-0000-000000000005"), new Week(Clock.Current()), name:"P5" ),
        }
    );
    
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