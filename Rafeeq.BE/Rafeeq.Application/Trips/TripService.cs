using Microsoft.EntityFrameworkCore;
using Rafeeq.Domain.Common;
using Rafeeq.Domain.Notifications.IService;
using Rafeeq.Domain.Trips;
using Rafeeq.Domain.Trips.DTOs;
using Rafeeq.Domain.Trips.IService;
using Rafeeq.Infrastructure.Persistence;
using Rafeeq.Infrastructure.Security;

namespace Rafeeq.Application.Trips;

public class TripService : ITripService
{
    private readonly RafeeqDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly INotificationService _notifications;

    public TripService(RafeeqDbContext db, ICurrentUser currentUser, INotificationService notifications)
    {
        _db = db;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task<ResultViewModel<bool>> Create(TripCreateDto dto)
    {
        var driverId = _currentUser.UserId ?? throw new BusinessException("notAuthenticated");

        var vehicle = await _db.Vehicles.AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == dto.VehicleId && v.UserId == driverId)
            ?? throw new BusinessException("vehicleNotFoundOrNotOwned", nameof(dto.VehicleId));

        if (dto.SeatsLimit > vehicle.SeatsCapacity)
            throw new BusinessException("seatsLimitExceedsVehicleCapacity", nameof(dto.SeatsLimit));

        var origin = await _db.Cities.Include(c => c.Country).FirstOrDefaultAsync(c => c.Id == dto.OriginCityId)
            ?? throw new BusinessException("originCityNotFound", nameof(dto.OriginCityId));
        var dest = await _db.Cities.AsNoTracking().FirstOrDefaultAsync(c => c.Id == dto.DestinationCityId)
            ?? throw new BusinessException("destinationCityNotFound", nameof(dto.DestinationCityId));

        if (origin.CountryId != dest.CountryId)
            throw new BusinessException("tripMustBeWithinOneCountry", nameof(dto.DestinationCityId));

        var trip = new Trip(driverId, dto.VehicleId, dto.OriginCityId, dto.DestinationCityId,
            dto.DepartureDateTime, dto.SeatsLimit, dto.IsFree, dto.IsFree ? 0 : dto.PricePerSeat,
            origin.Country!.CurrencyCode, dto.GenderPreference, dto.PickupPointText, dto.Notes);

        await _db.Trips.AddAsync(trip);
        await _db.SaveChangesAsync();

        return ResultViewModel<bool>.Success(true);
    }

    public async Task<ResultViewModel<List<TripResultDto>>> Search(
        QueryViewModel<TripSearchFilterDto, EnumTripSortDto> query)
    {
        var f = query.FilterModel;

        var q = _db.Trips.AsNoTracking()
            .Include(t => t.Driver).Include(t => t.OriginCity).Include(t => t.DestinationCity)
            .Where(t => t.Status == TripStatus.Published)   // only joinable trips appear in search
            .WhereIf(f?.OriginCityId != null, t => t.OriginCityId == f!.OriginCityId)
            .WhereIf(f?.DestinationCityId != null, t => t.DestinationCityId == f!.DestinationCityId)
            .WhereIf(f?.DepartureDate != null, t => t.DepartureDateTime.Date == f!.DepartureDate!.Value.Date)
            .WhereIf(f?.GenderPreference != null, t => t.GenderPreference == f!.GenderPreference)
            .WhereIf(f?.FreeOnly == true, t => t.IsFree);

        var sortField = query.OrderModel?.FieldName;
        var sortType = query.OrderModel?.SortType ?? SortType.Asc;
        q = q.SortIf(sortField is null or EnumTripSortDto.DepartureDateTime, t => t.DepartureDateTime, sortType);
        q = q.SortIf(sortField == EnumTripSortDto.PricePerSeat, t => t.PricePerSeat, sortType);
        q = q.SortIf(sortField == EnumTripSortDto.SeatsAvailable, t => t.SeatsAvailable, sortType);

        var total = await q.CountAsync();
        var page = await q.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize).ToListAsync();

        var ratings = await DriverRatings(page.Select(t => t.DriverId));
        var dtos = page.Select(t => ToDto(t, ratings)).ToList();

        return ResultViewModel<List<TripResultDto>>.Paged(dtos, total, query.PageNumber, query.PageSize);
    }

    public async Task<ResultViewModel<List<TripResultDto>>> GetMine()
    {
        var driverId = _currentUser.UserId ?? throw new BusinessException("notAuthenticated");

        var trips = await _db.Trips.AsNoTracking()
            .Include(t => t.Driver).Include(t => t.OriginCity).Include(t => t.DestinationCity)
            .Where(t => t.DriverId == driverId)
            .OrderByDescending(t => t.DepartureDateTime)
            .ToListAsync();

        var ratings = await DriverRatings(new[] { driverId });
        return ResultViewModel<List<TripResultDto>>.Success(trips.Select(t => ToDto(t, ratings)).ToList());
    }

    public async Task<ResultViewModel<TripResultDto>> GetOne(string uniqueId)
    {
        var id = EncryptionHelper.DecryptFromUrl(uniqueId);
        var trip = await _db.Trips.AsNoTracking()
            .Include(t => t.Driver).Include(t => t.OriginCity).Include(t => t.DestinationCity)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new BusinessException("tripNotFound");

        var ratings = await DriverRatings(new[] { trip.DriverId });
        return ResultViewModel<TripResultDto>.Success(ToDto(trip, ratings));
    }

    public async Task<ResultViewModel<bool>> Complete(string uniqueId)
    {
        var driverId = _currentUser.UserId ?? throw new BusinessException("notAuthenticated");
        var id = EncryptionHelper.DecryptFromUrl(uniqueId);

        var trip = await _db.Trips.Include(t => t.Bookings).FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new BusinessException("tripNotFound");
        if (trip.DriverId != driverId)
            throw new BusinessException("notTripOwner");
        if (trip.Status is TripStatus.Completed or TripStatus.Cancelled)
            throw new BusinessException("cannotCompleteTrip");

        trip.Complete();
        foreach (var b in trip.Bookings)
        {
            if (b.Status is BookingStatus.Accepted or BookingStatus.Confirmed)
            {
                b.Complete();
                await _notifications.Notify(b.PassengerId, NotificationType.TripCompleted, "notif.tripCompleted", null);
            }
            else if (b.Status == BookingStatus.Requested)
            {
                b.Reject(DateTime.UtcNow);   // pending requests are moot once the trip is done
            }
        }

        await _db.SaveChangesAsync();
        return ResultViewModel<bool>.Success(true);
    }

    public async Task<ResultViewModel<bool>> Cancel(string uniqueId)
    {
        var id = EncryptionHelper.DecryptFromUrl(uniqueId);
        var trip = await _db.Trips.FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new BusinessException("tripNotFound");

        if (trip.DriverId != _currentUser.UserId)
            throw new BusinessException("notTripOwner");

        trip.Cancel();
        await _db.SaveChangesAsync();
        return ResultViewModel<bool>.Success(true);
    }

    /// <summary>Average stars + count for each given driver (DriverRated ratings).</summary>
    private async Task<Dictionary<int, (double Avg, int Count)>> DriverRatings(IEnumerable<int> driverIds)
    {
        var ids = driverIds.Distinct().ToList();
        var rows = await _db.Ratings.AsNoTracking()
            .Where(r => r.RatedRole == RatingRole.DriverRated && ids.Contains(r.ToUserId))
            .GroupBy(r => r.ToUserId)
            .Select(g => new { ToUserId = g.Key, Avg = g.Average(x => (double)x.Stars), Count = g.Count() })
            .ToListAsync();
        return rows.ToDictionary(x => x.ToUserId, x => (x.Avg, x.Count));
    }

    private TripResultDto ToDto(Trip t, Dictionary<int, (double Avg, int Count)> ratings)
    {
        ratings.TryGetValue(t.DriverId, out var r);
        return new TripResultDto
        {
            UniqueId = EncryptionHelper.EncryptToUrl(t.Id),
            DriverName = t.Driver?.FullName ?? string.Empty,
            OriginCity = _currentUser.IsEnglish ? t.OriginCity?.NameEn ?? "" : t.OriginCity?.NameAr ?? "",
            DestinationCity = _currentUser.IsEnglish ? t.DestinationCity?.NameEn ?? "" : t.DestinationCity?.NameAr ?? "",
            DepartureDateTime = t.DepartureDateTime,
            SeatsLimit = t.SeatsLimit,
            SeatsAvailable = t.SeatsAvailable,
            IsFree = t.IsFree,
            PricePerSeat = t.PricePerSeat,
            CurrencyCode = t.CurrencyCode,
            GenderPreference = t.GenderPreference.ToString(),
            PickupPointText = t.PickupPointText,
            Status = t.Status.ToString(),
            IsMine = t.DriverId == _currentUser.UserId,
            DriverRating = Math.Round(r.Avg, 1),
            DriverRatingCount = r.Count,
        };
    }
}
