namespace Rafeeq.Domain.Common;

public enum Gender { Male = 1, Female = 2 }

public enum AccountStatus { Active = 1, Suspended = 2, Banned = 3 }

public enum VerificationLevel { Unverified = 0, EmailVerified = 1, DocumentVerified = 2 }

public enum GenderPreference { Any = 0, MaleOnly = 1, FemaleOnly = 2 }

public enum TripStatus { Draft = 0, Published = 1, Full = 2, InProgress = 3, Completed = 4, Cancelled = 5 }

public enum BookingStatus
{
    Requested = 0,
    Accepted = 1,
    Rejected = 2,
    Confirmed = 3,
    CancelledByPassenger = 4,
    CancelledByDriver = 5,
    Completed = 6,
    NoShow = 7
}

public enum RoleType { Driver = 1, Passenger = 2, Admin = 3 }

public enum DocType { NationalId = 1, DriverLicense = 2, VehicleRegistration = 3 }

public enum DocStatus { Pending = 0, Approved = 1, Rejected = 2 }

public enum ReportReason { InappropriateBehavior = 1, NoShow = 2, UnsafeDriving = 3, Harassment = 4, Other = 5 }

public enum ReportStatus { Open = 0, Reviewed = 1, ActionTaken = 2, Dismissed = 3 }

public enum RatingRole { DriverRated = 1, PassengerRated = 2 }   // which side the rated user was on

public enum NotificationType
{
    General = 0,
    BookingRequested = 1,
    BookingAccepted = 2,
    BookingRejected = 3,
    BookingCancelled = 4,
    TripCompleted = 5,
}
