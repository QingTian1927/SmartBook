using Microsoft.EntityFrameworkCore;
using SmartBook.Core.Data;
using SmartBook.Core.Interfaces;
using SmartBook.Core.Models;

namespace SmartBook.Core.Services;

public class AuthService : IAuthService
{
    private readonly SmartBookDbContext _db = ContextManager.Context;
    
    private static AuthService? _instance;
    public static AuthService Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new AuthService();
            }
            return _instance;
        }
    }

    public async Task<bool> ExistsUser(string email)
    {
        return await _db.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        User? user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
        {
            return null;
        }

        bool isValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
        return isValid ? user : null;
    }

    public async Task<bool> RegisterUserAsync(User user)
    {
        try
        {
            if (await ExistsUser(user.Email))
            {
                return false;
            }

            _db.Users.Add(user);
            return await _db.SaveChangesAsync() > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] RegisterUserAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }
}