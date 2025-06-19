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
        _sender = Environment.GetEnvironmentVariable("SMTP_USER");
        _password = Environment.GetEnvironmentVariable("SMTP_PASS");
        
        Console.WriteLine($"SMTP_USER: {_sender}, SMTP_PASSWORD: {_password}");
    }

    public async Task SendEmailAsync(string email, string subject, string message)
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

        await client.SendMailAsync(new MailMessage(
            _sender, email, subject, message
        ));
    }

    public async Task SendAccountCreatedEmailAsync(string email)
    {
        if (_sender is null || _password is null)
        {
            throw new NullReferenceException();
        }

        using MailMessage mail = new MailMessage();
        mail.From = new MailAddress(_sender, "SmartBook");
        mail.To.Add(email);
        mail.Subject = "Your SmartBook Account Has Been Created";
        mail.IsBodyHtml = true;

        mail.Body = $@"
            <!DOCTYPE html>
            <html>
            <head>
              <meta charset=""UTF-8"">
              <title>Account Registration</title>
              <style>
                body {{
                  font-family: Arial, sans-serif;
                  color: #333333;
                  background-color: #f8f8f8;
                  margin: 0;
                  padding: 20px;
                }}
                .container {{
                  max-width: 600px;
                  margin: auto;
                  background-color: #ffffff;
                  padding: 30px;
                  border: 1px solid #dddddd;
                  border-radius: 6px;
                  box-shadow: 0 0 10px rgba(0,0,0,0.05);
                }}
                h2 {{
                  color: #1C2A39;
                }}
                .footer {{
                  font-size: 12px;
                  color: #999999;
                  margin-top: 30px;
                  text-align: center;
                }}
                .support-link {{
                  color: #007BFF;
                  text-decoration: none;
                }}
              </style>
            </head>
            <body>
              <div class=""container"">
                <h2>Account Successfully Created</h2>
                <p>Dear user,</p>
                <p>An account has been successfully registered using this email address: <strong>[User Email]</strong>.</p>
                <p>If you created this account, no further action is required. You can now log in and start using our services.</p>
                <p>If you did not intend to create this account, or believe this was done in error, please contact our support team immediately:</p>
                <p><a href=""mailto:[Support Email]"" class=""support-link"">[Support Email]</a></p>
                <p>Thank you,<br>The SmartBook Team</p>

                <div class=""footer"">
                  &copy; 2025 SmartBook. All rights reserved.
                </div>
              </div>
            </body>
            </html>
        ";

        SmtpClient client = new SmtpClient(SmtpServer)
        {
            Port = 587,
            Credentials = new NetworkCredential(_sender, _password),
            EnableSsl = true
        };

        await client.SendMailAsync(mail);
    }
}