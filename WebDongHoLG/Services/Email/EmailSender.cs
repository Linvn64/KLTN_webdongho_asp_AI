using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace WebDongHoLG.Services.Email
{
    public class EmailSender : IEmailSender
    {

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using (var client = new SmtpClient("sandbox.smtp.mailtrap.io", 2525))
            {
                client.Credentials = new NetworkCredential("6b9b2fe2460cf4", "a988dc967cc892");
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("noreply@lgwatch.com", "LGWATCH Luxury"),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
            }
        }
    }
}
