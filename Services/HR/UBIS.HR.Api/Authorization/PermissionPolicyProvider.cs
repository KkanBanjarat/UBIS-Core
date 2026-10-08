using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using UBIS.HR.Application.Authorization;

namespace UBIS.HR.Api.Authorization;

/// <summary>
/// สร้าง Authorization Policy อัตโนมัติจากชื่อ Permission ที่ระบุใน [Authorize(Policy = "...")]
/// ทำให้เพิ่ม Permission ใหม่ได้โดยไม่ต้องแก้ Program.cs และ Deploy ใหม่ทุกครั้ง
/// </summary>
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackProvider;
    private readonly AuthorizationOptions _options;

    // Cache ไว้เพราะ GetPolicyAsync ถูกเรียกทุก Request ที่มี [Authorize(Policy=...)]
    private readonly ConcurrentDictionary<string, AuthorizationPolicy> _cache = new();

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _options = options.Value;
        _fallbackProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        => _fallbackProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        => _fallbackProvider.GetFallbackPolicyAsync();

    public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (string.IsNullOrWhiteSpace(policyName))
            return null;

        // Policy ที่ลงทะเบียนไว้ชัดเจนใน Program.cs มาก่อนเสมอ
        var existing = await _fallbackProvider.GetPolicyAsync(policyName);
        if (existing != null) return existing;

        return _cache.GetOrAdd(policyName, name =>
        {
            var builder = new AuthorizationPolicyBuilder(
                _options.DefaultPolicy.AuthenticationSchemes.ToArray());
            builder.RequireAuthenticatedUser();

            // รูปแบบ "menu.{resource}.{read|write|all}" เช่น menu.benefit-claim.write
            if (name.StartsWith("menu.", StringComparison.Ordinal))
            {
                var body = name["menu.".Length..];
                var dot = body.LastIndexOf('.');
                if (dot > 0)
                {
                    short level = body[(dot + 1)..] switch
                    {
                        "read" => 1,
                        "write" => 2,
                        "all" => 3,
                        _ => 0
                    };
                    if (level > 0)
                        return builder.AddRequirements(new MenuAccessRequirement(body[..dot], level)).Build();
                }
            }

            return builder.AddRequirements(new PermissionRequirement(name)).Build();
        });
    }
}