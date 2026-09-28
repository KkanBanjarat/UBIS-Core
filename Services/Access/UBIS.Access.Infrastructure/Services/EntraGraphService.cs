using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using UBIS.Access.Application.Dtos;
using UBIS.Access.Application.Interfaces;

namespace UBIS.Access.Infrastructure.Services;

public class EntraGraphService : IEntraGraphService
{
    private readonly IConfiguration _config;
    public EntraGraphService(IConfiguration config) => _config = config;

    public async Task<List<UserDto>> GetUsersAsync()
    {
        var tenantId = _config["Azure:TenantId"];
        var clientId = _config["Azure:ClientId"];
        var clientSecret = _config["Azure:ClientSecret"];
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(clientId)
            || string.IsNullOrWhiteSpace(clientSecret))
            throw new InvalidOperationException("ยังไม่ได้ตั้งค่า Azure:TenantId / Azure:ClientId / Azure:ClientSecret");

        try
        {
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            var graph = new GraphServiceClient(credential, new[] { "https://graph.microsoft.com/.default" });

            var raw = new List<User>();
            var page = await graph.Users.GetAsync(r =>
            {
                r.QueryParameters.Select = new[]
                {
                    "id", "displayName", "mail", "userPrincipalName",
                    "accountEnabled", "employeeId", "userType"
                };
                r.QueryParameters.Top = 999;
            });
            if (page is null) return new List<UserDto>();

            var iterator = PageIterator<User, UserCollectionResponse>
                .CreatePageIterator(graph, page, u => { raw.Add(u); return true; });
            await iterator.IterateAsync();

            return raw
                .Where(u => u.Id != null
                    && !string.Equals(u.UserType, "Guest", StringComparison.OrdinalIgnoreCase))
                .Select(u => new UserDto
                {
                    EntraObjectId = u.Id,
                    Email = (u.Mail ?? u.UserPrincipalName ?? "").Trim().ToLowerInvariant(),
                    DisplayName = u.DisplayName ?? "",
                    EmployeeCode = string.IsNullOrWhiteSpace(u.EmployeeId) ? null : u.EmployeeId.Trim(),
                    IsActive = u.AccountEnabled ?? true,
                }).ToList();
        }
        catch (ODataError ex)
        {
            throw new InvalidOperationException(
                $"เรียก Microsoft Graph ไม่สำเร็จ: {ex.Error?.Message} (ตรวจสอบสิทธิ์ User.Read.All และ Admin consent)");
        }
        catch (AuthenticationFailedException)
        {
            throw new InvalidOperationException("ขอ Token จาก Entra ไม่สำเร็จ ตรวจสอบ TenantId / ClientId / ClientSecret");
        }
    }
}