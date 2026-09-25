using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UBIS.HR.Application.Interfaces;

namespace UBIS.HR.Infrastructure.Services;

public class AccessServiceClient : IAccessServiceClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AccessServiceClient> _logger;

    public AccessServiceClient(HttpClient http,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        ILogger<AccessServiceClient> logger)
    {
        _http = http;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;

        var baseUrl = configuration["Services:AccessApiUrl"]
            ?? throw new InvalidOperationException("Services:AccessApiUrl ยังไม่ได้ตั้งค่า");

        _http.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        _http.Timeout = TimeSpan.FromSeconds(10);

        // Access Service ต้องการ API Key เหมือนกัน
        var apiKey = configuration["SecurityHeaders:AccessApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
            _http.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
    }

    private class UserLookupResponse
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public bool IsActive { get; set; }
    }

    public async Task<Guid?> LookupUserIdAsync(Guid employeeId, string? employeeCode, string? email)
    {
        try
        {
            // ส่ง Token ของผู้ใช้ปัจจุบันต่อไปให้ Access Service ตรวจสอบสิทธิ์
            // var token = _httpContextAccessor.HttpContext?.Request.Headers.Authorization
            //     .ToString()
            //     .Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);

            var url = $"Auth/lookup?employeeId={employeeId}";
            if (!string.IsNullOrWhiteSpace(employeeCode))
                url += $"&employeeCode={Uri.EscapeDataString(employeeCode)}";
            if (!string.IsNullOrWhiteSpace(email))
                url += $"&email={Uri.EscapeDataString(email)}";

            _logger.LogInformation(
            "กำลังเรียก Access Lookup: EmployeeId={EmployeeId}, EmployeeCode={EmployeeCode}, Email={Email}",
            employeeId,
            employeeCode,
            email);
            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            // if (!string.IsNullOrWhiteSpace(token))
            //     request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _http.SendAsync(request);
            _logger.LogInformation(
    "Access Lookup Response: {StatusCode}",
    response.StatusCode);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("เรียก Access Service ไม่สำเร็จ: {StatusCode} (EmployeeCode: {EmployeeCode})",
                    response.StatusCode, employeeCode);
                return null;
            }
            _logger.LogInformation(
    "LookupUser เริ่ม: EmployeeId={EmployeeId}, EmployeeCode={EmployeeCode}, Email={Email}",
    employeeId,
    employeeCode,
    email);
            var user = await response.Content.ReadFromJsonAsync<UserLookupResponse>();
            _logger.LogInformation("LookupUser เสร็จ");
            return user?.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการเรียก Access Service: {Message}", ex.Message);
            return null;
        }
    }
}