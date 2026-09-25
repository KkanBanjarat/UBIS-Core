using Microsoft.AspNetCore.Authorization;
using UBIS.HR.Application.Authorization;

namespace UBIS.HR.Api.Authorization;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var permClaims = context.User.Claims
            .Where(c => c.Type == "perm")
            .Select(c => c.Value)
            .ToList();

        // SuperAdmin ("*" หรือ "*:Scope") ผ่านทุก Permission
        var isSuperAdmin = permClaims.Any(v => v == "*" || v.StartsWith("*:"));

        // รองรับทั้งแบบมี Scope ("employee.write:Branch") และไม่มี Scope ("employee.write")
        var hasPermission = permClaims.Any(v =>
            v == requirement.Permission
            || v.StartsWith(requirement.Permission + ":"));

        if (isSuperAdmin || hasPermission)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}