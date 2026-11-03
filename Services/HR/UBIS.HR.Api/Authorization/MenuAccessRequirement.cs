using Microsoft.AspNetCore.Authorization;
namespace UBIS.HR.Application.Authorization;

public class MenuAccessRequirement : IAuthorizationRequirement
{
    public string Resource { get; }
    public short MinLevel { get; }

    public MenuAccessRequirement(string resource, short minLevel)
    {
        Resource = resource;
        MinLevel = minLevel;
    }
}