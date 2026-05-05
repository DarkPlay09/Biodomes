using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace BioDomes.Web.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpSettings _settings;

    public SmtpEmailSender(IOptions<SmtpSettings> options)
    {
        _settings = options.Value;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        return SendAsync(
            toEmail: email,
            subject: subject,
            htmlMessage: htmlMessage);
    }

    public Task SendContactEmailAsync(
        string fullName,
        string email,
        string subject,
        string message)
    {
        var safeFullName = WebUtility.HtmlEncode(fullName);
        var safeEmail = WebUtility.HtmlEncode(email);
        var safeSubject = WebUtility.HtmlEncode(subject);
        var safeMessage = WebUtility.HtmlEncode(message)
            .Replace("\r\n", "<br />")
            .Replace("\n", "<br />");

        var htmlMessage = $"""
            <h2>Nouveau message depuis BioDomes</h2>

            <p><strong>Nom :</strong> {safeFullName}</p>
            <p><strong>Adresse e-mail :</strong> {safeEmail}</p>
            <p><strong>Sujet :</strong> {safeSubject}</p>

            <hr />

            <p><strong>Message :</strong></p>
            <p>{safeMessage}</p>
            """;

        var toEmail = string.IsNullOrWhiteSpace(_settings.ContactToEmail)
            ? _settings.FromEmail
            : _settings.ContactToEmail;

        return SendAsync(
            toEmail: toEmail,
            subject: $"[BioDomes] {subject}",
            htmlMessage: htmlMessage,
            replyToEmail: email,
            replyToName: fullName);
    }

    private async Task SendAsync(
        string toEmail,
        string subject,
        string htmlMessage,
        string? replyToEmail = null,
        string? replyToName = null)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true,
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8
        };

        message.To.Add(toEmail);

        if (!string.IsNullOrWhiteSpace(replyToEmail))
        {
            message.ReplyToList.Add(new MailAddress(replyToEmail, replyToName));
        }

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            Credentials = new NetworkCredential(_settings.UserName, _settings.Password),
            EnableSsl = _settings.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        await client.SendMailAsync(message);
    }
}