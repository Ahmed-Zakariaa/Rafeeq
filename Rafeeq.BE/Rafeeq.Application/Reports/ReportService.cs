using Microsoft.EntityFrameworkCore;
using Rafeeq.Domain.Common;
using Rafeeq.Domain.Reports;
using Rafeeq.Domain.Reports.DTOs;
using Rafeeq.Domain.Reports.IService;
using Rafeeq.Infrastructure.Persistence;
using Rafeeq.Infrastructure.Security;

namespace Rafeeq.Application.Reports;

public class ReportService : IReportService
{
    private readonly RafeeqDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ReportService(RafeeqDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ResultViewModel<bool>> Create(ReportCreateDto dto)
    {
        var reporterId = _currentUser.UserId ?? throw new BusinessException("notAuthenticated");
        var bookingId = EncryptionHelper.DecryptFromUrl(dto.BookingUniqueId);

        var booking = await _db.Bookings.Include(b => b.Trip)
            .FirstOrDefaultAsync(b => b.Id == bookingId)
            ?? throw new BusinessException("bookingNotFound");

        // The other party in this booking is who gets reported.
        int reportedId;
        if (booking.PassengerId == reporterId)
            reportedId = booking.Trip!.DriverId;
        else if (booking.Trip!.DriverId == reporterId)
            reportedId = booking.PassengerId;
        else
            throw new BusinessException("notRelatedToBooking");

        var report = new Report(booking.TripId, reporterId, reportedId, dto.Reason, dto.Description);
        await _db.Reports.AddAsync(report);
        await _db.SaveChangesAsync();

        return ResultViewModel<bool>.Success(true);
    }

    public async Task<ResultViewModel<List<ReportListItemDto>>> GetAll(ReportStatus? status)
    {
        var en = _currentUser.IsEnglish;

        var q = _db.Reports.AsNoTracking()
            .Include(r => r.Reporter)
            .Include(r => r.Reported)
            .Include(r => r.Trip).ThenInclude(t => t!.OriginCity)
            .Include(r => r.Trip).ThenInclude(t => t!.DestinationCity)
            .WhereIf(status != null, r => r.Status == status);

        var list = await q.OrderByDescending(r => r.CreatedDate).ToListAsync();

        var items = list.Select(r => new ReportListItemDto
        {
            UniqueId = EncryptionHelper.EncryptToUrl(r.Id),
            ReporterName = r.Reporter?.FullName ?? "",
            ReportedName = r.Reported?.FullName ?? "",
            Reason = r.Reason.ToString(),
            Description = r.Description,
            Status = r.Status.ToString(),
            Route = r.Trip == null ? "" :
                (en ? $"{r.Trip.OriginCity?.NameEn} → {r.Trip.DestinationCity?.NameEn}"
                    : $"{r.Trip.OriginCity?.NameAr} ← {r.Trip.DestinationCity?.NameAr}"),
            CreatedDate = r.CreatedDate,
            HandledDate = r.HandledDate,
        }).ToList();

        return ResultViewModel<List<ReportListItemDto>>.Success(items);
    }

    public async Task<ResultViewModel<bool>> Handle(ReportHandleDto dto)
    {
        var adminId = _currentUser.UserId ?? throw new BusinessException("notAuthenticated");
        var id = EncryptionHelper.DecryptFromUrl(dto.ReportUniqueId);

        var report = await _db.Reports.FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new BusinessException("reportNotFound");

        report.Handle(adminId, dto.Status, DateTime.UtcNow);
        await _db.SaveChangesAsync();

        return ResultViewModel<bool>.Success(true);
    }
}
