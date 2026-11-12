using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace BulkyBook.Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly string _email;
        private readonly string _password;

        public EmailSender(IConfiguration config)
        {
            _email = config["Gmail:Email"]
                ?? throw new InvalidOperationException("Gmail:Email is not configured");
            _password = config["Gmail:AppPassword"]
                ?? throw new InvalidOperationException("Gmail:AppPassword is not configured");
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_email, _password)
            };

            var message = new MailMessage
            {
                From = new MailAddress(_email, "BulkyBook Store"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            message.To.Add(email);

            await client.SendMailAsync(message);
        }
    }
}