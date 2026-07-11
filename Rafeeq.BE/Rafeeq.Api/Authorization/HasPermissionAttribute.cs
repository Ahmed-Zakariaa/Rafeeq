using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Rafeeq.Domain.Common;

namespace Rafeeq.Api.Authorization;

/// <summary>
/// Guards an endpoint by a "Resource.Action" permission (from Domain.Identity.Permissions).
/// The permission is carried as a "permission" claim in the JWT, populated at login from the
/// user's roles (RolePermissions map). 401 if unauthenticated, 403 if the claim is missing or
/// the email isn't verified. Endpoints without this attribute (e.g. Auth/VerifyEmail) stay open
/// to unverified users so they can complete verification.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class HasPermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _permission;
    public HasPermissionAttribute(string permission) => _permission = permission;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Super admin bypasses every permission check.
        if (user.Claims.Any(c => c.Type == "isSuperAdmin" && c.Value == "true"))
            return;

        // Mandatory email verification for any permission-gated (resource) endpoint.
        if (!user.Claims.Any(c => c.Type == "emailVerified" && c.Value == "true"))
        {
            context.Result = new ObjectResult(ResultViewModel<object>.Failure("emailNotVerified"))
            {
                StatusCode = StatusCodes.Status403Forbidden,
            };
            return;
        }

        var hasPermission = user.Claims.Any(c => c.Type == "permission" && c.Value == _permission);
        if (!hasPermission)
            context.Result = new ForbidResult();
    }
}
