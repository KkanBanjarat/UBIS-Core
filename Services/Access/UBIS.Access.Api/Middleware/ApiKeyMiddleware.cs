namespace UBIS.Access.Api.Middleware;

/// <summary>
/// บังคับให้ทุก Request ต้องแนบ API Key มาด้วย นอกเหนือจาก JWT Token
/// เป็น Layer ป้องกันเพิ่มเติม กันการเรียก API ตรงจากเครื่องมือภายนอก
/// </summary>
public class ApiKeyMiddleware
{
    private const string ApiKeyHeaderName = "X-Api-Key";

    private readonly RequestDelegate _next;
    private readonly string _expectedApiKey;
    private readonly ILogger<ApiKeyMiddleware> _logger;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _expectedApiKey = configuration["SecurityHeaders:MySecretValue"] ?? string.Empty;

        // Fail Fast — ถ้าลืมตั้งค่าตอน Deploy ระบบจะไม่ยอมเริ่มทำงานเลย
        // ดีกว่าเริ่มได้แต่ไม่มีการป้องกันจริงโดยไม่มีใครรู้
        if (string.IsNullOrWhiteSpace(_expectedApiKey))
            throw new InvalidOperationException("SecurityHeaders:MySecretValue ยังไม่ได้ตั้งค่า");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // ข้าม Preflight (CORS) — Browser ไม่แนบ Custom Header มากับ OPTIONS
        if (HttpMethods.IsOptions(context.Request.Method))
        {
            await _next(context);
            return;
        }

        // ข้าม Swagger (เฉพาะ Development) และ Health Check
        if (path.StartsWith("/BeAccess", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/health", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var providedKey))
        {
            _logger.LogWarning("ปฏิเสธ Request ที่ไม่มี API Key: {Method} {Path} จาก {IP}",
                context.Request.Method, path, context.Connection.RemoteIpAddress);

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "ไม่พบ API Key" });
            return;
        }

        // เทียบแบบ Fixed-time กัน Timing Attack
        if (!CryptographicEquals(providedKey.ToString(), _expectedApiKey))
        {
            _logger.LogWarning("ปฏิเสธ Request ที่ API Key ไม่ถูกต้อง: {Method} {Path} จาก {IP}",
                context.Request.Method, path, context.Connection.RemoteIpAddress);

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "API Key ไม่ถูกต้อง" });
            return;
        }

        await _next(context);
    }

    private static bool CryptographicEquals(string a, string b)
    {
        return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(a),
            System.Text.Encoding.UTF8.GetBytes(b));
    }
}