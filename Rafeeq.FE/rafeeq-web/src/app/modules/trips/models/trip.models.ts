// Mirrors Rafeeq.Domain/Trips/DTOs/TripDtos.cs + Common/Enums.cs.

export enum GenderPreference {
  Any = 0,
  MaleOnly = 1,
  FemaleOnly = 2,
}

// EnumTripSortDto on the BE.
export enum TripSortField {
  DepartureDateTime = 1,
  PricePerSeat = 2,
  SeatsAvailable = 3,
}

// Flat v1 cost-share cap enforced by the BE (TripCreateValidator.MaxPricePerSeat).
// Kept in sync here only as a client-side hint; the BE remains the source of truth.
export const MAX_PRICE_PER_SEAT = 2000;

export interface TripSearchFilter {
  originCityId?: number | null;
  destinationCityId?: number | null;
  departureDate?: string | null;
  genderPreference?: GenderPreference | null;
  freeOnly?: boolean | null;
}

export interface TripCreateRequest {
  vehicleId: number;
  originCityId: number;
  destinationCityId: number;
  departureDateTime: string;
  seatsLimit: number;
  isFree: boolean;
  pricePerSeat: number;
  genderPreference: GenderPreference;
  pickupPointText: string;
  notes?: string | null;
}

// PascalCase: comes back inside ApiResponse.Data.
export interface TripResult {
  UniqueId: string;
  DriverName: string;
  OriginCity: string;
  DestinationCity: string;
  DepartureDateTime: string;
  SeatsLimit: number;
  SeatsAvailable: number;
  IsFree: boolean;
  PricePerSeat: number;
  CurrencyCode: string;
  GenderPreference: string;
  PickupPointText: string;
  Status: string;
  IsMine: boolean;
  DriverRating: number;
  DriverRatingCount: number;
}
