namespace MySpot.Api.Exceptions;

public class ReservationDoesNotExistException : CustomException
{
    public ReservationDoesNotExistException() : base("Reservation does not exist")
    {
    }
}