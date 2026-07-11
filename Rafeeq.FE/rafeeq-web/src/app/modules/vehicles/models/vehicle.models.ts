// Mirrors Rafeeq.Domain/Vehicles/DTOs/VehicleDtos.cs.

export interface VehicleCreateRequest {
  make: string;
  model: string;
  color: string;
  plateNumber: string;
  seatsCapacity: number;
}

// PascalCase: comes back inside ApiResponse.Data.
export interface VehicleResult {
  Id: number;
  Make: string;
  Model: string;
  Color: string;
  PlateNumber: string;
  SeatsCapacity: number;
}
