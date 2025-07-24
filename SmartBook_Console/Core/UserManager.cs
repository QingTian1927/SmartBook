using SmartBook_Console.Models;

namespace SmartBook_Console.Core;

public class UserManager
{
    private static UserManager? _instance;

    public static UserManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new UserManager();
            }

            return _instance;
        }
    }

    private readonly SmartBookContext _db;

    private UserManager()
    {
        _db = new SmartBookContext();
    }

    public User? GetUserById(int userId)
    {
        return _db.Users.Find(userId);
    }
    
    public List<User> GetAllUsers()
    {
        return _db.Users.ToList();
    }
    
}