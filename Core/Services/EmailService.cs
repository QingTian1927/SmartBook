using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using SmartBook.Core.Data;
using SmartBook.Core.Interfaces;

namespace SmartBook.Core.Services;

public class EmailService : IEmailService
{
    private const string SmtpServer = "smtp.gmail.com";
    
    private static EmailService? _instance;
    public static EmailService Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new EmailService();
            }
            return _instance;
        }
    }

    private readonly string? _sender;
    private readonly string? _password;
    
    private EmailService()
    {
        IConfiguration configuration = ContextManager.Configuration;
        _sender = configuration["SMTP:email"];
        _password = configuration["SMTP:password"];
    }
    
    public Task SendEmailAsync(string email, string subject, string message)
    {
        if (_sender is null || _password is null)
        {
            throw new NullReferenceException();
        }
        
        SmtpClient client = new SmtpClient(SmtpServer)
        {
            Port = 587,
            Credentials = new NetworkCredential(_sender, _password),
            EnableSsl = true
        };

        return client.SendMailAsync(new MailMessage(
            _sender, email, subject, message
        ));
    }
}