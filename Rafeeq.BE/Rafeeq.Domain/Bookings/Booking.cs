using Rafeeq.Domain.Common;
using Rafeeq.Domain.Identity;
using Rafeeq.Domain.Trips;

namespace Rafeeq.Domain.Bookings;

/// <summary>A passenger's request for a seat on a trip. State machine in docs/v1-spec.md §5.</summary>
public class Booking : BaseEntity<int>
{
    public int TripId { get; private set; }
    public Trip? Trip { get; private set; }
    public int PassengerId { get; private set; }
    public User? Passenger { get; private set; }

    public int SeatsRequested { get; private set; } = 1;   // always 1 in v1
    public string? PickupNote { get; private set; }
    public decimal AgreedPrice { get; private set; }

    public BookingStatus Status { get; private set; } = BookingStatus.Requested;
    public DateTime RequestedDate { get; private set; }
    public DateTime? RespondedDate { get; private set; }

    private Booking() { }

    public Booking(int tripId, int passengerId, string? pickupNote, decimal agreedPrice, DateTime requestedDate)
    {
        TripId = tripId;
        PassengerId = passengerId;
        PickupNote = pickupNote;
        AgreedPrice = agreedPrice;
        RequestedDate = requestedDate;
    }

    public void Accept(DateTime when) { Status = BookingStatus.Accepted; RespondedDate = when; }
    public void Reject(DateTime when) { Status = BookingStatus.Rejected; RespondedDate = when; }
    public void Confirm() => Status = BookingStatus.Confirmed;
    public void CancelByPassenger() => Status = BookingStatus.CancelledByPassenger;
    public void CancelByDriver() => Status = BookingStatus.CancelledByDriver;
    public void Complete() => Status = BookingStatus.Completed;
    public void MarkNoShow() => Status = BookingStatus.NoShow;
}
