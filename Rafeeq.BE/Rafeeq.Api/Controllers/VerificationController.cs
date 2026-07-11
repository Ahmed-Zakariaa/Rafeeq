using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.Api.Authorization;
using Rafeeq.Domain.Common;
using Rafeeq.Domain.Identity;
using Rafeeq.Domain.Verification.DTOs;
using Rafeeq.Domain.Verification.IService;

namespace Rafeeq.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VerificationController : ControllerBase
{
    private static readonly string[] AllowedTypes = { "image/jpeg", "image/png", "application/pdf" };
    private const long MaxBytes = 5 * 1024 * 1024; // 5 MB

    private readonly IVerificationService _svc;
    public VerificationController(IVerificationService svc) => _svc = svc;

    [HasPermission(Permissions.VerificationUpload)]
    [HttpPost("Upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ResultViewModel<bool>>> Upload([FromForm] DocType docType, IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new BusinessException("fileRequired");
        if (file.Length > MaxBytes)
            throw new BusinessException("fileTooLarge");
        if (!AllowedTypes.Contains(file.ContentType))
            throw new BusinessException("invalidFileType");

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        return Ok(await _svc.Upload(new UploadDocumentDto
        {
            DocType = docType,
            FileName = file.FileName,
            ContentType = file.ContentType,
            Content = ms.ToArray(),
        }));
    }

    [HasPermission(Permissions.VerificationUpload)]
    [HttpGet("Mine")]
    public async Task<ActionResult<ResultViewModel<List<MyDocumentDto>>>> Mine()
        => Ok(await _svc.GetMine());

    [HasPermission(Permissions.VerificationReview)]
    [HttpGet("Pending")]
    public async Task<ActionResult<ResultViewModel<List<PendingDocumentDto>>>> Pending()
        => Ok(await _svc.GetPending());

    [HasPermission(Permissions.VerificationReview)]
    [HttpPost("Review")]
    public async Task<ActionResult<ResultViewModel<bool>>> Review(VerificationReviewDto dto)
        => Ok(await _svc.Review(dto));

    // Private file download: owner or a reviewer only.
    [HttpGet("File/{uniqueId}")]
    public async Task<IActionResult> Download(string uniqueId)
    {
        var canReview = User.Claims.Any(c =>
            (c.Type == "permission" && c.Value == Permissions.VerificationReview)
            || (c.Type == "isSuperAdmin" && c.Value == "true"));

        var f = await _svc.GetFile(uniqueId, canReview);
        if (f is null) return NotFound();
        return File(f.Content, f.ContentType, f.FileName);
    }
}
