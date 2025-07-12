using SmartBook.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartBook.Core.DTOs;

namespace SmartBook.Core.Interfaces
{
    public interface IRecommendationService
    {
        // Generates recommendations based on user preferences and reading history.
        Task<List<BookDisplayModel>> GetContentBasedRecommendationsAsync(int userId, int maxCount = 10);
        
        // Generates recommendations based on collaborative filtering, considering similar users' reading habits.
        Task<List<BookDisplayModel>> GetCollaborativeRecommendationsAsync(int userId, int maxCount = 10);
    }
}
