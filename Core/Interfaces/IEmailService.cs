namespace SmartBook.Core.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string message);
    Task SendAccountCreatedEmailAsync(string email);
    Task SendPasswordResetEmailAsync(string email, string resetCode);

    Task SendEditRequestResultEmailAsync(
        string userEmail,
        string requestType,
        string itemName,
        string status,
        string? reviewComment = null);
}