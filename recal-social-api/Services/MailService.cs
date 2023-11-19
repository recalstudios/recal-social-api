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
            // Create email message
            using var emailMessage = new MimeMessage();

            // Set sender
            var emailFrom = new MailboxAddress(GlobalVars.MailSenderName, GlobalVars.MailSenderEmail);
            emailMessage.From.Add(emailFrom);

            // Set recipient
            var emailTo = new MailboxAddress(mailData.RecipientName, mailData.RecipientEmail);
            emailMessage.To.Add(emailTo);

            // Set subject
            emailMessage.Subject = mailData.EmailSubject;

            // Create both html and text body
            var emailBodyBuilder = new BodyBuilder
            {
                HtmlBody = mailData.EmailBody,
                TextBody = mailData.EmailBody
            };
            emailMessage.Body = emailBodyBuilder.ToMessageBody();

            // Send the email
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
