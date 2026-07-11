// Mirrors the BE auth contract (Rafeeq.Domain/Identity/DTOs/AuthDtos.cs).
// Requests are camelCase (ASP.NET Core binds case-insensitively); the
// AuthResult comes back PascalCase inside ApiResponse.Data.

export enum Gender {
  Male = 1,
  Female = 2,
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  phoneNumber: string;
  password: string;
  gender: Gender;
  countryId: number;
  asDriver: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResult {
  Token: string;
  ExpiresAt: string;
  FullName: string;
  Roles: string[];
}

export interface CurrentUser {
  id: string;
  fullName: string;
  roles: string[];
  permissions: string[];
  isSuperAdmin: boolean;
  isEmailVerified: boolean;
  exp: number;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
}

export interface CountryOption {
  id: number;
  nameKey: string;
  phonePrefix: string;
}

// v1 has exactly two countries (seeded EG=1, KSA=2). No lookup endpoint yet,
// so we list them here; matches Rafeeq.Infrastructure/Persistence/DbSeeder.cs.
export const COUNTRIES: CountryOption[] = [
  { id: 1, nameKey: 'fields.egypt', phonePrefix: '+20' },
  { id: 2, nameKey: 'fields.ksa', phonePrefix: '+966' },
];
