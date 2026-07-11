using Rafeeq.Domain.Bookings;
using Rafeeq.Domain.Common;
using Rafeeq.Domain.Geography;
using Rafeeq.Domain.Identity;
using Rafeeq.Domain.Vehicles;

namespace Rafeeq.Domain.Trips;

/// <summary>
/// A one-off intercity trip a driver offers. Seat accounting and status transitions live here
/// (the Trip state machine, docs/v1-spec.md §5) — services orchestrate, the entity guards its own rules.
/// </summary>
public class Trip : BaseEntity<int>
{
    public int DriverId { get; private set; }
    public User? Driver { get; private set; }
    public int VehicleId { get; private set; }
    public Vehicle? Vehicle { get; private set; }

    public int OriginCityId { get; private set; }
    public City? OriginCity { get; private set; }
    public int DestinationCityId { get; private set; }
    public City? DestinationCity { get; private set; }

    public DateTime DepartureDateTime { get; private set; }
    public int SeatsLimit { get; private set; }
    public int SeatsAvailable { get; private set; }

    public bool IsFree { get; private set; }
    public decimal PricePerSeat { get; private set; }
    public string CurrencyCode { get; private set; } = string.Empty;

    public GenderPreference GenderPreference { get; private set; }
    public string PickupPointText { get; private set; } = string.Empty;
    public string? Notes { get; private set; }

    public TripStatus Status { get; private set; } = TripStatus.Draft;

    public ICollection<Booking> Bookings { get; private set; } = new List<Booking>();

    private Trip() { }

    public Trip(int driverId, int vehicleId, int originCityId, int destinationCityId,
        DateTime departureDateTime, int seatsLimit, bool isFree, decimal pricePerSeat,
        string currencyCode, GenderPreference genderPreference, string pickupPointText, string? notes)
    {
        if (originCityId == destinationCityId)
            throw new BusinessException("originAndDestinationMustDiffer", nameof(destinationCityId));
        if (seatsLimit < 1)
            throw new BusinessException("seatsLimitMustBeAtLeastOne", nameof(seatsLimit));

        DriverId = driverId;
        VehicleId = vehicleId;
        OriginCityId = originCityId;
        DestinationCityId = destinationCityId;
        DepartureDateTime = departureDateTime;
        SeatsLimit = seatsLimit;
        SeatsAvailable = seatsLimit;
        IsFree = isFree;
        PricePerSeat = isFree ? 0 : pricePerSeat;
        CurrencyCode = currencyCode;
        GenderPreference = genderPreference;
        PickupPointText = pickupPointText;
        Notes = notes;
        Status = TripStatus.Published;   // v1: posting publishes immediately
    }

    /// <summary>Called when a booking is accepted. Decrements seats; flips to Full at zero.</summary>
    public void ReserveSeat()
    {
        if (SeatsAvailable <= 0)
            throw new BusinessException("noSeatsAvailable");
        SeatsAvailable--;
        if (SeatsAvailable == 0)
            Status = TripStatus.Full;
    }

    /// <summary>Called when an accepted booking is cancelled before departure. Frees a seat.</summary>
    public void ReleaseSeat()
    {
        if (SeatsAvailable < SeatsLimit)
            SeatsAvailable++;
        if (Status == TripStatus.Full && SeatsAvailable > 0)
            Status = TripStatus.Published;
    }

    public void Cancel()
    {
        if (Status is TripStatus.Completed or TripStatus.InProgress)
            throw new BusinessException("cannotCancelStartedOrCompletedTrip");
        Status = TripStatus.Cancelled;
    }

    public void Start() => Status = TripStatus.InProgress;
    public void Complete() => Status = TripStatus.Completed;
}
