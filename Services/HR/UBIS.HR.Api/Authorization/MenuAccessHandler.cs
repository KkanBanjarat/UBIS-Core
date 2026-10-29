using Microsoft.AspNetCore.Authorization;
using UBIS.HR.Application.Authorization;

namespace UBIS.HR.Api.Authorization;

public class MenuAccessHandler : AuthorizationHandler<MenuAccessRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MenuAccessRequirement requirement)
    {
        // claim "menu" รูปแบบ "resource:level" เช่น "benefit-claim:2" หรือ "*:3" (SuperAdmin)
        foreach (var c in context.User.Claims.Where(c => c.Type == "menu"))
        {
            var idx = c.Value.LastIndexOf(':');
            if (idx <= 0) continue;

            var resource = c.Value[..idx];
            if (!short.TryParse(c.Value[(idx + 1)..], out var level)) continue;

            if ((resource == "*" || resource == requirement.Resource) && level >= requirement.MinLevel)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }
        return Task.CompletedTask;
    }
}