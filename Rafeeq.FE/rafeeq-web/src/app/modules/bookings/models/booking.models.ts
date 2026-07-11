// Mirrors Rafeeq.Domain/Bookings/DTOs/BookingDtos.cs.

export interface BookingRequest {
  tripUniqueId: string;
  pickupNote?: string | null;
}

export interface BookingRespond {
  bookingUniqueId: string;
  accept: boolean;
}

// PascalCase: inside ApiResponse.Data.
export interface BookingResult {
  UniqueId: string;
  TripUniqueId: string;
  OriginCity: string;
  DestinationCity: string;
  DepartureDateTime: string;
  DriverName: string;
  PassengerName: string;
  ContactPhone: string | null;
  SeatsRequested: number;
  PickupNote: string | null;
  AgreedPrice: number;
  IsFree: boolean;
  CurrencyCode: string;
  Status: string;
  RequestedDate: string;
  RespondedDate: string | null;
  HasRated: boolean;
}

// Statuses that a passenger can still cancel.
export const CANCELLABLE = ['Requested', 'Accepted', 'Confirmed'];
