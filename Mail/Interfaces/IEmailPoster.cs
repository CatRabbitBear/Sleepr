namespace Sleepr.Mail.Interfaces;

public interface IEmailPoster
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
