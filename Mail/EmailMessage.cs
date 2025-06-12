namespace Sleepr.Mail;

public record EmailMessage(string Subject, string From, DateTimeOffset Date, string Body);