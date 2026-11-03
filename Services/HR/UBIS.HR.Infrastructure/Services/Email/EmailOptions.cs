namespace Ubis.Hr.Api.Services.Email;

public class EmailOptions
{
    public const string Section = "Email";

    public string Provider { get; set; } = "None";
    public string FromName { get; set; } = "UBIS HR";
    public string FromAddress { get; set; } = "";

    // SMTP
    public string SmtpHost { get; set; } = "";
    public int SmtpPort { get; set; } = 587;
    public bool UseStartTls { get; set; } = true;
    public string SmtpUser { get; set; } = "";
    public string SmtpPassword { get; set; } = "";

    // Mailjet REST API
    public string MailjetApiKey { get; set; } = "";
    public string MailjetSecretKey { get; set; } = "";

    // Webhook
    public string WebhookUrl { get; set; } = "";

    public string WebBaseUrl { get; set; } = "";
}