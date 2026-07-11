using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.Domain.Common;
using Rafeeq.Domain.Identity.DTOs;
using Rafeeq.Domain.Identity.IService;

namespace Rafeeq.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    [AllowAnonymous]
    [HttpPost("Register")]
    public async Task<ActionResult<ResultViewModel<AuthResultDto>>> Register(RegisterDto dto)
        => Ok(await _auth.Register(dto));

    [AllowAnonymous]
    [HttpPost("Login")]
    public async Task<ActionResult<ResultViewModel<AuthResultDto>>> Login(LoginDto dto)
        => Ok(await _auth.Login(dto));

    [AllowAnonymous]
    [HttpPost("ForgotPassword")]
    public async Task<ActionResult<ResultViewModel<bool>>> ForgotPassword(ForgotPasswordDto dto)
        => Ok(await _auth.ForgotPassword(dto));

    // Used by both initial activation (invited accounts) and forgot-password reset.
    [AllowAnonymous]
    [HttpPost("ResetPassword")]
    public async Task<ActionResult<ResultViewModel<bool>>> ResetPassword(ResetPasswordDto dto)
        => Ok(await _auth.ResetPassword(dto));

    [Authorize]
    [HttpPost("VerifyEmail")]
    public async Task<ActionResult<ResultViewModel<AuthResultDto>>> VerifyEmail(VerifyEmailDto dto)
        => Ok(await _auth.VerifyEmail(dto));

    [Authorize]
    [HttpPost("ResendOtp")]
    public async Task<ActionResult<ResultViewModel<bool>>> ResendOtp()
        => Ok(await _auth.ResendOtp());
}
