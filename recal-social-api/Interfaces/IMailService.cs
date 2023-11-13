using recal_social_api.Models;

namespace recal_social_api.Interfaces;

public interface IMailService
{
    bool SendMail(MailData mailData);
}
