using Rafeeq.Domain.Common;

namespace Rafeeq.Domain.Verification.DTOs;

/// <summary>Built by the controller from the multipart upload (not bound from JSON).</summary>
public class UploadDocumentDto
{
    public DocType DocType { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
}

public class VerificationReviewDto
{
    public string DocumentUniqueId { get; set; } = string.Empty;
    public bool Approve { get; set; }
    public string? RejectionReason { get; set; }
}

public class MyDocumentDto
{
    public string UniqueId { get; set; } = string.Empty;
    public string DocType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ReviewedDate { get; set; }
}

public class PendingDocumentDto
{
    public string UniqueId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string DocType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

/// <summary>Raw file payload streamed by the download endpoint (not wrapped in the envelope).</summary>
public class DownloadFileDto
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
