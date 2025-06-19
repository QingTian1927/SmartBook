namespace SmartBook.Core.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string message);
    Task SendAccountCreatedEmailAsync(string email);
    
}