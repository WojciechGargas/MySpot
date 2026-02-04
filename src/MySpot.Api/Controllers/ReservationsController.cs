using Microsoft.AspNetCore.Mvc;
using MySpot.Api.Models;

namespace MySpot.Api.Controllers;

[ApiController]
[Route("reservations")]
public class ReservationsController : ControllerBase
{
    private static int _id = 1;
    private static readonly List<Reservation> _reservations = new();

    private static readonly List<string> _parkingSpotNames = new()
    {
        "P1", "P2", "P3", "P4", "P5"
    };

    [HttpGet]
    public ActionResult<IEnumerable<Reservation>> Get() => Ok(_reservations);

    [HttpGet("{id:int}")]
    public ActionResult<Reservation> Get(int id)
    {
        var reservation = _reservations.SingleOrDefault(x => x.Id == id);
        if (reservation == null)
            return NotFound();
        return Ok(reservation);
    }

    [HttpPost]
    public ActionResult Post([FromBody] Reservation reservation)
    {
        if (_parkingSpotNames.All(x => x != reservation.ParkingSpotName))
        {
            return BadRequest();
        }
        
        reservation.Date = DateTime.UtcNow.AddDays(1).Date;
        var reservationAlreadyExists = _reservations.Any(x => 
            x.ParkingSpotName == reservation.ParkingSpotName
            && x.Date.Date == reservation.Date.Date );

        if (reservationAlreadyExists)
        {
            return BadRequest();
        }
        reservation.Id = _id;
        _id++;
        _reservations.Add(reservation);

        return CreatedAtAction(nameof(Get), new { id = reservation.Id }, null);
    }

    [HttpPut("{id:int}")]
    public ActionResult Put(int id, [FromBody] Reservation reservation)
    {
        var exisitngReservation = _reservations.SingleOrDefault(x => x.Id == id);
        if (exisitngReservation == null)
            return NotFound();


        exisitngReservation.LicensePlate = reservation.LicensePlate;
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        var exisitngReservation = _reservations.SingleOrDefault(x => x.Id == id);
        if (exisitngReservation == null)
            return NotFound();

        _reservations.Remove(exisitngReservation);
        return NoContent();
    }
}