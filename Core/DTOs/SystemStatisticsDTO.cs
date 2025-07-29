using SmartBook.Core.DTOs;

namespace SmartBook.Core.Models;

public class SystemStatisticsDTO
{
    public int TotalUsers { get; set; }
    public int BannedUsers { get; set; }
    public int TotalBooks { get; set; }
    public int TotalAuthors { get; set; }
    public int TotalCategories { get; set; }
    public int BooksReadCount { get; set; }
    public double AverageBookRating { get; set; }
    public int PendingAuthorEditRequests { get; set; }
    public int PendingCategoryEditRequests { get; set; }
    public int ApprovedAuthorEditRequests { get; set; }
    public int ApprovedCategoryEditRequests { get; set; }
    public int RejectedAuthorEditRequests { get; set; }
    public int RejectedCategoryEditRequests { get; set; }
    public List<TopBookDTO> TopBooks { get; set; } = new();
    public List<TopUserDTO> TopUsers { get; set; } = new();
}