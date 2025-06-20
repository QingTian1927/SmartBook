using System.Text;

namespace SmartBook.Utils;

public static class Randomizer
{
    public const string AlphaNumeric = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    
    public static string GenerateRandomString(int length, string allowedChars)
    {
        if (length <= 0) return string.Empty;

        var random = new Random(); // for non-crypto; for security use RNGCryptoServiceProvider
        var result = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            int index = random.Next(allowedChars.Length);
            result.Append(allowedChars[index]);
        }

        return result.ToString();
    }
}