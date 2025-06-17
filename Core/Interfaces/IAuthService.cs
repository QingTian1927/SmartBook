using SmartBook.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Core.Interfaces
{
    public interface IAuthService
    {
        Task<bool> ExistsUser(string email);
        Task<User?> AuthenticateAsync(string email, string password);
        Task<bool> RegisterUserAsync(User user);
        Task<User?> GetUserByIdAsync(int userId);
    }
}
