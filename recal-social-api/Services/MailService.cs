using MailKit.Net.Smtp;
using MimeKit;
using recal_social_api.Interfaces;
using recal_social_api.Models;

namespace recal_social_api.Services;

public class MailService : IMailService
{
    public bool SendMail(MailData mailData)
    {
        try
        {
            using var emailMessage = new MimeMessage();

            var emailFrom = new MailboxAddress(GlobalVars.MailSenderName, GlobalVars.MailSenderEmail);
            emailMessage.From.Add(emailFrom);

            var emailTo = new MailboxAddress(mailData.RecipientName, mailData.RecipientEmail);
            emailMessage.To.Add(emailTo);

            emailMessage.Subject = mailData.EmailSubject;

            var emailBodyBuilder = new BodyBuilder
            {
                TextBody = mailData.EmailBody
            };
            emailMessage.Body = emailBodyBuilder.ToMessageBody();

            using var mailClient = new SmtpClient();

            mailClient.Connect(GlobalVars.MailServer, GlobalVars.MailServerPort, MailKit.Security.SecureSocketOptions.StartTls);
            mailClient.Authenticate(GlobalVars.MailUsername, GlobalVars.MailPassword);
            mailClient.Send(emailMessage);
            mailClient.Disconnect(true);

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
}
