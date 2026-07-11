using Rafeeq.Domain.Common;
using Rafeeq.Domain.Identity;

namespace Rafeeq.Domain.Verification;

/// <summary>
/// A document a user uploads for trust verification (National ID / driver license / vehicle reg).
/// An admin reviews it; approval grants the user the Verified badge. docs/v1-spec.md §3, §4.
/// </summary>
public class VerificationDocument : BaseEntity<int>
{
    public int UserId { get; private set; }
    public User? User { get; private set; }

    public DocType DocType { get; private set; }
    public string FileName { get; private set; } = string.Empty;     // original name (display)
    public string StoredKey { get; private set; } = string.Empty;    // key on disk (never exposed)
    public string ContentType { get; private set; } = string.Empty;

    public DocStatus Status { get; private set; } = DocStatus.Pending;
    public int? ReviewedByAdminId { get; private set; }
    public DateTime? ReviewedDate { get; private set; }
    public string? RejectionReason { get; private set; }

    private VerificationDocument() { }

    public VerificationDocument(int userId, DocType docType, string fileName, string storedKey, string contentType)
    {
        UserId = userId;
        DocType = docType;
        FileName = fileName;
        StoredKey = storedKey;
        ContentType = contentType;
    }

    public void Approve(int adminId, DateTime when)
    {
        Status = DocStatus.Approved;
        ReviewedByAdminId = adminId;
        ReviewedDate = when;
        RejectionReason = null;
    }

    public void Reject(int adminId, DateTime when, string? reason)
    {
        Status = DocStatus.Rejected;
        ReviewedByAdminId = adminId;
        ReviewedDate = when;
        RejectionReason = reason;
    }
}
