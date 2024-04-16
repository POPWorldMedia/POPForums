namespace PopForums.Services.Interfaces;

public interface IMailingListService
{
    void MailUsers(string subject, string body, string htmlBody, Func<User, string> unsubscribeLinkGenerator);
}
