// Mirrors Rafeeq.Domain/Reports/DTOs + Common/Enums.cs.

export enum ReportReason {
  InappropriateBehavior = 1,
  NoShow = 2,
  UnsafeDriving = 3,
  Harassment = 4,
  Other = 5,
}

export enum ReportStatus {
  Open = 0,
  Reviewed = 1,
  ActionTaken = 2,
  Dismissed = 3,
}

export interface ReportCreateRequest {
  bookingUniqueId: string;
  reason: ReportReason;
  description?: string | null;
}

export interface ReportHandleRequest {
  reportUniqueId: string;
  status: ReportStatus;
}

// PascalCase: inside ApiResponse.Data.
export interface ReportListItem {
  UniqueId: string;
  ReporterName: string;
  ReportedName: string;
  Reason: string;
  Description: string | null;
  Status: string;
  Route: string;
  CreatedDate: string;
  HandledDate: string | null;
}
