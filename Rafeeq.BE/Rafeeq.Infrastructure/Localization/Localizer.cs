namespace Rafeeq.Infrastructure.Localization;

/// <summary>
/// Resolves BusinessException / validation message KEYS to ar/en text. Unknown keys (e.g. built-in
/// FluentValidation sentences) pass through unchanged. Arabic is the default.
/// </summary>
public static class Localizer
{
    public static string Translate(string key, string lang)
        => Catalog.TryGetValue(key, out var v) ? (lang == "en" ? v.En : v.Ar) : key;

    private static readonly Dictionary<string, (string Ar, string En)> Catalog = new()
    {
        // Auth
        ["invalidCredentials"] = ("بيانات الدخول غير صحيحة", "Invalid email or password"),
        ["accountNotActivated"] = ("الحساب غير مُفعّل بعد، فعّله عبر الرابط المُرسل إلى بريدك", "Account not activated yet — activate it via the email link"),
        ["accountNotActive"] = ("الحساب موقوف أو محظور", "Account is suspended or banned"),
        ["emailAlreadyRegistered"] = ("البريد الإلكتروني مسجّل بالفعل", "Email is already registered"),
        ["countryNotFound"] = ("الدولة غير موجودة", "Country not found"),
        ["invalidOrExpiredToken"] = ("الرابط غير صالح أو منتهي الصلاحية", "The link is invalid or expired"),
        ["invalidOrExpiredOtp"] = ("الرمز غير صحيح أو منتهي الصلاحية", "The code is incorrect or expired"),
        ["notAuthenticated"] = ("يجب تسجيل الدخول أولاً", "You must be logged in"),
        ["unexpectedError"] = ("حدث خطأ غير متوقع", "An unexpected error occurred"),
        ["businessError"] = ("حدث خطأ", "An error occurred"),

        // Vehicles / Trips
        ["vehicleNotFoundOrNotOwned"] = ("السيارة غير موجودة أو ليست ملكك", "Vehicle not found or not yours"),
        ["seatsLimitExceedsVehicleCapacity"] = ("عدد المقاعد يتجاوز سعة السيارة", "Seats exceed the vehicle capacity"),
        ["originCityNotFound"] = ("مدينة الانطلاق غير موجودة", "Origin city not found"),
        ["destinationCityNotFound"] = ("مدينة الوصول غير موجودة", "Destination city not found"),
        ["tripMustBeWithinOneCountry"] = ("يجب أن تكون الرحلة داخل دولة واحدة", "A trip must be within one country"),
        ["tripNotFound"] = ("الرحلة غير موجودة", "Trip not found"),
        ["notTripOwner"] = ("لست صاحب هذه الرحلة", "You are not the trip owner"),
        ["cannotCancelStartedOrCompletedTrip"] = ("لا يمكن إلغاء رحلة بدأت أو اكتملت", "Cannot cancel a started or completed trip"),
        ["originAndDestinationMustDiffer"] = ("يجب أن تختلف مدينة الانطلاق عن الوصول", "Origin and destination must differ"),
        ["seatsLimitMustBeAtLeastOne"] = ("عدد المقاعد يجب أن يكون واحدًا على الأقل", "Seats must be at least one"),
        ["departureMustBeInFuture"] = ("يجب أن يكون موعد الانطلاق في المستقبل", "Departure must be in the future"),
        ["priceRequiredForPaidTrip"] = ("السعر مطلوب للرحلة المدفوعة", "Price is required for a paid trip"),
        ["priceExceedsCostShareCap"] = ("السعر يتجاوز الحد الأقصى لمشاركة التكلفة", "Price exceeds the cost-sharing cap"),

        // Bookings
        ["tripNotJoinable"] = ("هذه الرحلة غير متاحة للحجز", "This trip is not open for booking"),
        ["cannotBookOwnTrip"] = ("لا يمكنك حجز رحلتك الخاصة", "You can't book your own trip"),
        ["noSeatsAvailable"] = ("لا توجد مقاعد متاحة", "No seats available"),
        ["userNotFound"] = ("المستخدم غير موجود", "User not found"),
        ["genderNotAllowed"] = ("هذه الرحلة مخصّصة لنوع مختلف", "This trip is restricted to a different gender"),
        ["alreadyHasActiveBooking"] = ("لديك حجز نشط على هذه الرحلة بالفعل", "You already have an active booking on this trip"),
        ["bookingNotFound"] = ("الحجز غير موجود", "Booking not found"),
        ["bookingNotPending"] = ("لم يعد الحجز بانتظار الرد", "The booking is no longer pending"),
        ["notBookingOwner"] = ("لست صاحب هذا الحجز", "You are not the booking owner"),
        ["cannotCancelBooking"] = ("لا يمكن إلغاء هذا الحجز", "This booking can't be cancelled"),

        // Admin
        ["invalidPermission"] = ("صلاحية غير صالحة", "Invalid permission"),
        ["permissionsOnlyForAdmins"] = ("الصلاحيات تُمنح للمشرفين فقط", "Permissions can only be assigned to admins"),
        ["cannotManageSuperAdmin"] = ("لا يمكن إدارة المدير الأعلى", "The super admin can't be managed"),
        ["invalidStatus"] = ("حالة غير صالحة", "Invalid status"),
        ["roleNameRequired"] = ("اسم الدور مطلوب", "Role name is required"),
        ["roleNotFound"] = ("الدور غير موجود", "Role not found"),

        // Verification
        ["documentNotFound"] = ("المستند غير موجود", "Document not found"),
        ["documentAlreadyReviewed"] = ("تمت مراجعة المستند بالفعل", "Document has already been reviewed"),
        ["rejectionReasonRequired"] = ("سبب الرفض مطلوب", "A rejection reason is required"),
        ["fileRequired"] = ("الملف مطلوب", "A file is required"),
        ["fileTooLarge"] = ("حجم الملف كبير جدًا (الحد 5 ميجابايت)", "File is too large (max 5 MB)"),
        ["invalidFileType"] = ("نوع الملف غير مدعوم (JPG أو PNG أو PDF)", "Unsupported file type (JPG, PNG or PDF)"),

        // Reports
        ["notRelatedToBooking"] = ("لا يمكنك الإبلاغ عن هذا الحجز", "You can't report on this booking"),
        ["reportNotFound"] = ("البلاغ غير موجود", "Report not found"),

        // Trips completion / Ratings
        ["cannotCompleteTrip"] = ("لا يمكن إكمال هذه الرحلة", "This trip can't be completed"),
        ["cannotRateUncompleted"] = ("يمكن التقييم بعد اكتمال الرحلة فقط", "You can only rate after the trip is completed"),
        ["alreadyRated"] = ("لقد قمت بالتقييم بالفعل", "You have already rated"),
    };
}
