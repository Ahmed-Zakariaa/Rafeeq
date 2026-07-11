// Mirrors Rafeeq.Domain/Identity/DTOs/AdminDtos.cs.

export enum AccountStatus {
  Active = 1,
  Suspended = 2,
  Banned = 3,
}

export enum UserSortField {
  CreatedDate = 1,
  FullName = 2,
}

export interface UserListFilter {
  role?: string | null; // "Driver" | "Passenger" | "Admin"
  search?: string | null;
  accountStatus?: AccountStatus | null;
}

export interface CreateAdminRequest {
  fullName: string;
  email: string;
  phoneNumber: string;
  gender: number;
  countryId: number;
  permissions: string[];
}

// PascalCase: inside ApiResponse.Data.
export interface UserListItem {
  UniqueId: string;
  FullName: string;
  Email: string;
  PhoneNumber: string;
  Roles: string[];
  AccountStatus: string;
  VerificationLevel: string;
  IsActivated: boolean;
  CreatedDate: string;
}

export interface DashboardData {
  TotalUsers: number;
  Drivers: number;
  Passengers: number;
  Admins: number;
  TotalTrips: number;
  PublishedTrips: number;
  TotalBookings: number;
  TotalVehicles: number;
}
