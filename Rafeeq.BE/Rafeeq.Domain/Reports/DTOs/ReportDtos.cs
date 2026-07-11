using Rafeeq.Domain.Common;

namespace Rafeeq.Domain.Reports.DTOs;

public class ReportCreateDto
{
    public string BookingUniqueId { get; set; } = string.Empty; // derives the trip + reported user
    public ReportReason Reason { get; set; }
    public string? Description { get; set; }
}

public class ReportHandleDto
{
    public string ReportUniqueId { get; set; } = string.Empty;
    public ReportStatus Status { get; set; } // Reviewed / ActionTaken / Dismissed
}

public class ReportListItemDto
{
    public string UniqueId { get; set; } = string.Empty;
    public string ReporterName { get; set; } = string.Empty;
    public string ReportedName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? HandledDate { get; set; }
}
