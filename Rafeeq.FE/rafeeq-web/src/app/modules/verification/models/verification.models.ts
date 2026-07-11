// Mirrors Rafeeq.Domain/Verification/DTOs + Common/Enums.cs.

export enum DocType {
  NationalId = 1,
  DriverLicense = 2,
  VehicleRegistration = 3,
}

export interface MyDocument {
  UniqueId: string;
  DocType: string;
  FileName: string;
  Status: string;
  RejectionReason: string | null;
  CreatedDate: string;
  ReviewedDate: string | null;
}

export interface PendingDocument {
  UniqueId: string;
  UserName: string;
  UserEmail: string;
  DocType: string;
  Status: string;
  CreatedDate: string;
}

export interface VerificationReviewRequest {
  documentUniqueId: string;
  approve: boolean;
  rejectionReason?: string | null;
}
