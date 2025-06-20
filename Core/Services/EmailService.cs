using System.Net;
using System.Net.Mail;
using SmartBook.Core.Interfaces;

namespace SmartBook.Core.Services;

public class EmailService : IEmailService
{
    public const int PasswordResetCodeLength = 6;
    public const int PasswordResetCodeExpirationMinutes = 15;

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

    private SmtpClient _smtpClient;
    private readonly string? _sender;
    private readonly string? _password;

    private EmailService()
    {
        _sender = Environment.GetEnvironmentVariable("SMTP_USER");
        _password = Environment.GetEnvironmentVariable("SMTP_PASS");
        _smtpClient = new SmtpClient(SmtpServer)
        {
            Port = 587,
            Credentials = new NetworkCredential(_sender, _password),
            EnableSsl = true
        };
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        if (_sender is null || _password is null)
        {
            throw new NullReferenceException();
        }

        await _smtpClient.SendMailAsync(new MailMessage(
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
                <p>An account has been successfully registered using this email address: <strong>{email}</strong>.</p>
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

        await _smtpClient.SendMailAsync(mail);
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetCode)
    {
        if (_sender is null || _password is null)
        {
            throw new NullReferenceException("SMTP credentials are not configured properly.");
        }

        using MailMessage mail = new MailMessage();
        mail.From = new MailAddress(_sender, "SmartBook");
        mail.To.Add(email);
        mail.Subject = "SmartBook Password Reset";
        mail.IsBodyHtml = true;

        mail.Body = $@"
        <!DOCTYPE html>
        <html>
        <head>
          <meta charset=""UTF-8"">
          <title>Password Reset</title>
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
            .code {{
              font-size: 20px;
              font-weight: bold;
              background-color: #f0f0f0;
              padding: 10px 20px;
              border-radius: 5px;
              display: inline-block;
            }}
            .footer {{
              font-size: 12px;
              color: #999999;
              margin-top: 30px;
              text-align: center;
            }}
          </style>
        </head>
        <body>
          <div class=""container"">
            <h2>Password Reset Request</h2>
            <p>Dear user,</p>
            <p>We received a request to reset your SmartBook account password.</p>
            <p>Please enter the following confirmation code in the app to continue:</p>
            <p class=""code"">{resetCode}</p>
            <p>Please note that the above code will expire after 15 minutes from the time this email was sent.</p>
            <p>If you did not request a password reset, you can safely ignore this email.</p>
            <p>Thank you,<br>The SmartBook Team</p>
            <div class=""footer"">
              &copy; 2025 SmartBook. All rights reserved.
            </div>
          </div>
        </body>
        </html>
    ";

        await _smtpClient.SendMailAsync(mail);
    }
}