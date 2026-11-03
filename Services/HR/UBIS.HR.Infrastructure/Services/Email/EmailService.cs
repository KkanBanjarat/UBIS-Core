using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using UBIS.HR.Application.Interfaces;

namespace Ubis.Hr.Api.Services.Email;

public class EmailService : IEmailService
{
    private readonly EmailOptions _opt;
    private readonly IHttpClientFactory _http;
    private readonly ILogger<EmailService> _log;

    public EmailService(IOptions<EmailOptions> opt, IHttpClientFactory http, ILogger<EmailService> log)
    {
        _opt = opt.Value;
        _http = http;
        _log = log;
    }

    public async Task SendAsync(string? to, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(to)) return;

        try
        {
            switch (_opt.Provider)
            {
                case "Smtp": await SendSmtpAsync(to, subject, htmlBody, ct); break;
                case "Mailjet": await SendMailjetAsync(to, subject, htmlBody, ct); break;
                case "Webhook": await SendWebhookAsync(to, subject, htmlBody, ct); break;
                default:
                    _log.LogInformation("Email disabled. Skip mail to {To}: {Subject}", to, subject);
                    break;
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Send email failed. To={To}, Subject={Subject}", to, subject);
        }
    }

    private async Task SendSmtpAsync(string to, string subject, string html, CancellationToken ct)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_opt.FromName, _opt.FromAddress));
        msg.To.Add(MailboxAddress.Parse(to));
        msg.Subject = subject;
        msg.Body = new BodyBuilder { HtmlBody = html }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_opt.SmtpHost, _opt.SmtpPort,
            _opt.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto, ct);
        if (!string.IsNullOrEmpty(_opt.SmtpUser))
            await client.AuthenticateAsync(_opt.SmtpUser, _opt.SmtpPassword, ct);
        await client.SendAsync(msg, ct);
        await client.DisconnectAsync(true, ct);
    }
    private async Task SendMailjetAsync(string to, string subject, string html, CancellationToken ct)
    {
        _log.LogInformation(
            "Mailjet config source check: ApiKeyPresent={ApiKeyPresent}, " +
            "SecretPresent={SecretPresent}, ApiKeyLength={ApiKeyLength}, " +
            "SecretLength={SecretLength}",
            !string.IsNullOrWhiteSpace(_opt.MailjetApiKey),
            !string.IsNullOrWhiteSpace(_opt.MailjetSecretKey),
            _opt.MailjetApiKey?.Length ?? 0,
            _opt.MailjetSecretKey?.Length ?? 0);

        var client = _http.CreateClient();
        // var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_opt.SmtpUser}:{_opt.SmtpPassword}"));
        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_opt.MailjetApiKey}:{_opt.MailjetSecretKey}"));
        var req = new HttpRequestMessage(HttpMethod.Post, "https://api.mailjet.com/v3.1/send");
        req.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
        req.Content = JsonContent.Create(new
        {
            Messages = new[]
            {
                new
                {
                    From = new { Email = _opt.FromAddress, Name = _opt.FromName },
                    To = new[] { new { Email = to } },
                    Subject = subject,
                    HTMLPart = html
                }
            }
        });

        var res = await client.SendAsync(req, ct);
        if (!res.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Mailjet {(int)res.StatusCode}: {await res.Content.ReadAsStringAsync(ct)}");
    }

    private async Task SendWebhookAsync(string to, string subject, string html, CancellationToken ct)
    {
        var client = _http.CreateClient();
        var res = await client.PostAsJsonAsync(_opt.WebhookUrl, new { to, subject, body = html }, ct);
        res.EnsureSuccessStatusCode();
    }
}