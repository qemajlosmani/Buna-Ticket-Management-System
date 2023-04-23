using Btms.API.Helpers;
using Btms.Interfaces;
using Btms.Models;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System.Net.Mime;

namespace Btms.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly AppSettings _appSettings;
        public EmailService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }


        public GeneralApiResponse SendEmail(string from, string from_email, string[] toEmail, string[]? toCC, string[]? toBCC, string? subject, string? text, List<IFormFile>? attachment = null)
        {
            var res = new GeneralApiResponse();

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(from, from_email));

                foreach (var e in toEmail)
                {
                    message.To.Add(MailboxAddress.Parse(e));
                }

                if (toCC != null)
                {
                    foreach (var c in toCC)
                    {
                        message.Cc.Add(MailboxAddress.Parse(c));
                    }
                }

                if (toBCC != null)
                {
                    foreach (var bcc in toBCC)
                    {
                        message.Bcc.Add(MailboxAddress.Parse(bcc));
                    }
                }

                message.Subject = subject;
                message.Body = new TextPart(TextFormat.Html) { Text = text ?? "" };

                var builder = new BodyBuilder();
                builder.HtmlBody = text ?? "";

                byte[] fileBytes;
                if (attachment != null && attachment.Count > 0)
                {
                    foreach (var file in attachment)
                    {
                        if (file.Length > 0)
                        {
                            using (var ms = new MemoryStream())
                            {
                                file.CopyTo(ms);
                                fileBytes = ms.ToArray();
                            }
                            builder.Attachments.Add(file.FileName, fileBytes, MimeKit.ContentType.Parse(MediaTypeNames.Application.Octet));
                        }
                    }
                }

                message.Body = builder.ToMessageBody();

                // send email
                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                smtp.Connect(_appSettings.SmtpHost, _appSettings.SmtpPort, SecureSocketOptions.StartTls);
                smtp.Authenticate(_appSettings.SmtpUser, _appSettings.SmtpPass);
                smtp.Send(message);
                smtp.Disconnect(true);

                res.response_message = "Email sent successfully.";
                return res;
            }
            catch (Exception ex)
            {
                throw new AppException("Error sending email!");
            }
        }

    }
}
