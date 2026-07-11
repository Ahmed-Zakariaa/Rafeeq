using Rafeeq.Domain.Common;
using Rafeeq.Domain.Verification.DTOs;

namespace Rafeeq.Domain.Verification.IService;

public interface IVerificationService
{
    Task<ResultViewModel<bool>> Upload(UploadDocumentDto dto);
    Task<ResultViewModel<List<MyDocumentDto>>> GetMine();
    Task<ResultViewModel<List<PendingDocumentDto>>> GetPending();
    Task<ResultViewModel<bool>> Review(VerificationReviewDto dto);

    /// <summary>Returns the file if the caller owns it or may review (else null).</summary>
    Task<DownloadFileDto?> GetFile(string uniqueId, bool canReview);
}
