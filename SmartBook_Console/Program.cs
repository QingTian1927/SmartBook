using SmartBook_Console.Models;

namespace SmartBook_Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var db = new SmartBookContext();
            foreach (var user in db.Users.ToList())
            {
                Console.WriteLine($"User: {user.Username}, Email: {user.Email}");
            }
        }
    }
}
