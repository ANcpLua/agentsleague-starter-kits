using System.ComponentModel;
using System.Net;
using System.Net.Mail;

namespace CertPrepAgents.Tools;

internal static class SendEmailTool
{
    [Description("Sends an email with the study plan or remediation plan to the student.")]
    public static async Task<string> SendEmail(
        [Description("Recipient email address")] string to,
        [Description("Email subject line")] string subject,
        [Description("Email body in plain text")] string body,
        IConfiguration configuration)
    {
        var smtpHost = configuration["Smtp:Host"];
        var smtpPort = configuration.GetValue("Smtp:Port", 587);
        var smtpUser = configuration["Smtp:Username"];
        var smtpPass = configuration["Smtp:Password"];
        var fromAddress = configuration["Smtp:From"] ?? "certprep@agentsleague.dev";

        if (string.IsNullOrEmpty(smtpHost))
        {
            // Fallback: log the email instead of sending when SMTP is not configured
            var timestamp = TimeProvider.System.GetUtcNow().ToString("O");
            return $"[{timestamp}] Email logged (SMTP not configured). To: {to}, Subject: {subject}, Body length: {body.Length} chars. Configure Smtp:Host in appsettings to enable real delivery.";
        }

        try
        {
            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var message = new MailMessage(fromAddress, to, subject, body)
            {
                IsBodyHtml = false
            };

            await client.SendMailAsync(message);
            return $"Email sent to {to} with subject: {subject}";
        }
        catch (SmtpException ex)
        {
            return $"Failed to send email to {to}: {ex.Message}. The email content was preserved for retry.";
        }
    }
}
