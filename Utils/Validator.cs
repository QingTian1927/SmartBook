using System.Text.RegularExpressions;

namespace SmartBook.Utils;

public static class Validator
{
    public static bool IsValidUsername(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return false;
        }

        string pattern = @"^[A-Za-z0-9_]{4,99}$";
        return Regex.IsMatch(username, pattern);
    }
    
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return false;
        }
        
        string pattern = @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}$";
        return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
    }

    public static bool IsValidPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return false;
        }
        
        string pattern = @"^[A-Za-z0-9!@#$%^&*()_\-+=\[\]{}|\\:;""'<>,.?/~`]{3,32}$";
        return Regex.IsMatch(password, pattern);
    }
}