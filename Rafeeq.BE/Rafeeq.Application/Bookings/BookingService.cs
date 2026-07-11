using Microsoft.EntityFrameworkCore;
using Rafeeq.Domain.Bookings;
using Rafeeq.Domain.Bookings.DTOs;
using Rafeeq.Domain.Bookings.IService;
using Rafeeq.Domain.Common;
using Rafeeq.Domain.Notifications.IService;
using Rafeeq.Domain.Trips;
using Rafeeq.Infrastructure.Persistence;
using Rafeeq.Infrastructure.Security;

namespace Rafeeq.Application.Bookings;

public class BookingService : IBookingService
{
    private readonly RafeeqDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly INotificationService _notifications;

    public BookingService(RafeeqDbContext db, ICurrentUser currentUser, INotificationService notifications)
    {
        _db = db;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task<ResultViewModel<bool>> Request(BookingRequestDto dto)
    {
        var passengerId = _currentUser.UserId ?? throw new BusinessException("notAuthenticated");
        var tripId = EncryptionHelper.DecryptFromUrl(dto.TripUniqueId);

        var trip = await _db.Trips.FirstOrDefaultAsync(t => t.Id == tripId)
            ?? throw new BusinessException("tripNotFound");

        if (trip.Status != TripStatus.Published)
            throw new BusinessException("tripNotJoinable");
        if (trip.DriverId == passengerId)
            throw new BusinessException("cannotBookOwnTrip");
        if (trip.SeatsAvailable <= 0)
            throw new BusinessException("noSeatsAvailable");

        var passenger = await _db.Users.FirstOrDefaultAsync(u => u.Id == passengerId)
            ?? throw new BusinessException("userNotFound");

        if (trip.GenderPreference == GenderPreference.MaleOnly && passenger.Gender != Gender.Male)
            throw new BusinessException("genderNotAllowed");
        if (trip.GenderPreference == GenderPreference.FemaleOnly && passenger.Gender != Gender.Female)
            throw new BusinessException("genderNotAllowed");

        var hasActive = await _db.Bookings.AnyAsync(b => b.TripId == tripId && b.PassengerId == passengerId
            && (b.Status == BookingStatus.Requested || b.Status == BookingStatus.Accepted
                || b.Status == BookingStatus.Confirmed));
        if (hasActive)
            throw new BusinessException("alreadyHasActiveBooking");

        var agreedPrice = trip.IsFree ? 0 : trip.PricePerSeat;
        var booking = new Booking(tripId, passengerId, dto.PickupNote, agreedPrice, DateTime.UtcNow);
        await _db.Bookings.AddAsync(booking);

        await _notifications.Notify(trip.DriverId, NotificationType.BookingRequested,
            "notif.bookingRequested", passenger.FullName);
        await _db.SaveChangesAsync();

        return ResultViewModel<bool>.Success(true);
    }

    public async Task<ResultViewModel<bool>> Respond(BookingRespondDto dto)
    {
        var driverId = _currentUser.UserId ?? throw new BusinessException("notAuthenticated");
        var bookingId = EncryptionHelper.DecryptFromUrl(dto.BookingUniqueId);

        var booking = await _db.Bookings.Include(b => b.Trip)
            .FirstOrDefaultAsync(b => b.Id == bookingId)
            ?? throw new BusinessException("bookingNotFound");

        if (booking.Trip!.DriverId != driverId)
            throw new BusinessException("notTripOwner");
        if (booking.Status != BookingStatus.Requested)
            throw new BusinessException("bookingNotPending");

        if (dto.Accept)
        {
            booking.Trip.ReserveSeat();   // decrements seats; flips trip to Full at zero
            booking.Accept(DateTime.UtcNow);
        }
        else
        {
            booking.Reject(DateTime.UtcNow);
        }

        await _notifications.Notify(booking.PassengerId,
            dto.Accept ? NotificationType.BookingAccepted : NotificationType.BookingRejected,
            dto.Accept ? "notif.bookingAccepted" : "notif.bookingRejected", null);
        await _db.SaveChangesAsync();
        return ResultViewModel<bool>.Success(true);
    }

    public async Task<ResultViewModel<bool>> Cancel(string uniqueId)
    {
        var userId = _currentUser.UserId ?? throw new BusinessException("notAuthenticated");
        var bookingId = EncryptionHelper.DecryptFromUrl(uniqueId);

        var booking = await _db.Bookings.Include(b => b.Trip)
            .FirstOrDefaultAsync(b => b.Id == bookingId)
            ?? throw new BusinessException("bookingNotFound");

        if (booking.PassengerId != userId)
            throw new BusinessException("notBookingOwner");
        if (booking.Status is not (BookingStatus.Requested or BookingStatus.Accepted or BookingStatus.Confirmed))
            throw new BusinessException("cannotCancelBooking");

        // Free the seat back if it had been reserved.
        if (booking.Status is BookingStatus.Accepted or BookingStatus.Confirmed)
            booking.Trip!.ReleaseSeat();

        booking.CancelByPassenger();
        await _notifications.Notify(booking.Trip!.DriverId, NotificationType.BookingCancelled,
            "notif.bookingCancelled", null);
        await _db.SaveChangesAsync();
        return ResultViewModel<bool>.Success(true);
    }

    public async Task<ResultViewModel<List<BookingResultDto>>> GetMine()
    {
        var userId = _currentUser.UserId ?? throw new BusinessException("notAuthenticated");

        var list = await _db.Bookings.AsNoTracking()
            .Include(b => b.Trip).ThenInclude(t => t!.Driver)
            .Include(b => b.Trip).ThenInclude(t => t!.OriginCity)
            .Include(b => b.Trip).ThenInclude(t => t!.DestinationCity)
            .Where(b => b.PassengerId == userId)
            .OrderByDescending(b => b.RequestedDate)
            .ToListAsync();

        var rated = await RatedTrips(userId, list.Select(b => b.TripId));
        return ResultViewModel<List<BookingResultDto>>.Success(
            list.Select(b => ToDto(b, driverView: false, rated.Contains((b.TripId, b.Trip!.DriverId)))).ToList());
    }

    public async Task<ResultViewModel<List<BookingResultDto>>> GetIncoming()
    {
        var driverId = _currentUser.UserId ?? throw new BusinessException("notAuthenticated");

        var list = await _db.Bookings.AsNoTracking()
            .Include(b => b.Trip).ThenInclude(t => t!.OriginCity)
            .Include(b => b.Trip).ThenInclude(t => t!.DestinationCity)
            .Include(b => b.Passenger)
            .Where(b => b.Trip!.DriverId == driverId)
            .OrderByDescending(b => b.RequestedDate)
            .ToListAsync();

        var rated = await RatedTrips(driverId, list.Select(b => b.TripId));
        return ResultViewModel<List<BookingResultDto>>.Success(
            list.Select(b => ToDto(b, driverView: true, rated.Contains((b.TripId, b.PassengerId)))).ToList());
    }

    /// <summary>(tripId, toUserId) pairs the given user has already rated — to flag HasRated.</summary>
    private async Task<HashSet<(int, int)>> RatedTrips(int fromUserId, IEnumerable<int> tripIds)
    {
        var ids = tripIds.Distinct().ToList();
        var rows = await _db.Ratings.AsNoTracking()
            .Where(r => r.FromUserId == fromUserId && ids.Contains(r.TripId))
            .Select(r => new { r.TripId, r.ToUserId })
            .ToListAsync();
        return rows.Select(x => (x.TripId, x.ToUserId)).ToHashSet();
    }

    private BookingResultDto ToDto(Booking b, bool driverView, bool hasRated)
    {
        var t = b.Trip!;
        var en = _currentUser.IsEnglish;
        var revealed = b.Status is BookingStatus.Accepted or BookingStatus.Confirmed or BookingStatus.Completed;

        return new BookingResultDto
        {
            UniqueId = EncryptionHelper.EncryptToUrl(b.Id),
            TripUniqueId = EncryptionHelper.EncryptToUrl(t.Id),
            OriginCity = en ? t.OriginCity?.NameEn ?? "" : t.OriginCity?.NameAr ?? "",
            DestinationCity = en ? t.DestinationCity?.NameEn ?? "" : t.DestinationCity?.NameAr ?? "",
            DepartureDateTime = t.DepartureDateTime,
            DriverName = t.Driver?.FullName ?? "",
            PassengerName = b.Passenger?.FullName ?? "",
            ContactPhone = revealed ? (driverView ? b.Passenger?.PhoneNumber : t.Driver?.PhoneNumber) : null,
            SeatsRequested = b.SeatsRequested,
            PickupNote = b.PickupNote,
            AgreedPrice = b.AgreedPrice,
            IsFree = t.IsFree,
            CurrencyCode = t.CurrencyCode,
            Status = b.Status.ToString(),
            RequestedDate = b.RequestedDate,
            RespondedDate = b.RespondedDate,
            HasRated = hasRated,
        };
    }
}
