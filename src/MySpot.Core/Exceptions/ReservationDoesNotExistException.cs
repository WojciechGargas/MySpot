namespace MySpot.Core.Exceptions;

public class ReservationDoesNotExistException : CustomException
{
    public ReservationDoesNotExistException() : base("Reservation does not exist")
    {
    }
}