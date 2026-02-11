using MySpot.Core.Abstractions;
using MySpot.Core.Entities;
using MySpot.Core.Exceptions;
using MySpot.Core.Policies;
using MySpot.Core.ValueObjects;

namespace MySpot.Core.DamainServices;

internal sealed class ParkingreservationService : IParkingreservationService
{
    private readonly IEnumerable<IReservationPolicy> _policies;
    private readonly IClock _clock;

    public ParkingreservationService(IEnumerable<IReservationPolicy> policies, IClock clock)
    {
        _policies = policies;
        _clock = clock;
    }
    
    public void ReserveSpotForVehicle(IEnumerable<WeeklyParkingSpot> allParkingSpots,
        JobTitle jobTitle, WeeklyParkingSpot parkingSpotToReserve, Reservation reservation)
    {
        var parkingSpotId = parkingSpotToReserve.Id;
        var policy = _policies.SingleOrDefault(x => x.CanBeApplied(jobTitle));

        if (policy is null)
        {
            throw new NoReservationPolicyFoundException(jobTitle);
        }
        
        if(!policy.CanReserve(allParkingSpots, reservation.EmployeeName))
        {
            throw new CannotReserveParkingSpotException(parkingSpotId);
        }
        
        parkingSpotToReserve.AddReservation(reservation, new Date(_clock.Current()));
    }
}