using Microsoft.EntityFrameworkCore;
using SmartBook.Core.Data;
using SmartBook.Core.Interfaces;
using SmartBook.Core.Models;
using Exception = System.Exception;

namespace SmartBook.Core.Services;

public class AuthService : IAuthService
{
    private readonly SmartBookDbContext _db;

    public AuthService(SmartBookDbContext db)
    {
        _db = db;
    }

    public static bool IsMatchingPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
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

        return IsMatchingPassword(password, user.Password) ? user : null;
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

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        try
        {
            if (!await ExistsUser(user.Email))
            {
                return false;
            }

            _db.Users.Update(user);
            return await _db.SaveChangesAsync() > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] UpdateUserAsync: {ex.Message}");
            return false;
        }
    }
}