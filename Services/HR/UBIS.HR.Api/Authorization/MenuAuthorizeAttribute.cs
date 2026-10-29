using Microsoft.AspNetCore.Authorization;

namespace UBIS.HR.Api.Authorization;

public enum MenuLevel { Read, Write, All }

public class MenuAuthorizeAttribute : AuthorizeAttribute
{
    public MenuAuthorizeAttribute(string resource, MenuLevel level)
    {
        Policy = $"menu.{resource}.{level.ToString().ToLowerInvariant()}";
    }
}