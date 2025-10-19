namespace Shopilent.Infrastructure.Settings;

public class EmailSettings
{
    public string SenderEmail { get; set; } = "noreply@shopilent.com";
    public string SenderName { get; set; } = "Shopilent";
    public string SmtpServer { get; set; } = "smtp.example.com";
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = "username";
    public string SmtpPassword { get; set; } = "password";
    public bool EnableSsl { get; set; } = true;
    public bool SendEmails { get; set; } = true;
    public string AppUrl { get; set; } = "https://shopilent.com";

    // Connection settings
    public int ConnectionTimeoutSeconds { get; set; } = 30;
    public int SendTimeoutSeconds { get; set; } = 30;
    public bool UseSslOnConnect { get; set; } = false;
}
