using SmartBook.Core.Models;

namespace SmartBook.Core.Interfaces;

public interface IStatisticsService
{ 
    Task<SystemStatisticsDTO> GetStatisticsAsync(int topBooks = 5, int topUsers = 5);
}