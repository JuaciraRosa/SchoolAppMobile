using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MauiAppSchool.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _opt;
        public SmtpEmailSender(IOptions<SmtpOptions> opt) => _opt = opt.Value;

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            using var msg = new MailMessage(_opt.From, to, subject, htmlBody) { IsBodyHtml = true };
            using var smtp = new SmtpClient(_opt.Host, _opt.Port)
            {
                EnableSsl = _opt.UseStartTls,
                Credentials = new NetworkCredential(_opt.User, _opt.Pass)
            };
            await smtp.SendMailAsync(msg);
        }
    }

}
