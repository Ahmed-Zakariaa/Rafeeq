using Rafeeq.Domain.Common;

namespace Rafeeq.Domain.Trips.DTOs;

public class TripCreateDto
{
    public int VehicleId { get; set; }
    public int OriginCityId { get; set; }
    public int DestinationCityId { get; set; }
    public DateTime DepartureDateTime { get; set; }
    public int SeatsLimit { get; set; }
    public bool IsFree { get; set; }
    public decimal PricePerSeat { get; set; }
    public GenderPreference GenderPreference { get; set; } = GenderPreference.Any;
    public string PickupPointText { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class TripResultDto
{
    public string UniqueId { get; set; } = string.Empty;   // encrypted Id for URLs
    public string DriverName { get; set; } = string.Empty;
    public string OriginCity { get; set; } = string.Empty;
    public string DestinationCity { get; set; } = string.Empty;
    public DateTime DepartureDateTime { get; set; }
    public int SeatsLimit { get; set; }
    public int SeatsAvailable { get; set; }
    public bool IsFree { get; set; }
    public decimal PricePerSeat { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string GenderPreference { get; set; } = string.Empty;
    public string PickupPointText { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsMine { get; set; }   // true if the current user is the trip's driver

    public double DriverRating { get; set; }   // average stars (0 if none)
    public int DriverRatingCount { get; set; }
}

public class TripSearchFilterDto
{
    public int? OriginCityId { get; set; }
    public int? DestinationCityId { get; set; }
    public DateTime? DepartureDate { get; set; }
    public GenderPreference? GenderPreference { get; set; }
    public bool? FreeOnly { get; set; }
}

public enum EnumTripSortDto { DepartureDateTime = 1, PricePerSeat = 2, SeatsAvailable = 3 }