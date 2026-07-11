using Microsoft.EntityFrameworkCore;
using Rafeeq.Domain.Common;
using Rafeeq.Domain.Ratings;
using Rafeeq.Domain.Ratings.DTOs;
using Rafeeq.Domain.Ratings.IService;
using Rafeeq.Infrastructure.Persistence;
using Rafeeq.Infrastructure.Security;

namespace Rafeeq.Application.Ratings;

public class RatingService : IRatingService
{
    private readonly RafeeqDbContext _db;
    private readonly ICurrentUser _currentUser;

    public RatingService(RafeeqDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ResultViewModel<bool>> Rate(RateDto dto)
    {
        var fromId = _currentUser.UserId ?? throw new BusinessException("notAuthenticated");
        var bookingId = EncryptionHelper.DecryptFromUrl(dto.BookingUniqueId);

        var booking = await _db.Bookings.Include(b => b.Trip)
            .FirstOrDefaultAsync(b => b.Id == bookingId)
            ?? throw new BusinessException("bookingNotFound");

        if (booking.Status != BookingStatus.Completed)
            throw new BusinessException("cannotRateUncompleted");

        int toId;
        RatingRole role;
        if (booking.PassengerId == fromId)
        {
            toId = booking.Trip!.DriverId;
            role = RatingRole.DriverRated;       // passenger rates the driver
        }
        else if (booking.Trip!.DriverId == fromId)
        {
            toId = booking.PassengerId;
            role = RatingRole.PassengerRated;    // driver rates the passenger
        }
        else
        {
            throw new BusinessException("notRelatedToBooking");
        }

        var already = await _db.Ratings.AnyAsync(r =>
            r.TripId == booking.TripId && r.FromUserId == fromId && r.ToUserId == toId);
        if (already)
            throw new BusinessException("alreadyRated");

        var rating = new Rating(booking.TripId, fromId, toId, role, dto.Stars, dto.Comment);
        await _db.Ratings.AddAsync(rating);
        await _db.SaveChangesAsync();

        return ResultViewModel<bool>.Success(true);
    }
}
