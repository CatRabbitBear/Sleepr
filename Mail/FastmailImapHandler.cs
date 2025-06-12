using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using Sleepr.Mail.Interfaces;

namespace Sleepr.Mail;

public class FastmailImapHandler : IEmailReader, IEmailPoster
{
    private readonly string _username;
    private readonly string _appPassword;

    public FastmailImapHandler(string username, string appPassword)
    {
        _username = username ?? throw new ArgumentNullException(nameof(username));
        _appPassword = appPassword ?? throw new ArgumentNullException(nameof(appPassword));
    }

    public async Task<IReadOnlyList<EmailMessage>> FetchRecentAsync(int count = 5, CancellationToken cancellationToken = default)
    {
        using var client = new ImapClient();
        client.ServerCertificateValidationCallback = (s, c, h, e) => true;
        await client.ConnectAsync("imap.fastmail.com", 993, SecureSocketOptions.SslOnConnect, cancellationToken);
        await client.AuthenticateAsync(_username, _appPassword, cancellationToken);

        var inbox = client.Inbox;
        await inbox.OpenAsync(FolderAccess.ReadOnly, cancellationToken);

        var uids = await inbox.SearchAsync(SearchQuery.All, cancellationToken);
        var recent = uids.TakeLast(count).ToList();

        var results = new List<EmailMessage>();

        foreach (var uid in recent)
        {
            var message = await inbox.GetMessageAsync(uid, cancellationToken);
            results.Add(new EmailMessage(
                message.Subject ?? "(No subject)",
                message.From.Mailboxes.FirstOrDefault()?.Address ?? "(Unknown)",
                message.Date,
                message.TextBody ?? message.HtmlBody ?? ""
            ));
        }

        await client.DisconnectAsync(true, cancellationToken);
        return results;
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_username));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.fastmail.com", 587, SecureSocketOptions.StartTls, cancellationToken);
        await client.AuthenticateAsync(_username, _appPassword, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
