using Rafeeq.Domain.Common;
using Rafeeq.Domain.Identity;
using Rafeeq.Domain.Trips;

namespace Rafeeq.Domain.Reports;

/// <summary>One user reporting another (in a trip/booking context). Admins handle it. docs/v1-spec.md §4.</summary>
public class Report : BaseEntity<int>
{
    public int? TripId { get; private set; }
    public Trip? Trip { get; private set; }

    public int ReporterUserId { get; private set; }
    public User? Reporter { get; private set; }
    public int ReportedUserId { get; private set; }
    public User? Reported { get; private set; }

    public ReportReason Reason { get; private set; }
    public string? Description { get; private set; }

    public ReportStatus Status { get; private set; } = ReportStatus.Open;
    public int? HandledByAdminId { get; private set; }
    public DateTime? HandledDate { get; private set; }

    private Report() { }

    public Report(int? tripId, int reporterUserId, int reportedUserId, ReportReason reason, string? description)
    {
        TripId = tripId;
        ReporterUserId = reporterUserId;
        ReportedUserId = reportedUserId;
        Reason = reason;
        Description = description;
    }

    public void Handle(int adminId, ReportStatus status, DateTime when)
    {
        Status = status;
        HandledByAdminId = adminId;
        HandledDate = when;
    }
}
