namespace MySpot.Core.Exceptions;

public sealed class InvalidLicensePlateException : CustomException
{
    public InvalidLicensePlateException(string licensePlate) : base($"License plate: {licensePlate} is invalid")
    {
    }
}