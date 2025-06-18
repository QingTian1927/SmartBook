using System.IO;
using Microsoft.Extensions.Configuration;
using SmartBook.Core.Models;

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
}