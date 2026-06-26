using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace Career_Guidance_Platform.Service;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(
        string email,
        string subject,
        string htmlMessage)
    {
        var smtpServer =
            _configuration["EmailSettings:SmtpServer"];

        var port =
            int.Parse(
                _configuration["EmailSettings:Port"]!);

        var senderEmail =
            _configuration["EmailSettings:SenderEmail"];

        var password =
            _configuration["EmailSettings:Password"];

        using var client =
            new SmtpClient(smtpServer, port);

        client.EnableSsl = true;

        client.Credentials =
            new NetworkCredential(
                senderEmail,
                password);

        var mail = new MailMessage
        {
            From = new MailAddress(senderEmail!),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };

        mail.To.Add(email);

        await client.SendMailAsync(mail);
    }
}