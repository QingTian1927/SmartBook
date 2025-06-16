using SmartBook.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Core.Interfaces
{
    public interface IRecommendationService
    {
        Task<Book?> GetNextRecommendedBookAsync(int userId);
    }
}
