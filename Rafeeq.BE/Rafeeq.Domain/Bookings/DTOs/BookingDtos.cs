namespace Rafeeq.Domain.Bookings.DTOs;

public class BookingRequestDto
{
    public string TripUniqueId { get; set; } = string.Empty;
    public string? PickupNote { get; set; }
}

public class BookingRespondDto
{
    public string BookingUniqueId { get; set; } = string.Empty;
    public bool Accept { get; set; }   // true = accept (reserves a seat), false = reject
}

public class BookingResultDto
{
    public string UniqueId { get; set; } = string.Empty;
    public string TripUniqueId { get; set; } = string.Empty;
    public string OriginCity { get; set; } = string.Empty;
    public string DestinationCity { get; set; } = string.Empty;
    public DateTime DepartureDateTime { get; set; }

    public string DriverName { get; set; } = string.Empty;      // for the passenger's view
    public string PassengerName { get; set; } = string.Empty;   // for the driver's view
    public string? ContactPhone { get; set; }                   // revealed only after acceptance

    public int SeatsRequested { get; set; }
    public string? PickupNote { get; set; }
    public decimal AgreedPrice { get; set; }
    public bool IsFree { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
    public DateTime RequestedDate { get; set; }
    public DateTime? RespondedDate { get; set; }

    public bool HasRated { get; set; }   // current user already rated the counterparty for this trip
}
