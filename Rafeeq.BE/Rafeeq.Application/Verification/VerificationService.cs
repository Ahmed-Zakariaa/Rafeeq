using Microsoft.EntityFrameworkCore;
using Rafeeq.Domain.Common;
using Rafeeq.Domain.Verification;
using Rafeeq.Domain.Verification.DTOs;
using Rafeeq.Domain.Verification.IService;
using Rafeeq.Infrastructure.Persistence;
using Rafeeq.Infrastructure.Security;

namespace Rafeeq.Application.Verification;

public class VerificationService : IVerificationService
{
    private readonly RafeeqDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IFileStorage _storage;

    public VerificationService(RafeeqDbContext db, ICurrentUser currentUser, IFileStorage storage)
    {
        _db = db;
        _currentUser = currentUser;
        _storage = storage;
    }

    public async Task<ResultViewModel<bool>> Upload(UploadDocumentDto dto)
    {
        var userId = _currentUser.UserId ?? throw new BusinessException("notAuthenticated");

        var key = await _storage.SaveAsync(dto.Content, dto.FileName);
        var doc = new VerificationDocument(userId, dto.DocType, dto.FileName, key, dto.ContentType);
        await _db.VerificationDocuments.AddAsync(doc);
        await _db.SaveChangesAsync();

        return ResultViewModel<bool>.Success(true);
    }

    public async Task<ResultViewModel<List<MyDocumentDto>>> GetMine()
    {
        var userId = _currentUser.UserId ?? throw new BusinessException("notAuthenticated");

        var list = await _db.VerificationDocuments.AsNoTracking()
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.CreatedDate)
            .Select(d => new MyDocumentDto
            {
                UniqueId = EncryptionHelper.EncryptToUrl(d.Id),
                DocType = d.DocType.ToString(),
                FileName = d.FileName,
                Status = d.Status.ToString(),
                RejectionReason = d.RejectionReason,
                CreatedDate = d.CreatedDate,
                ReviewedDate = d.ReviewedDate,
            })
            .ToListAsync();

        return ResultViewModel<List<MyDocumentDto>>.Success(list);
    }

    public async Task<ResultViewModel<List<PendingDocumentDto>>> GetPending()
    {
        var list = await _db.VerificationDocuments.AsNoTracking()
            .Include(d => d.User)
            .Where(d => d.Status == DocStatus.Pending)
            .OrderBy(d => d.CreatedDate)
            .Select(d => new PendingDocumentDto
            {
                UniqueId = EncryptionHelper.EncryptToUrl(d.Id),
                UserName = d.User!.FullName,
                UserEmail = d.User.Email,
                DocType = d.DocType.ToString(),
                Status = d.Status.ToString(),
                CreatedDate = d.CreatedDate,
            })
            .ToListAsync();

        return ResultViewModel<List<PendingDocumentDto>>.Success(list);
    }

    public async Task<ResultViewModel<bool>> Review(VerificationReviewDto dto)
    {
        var adminId = _currentUser.UserId ?? throw new BusinessException("notAuthenticated");
        var id = EncryptionHelper.DecryptFromUrl(dto.DocumentUniqueId);

        var doc = await _db.VerificationDocuments.FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new BusinessException("documentNotFound");
        if (doc.Status != DocStatus.Pending)
            throw new BusinessException("documentAlreadyReviewed");

        if (dto.Approve)
        {
            doc.Approve(adminId, DateTime.UtcNow);
            // Approval grants the Verified badge.
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == doc.UserId);
            user?.MarkDocumentVerified();
        }
        else
        {
            if (string.IsNullOrWhiteSpace(dto.RejectionReason))
                throw new BusinessException("rejectionReasonRequired");
            doc.Reject(adminId, DateTime.UtcNow, dto.RejectionReason);
        }

        await _db.SaveChangesAsync();
        return ResultViewModel<bool>.Success(true);
    }

    public async Task<DownloadFileDto?> GetFile(string uniqueId, bool canReview)
    {
        var id = EncryptionHelper.DecryptFromUrl(uniqueId);
        var doc = await _db.VerificationDocuments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        if (doc == null) return null;
        if (doc.UserId != _currentUser.UserId && !canReview) return null; // not owner, not reviewer

        var content = await _storage.ReadAsync(doc.StoredKey);
        if (content == null) return null;

        return new DownloadFileDto { Content = content, ContentType = doc.ContentType, FileName = doc.FileName };
    }
}
