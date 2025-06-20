using System.IO;
using Microsoft.Extensions.Configuration;
using SmartBook.Core.Models;
using SmartBook.Core.Services;

namespace SmartBook.Core.Data;

public static class ContextManager
{
    private static SmartBookDbContext? _context;

    public static SmartBookDbContext Context
    {
        get
        {
            if (_context == null)
            {
                _context = new SmartBookDbContext();
            }

            return _context;
        }
    }

    public static IConfiguration Configuration
    {
        get
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            return builder.Build();
        }
    }

    public static bool IsAdmin { get; set; }
    public static User? CurrentUser { get; set; }

    private static string? _passwordResetCode;
    private static DateTime? _passwordResetCreatedDate;

    public static string? PasswordResetEmail { get; set; }
    public static string? PasswordResetCode
    {
        get
        {
            if (!_passwordResetCreatedDate.HasValue)
            {
                return null;
            }

            if (_passwordResetCreatedDate.Value.AddMinutes(EmailService.PasswordResetCodeExpirationMinutes) <=
                DateTime.UtcNow)
            {
                _passwordResetCode = null;
                _passwordResetCreatedDate = null;
                return null;
            }

            return _passwordResetCode;
        }

        set
        {
            _passwordResetCode = value;
            _passwordResetCreatedDate = DateTime.UtcNow;
        }
    }
}