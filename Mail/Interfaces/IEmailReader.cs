namespace Sleepr.Mail.Interfaces;

public interface IEmailReader
{
    Task<IReadOnlyList<EmailMessage>> FetchRecentAsync(int count = 5, CancellationToken cancellationToken = default);
}
