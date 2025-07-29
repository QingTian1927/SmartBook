using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartBook.Core.Data;
using SmartBook.Core.DTOs;
using SmartBook.Core.Interfaces;
using SmartBook.Core.Models;

namespace SmartBook.Core.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly SmartBookDbContext _db;

        public StatisticsService(SmartBookDbContext db)
        {
            _db = db;
        }

        public async Task<SystemStatisticsDTO> GetStatisticsAsync(int topBooks = 5, int topUsers = 5)
        {
            // Run queries sequentially (no parallel calls on the same DbContext)
            var totalUsers = await _db.Users.CountAsync();
            var bannedUsers = await _db.Users.CountAsync(u => u.IsBanned);
            var totalBooks = await _db.Books.CountAsync();
            var totalAuthors = await _db.Authors.CountAsync();
            var totalCategories = await _db.Categories.CountAsync();
            var booksReadCount = await _db.UserBooks.CountAsync(ub => ub.IsRead);
            var avgBookRatingNullable = await _db.UserBooks
                .Where(ub => ub.Rating != null)
                .AverageAsync(ub => (double?)ub.Rating);
            double avgBookRating = avgBookRatingNullable ?? 0.0;

            var pendingAuthorEditRequests = await _db.AuthorEditRequests.CountAsync(r => r.Status == "Pending");
            var approvedAuthorEditRequests = await _db.AuthorEditRequests.CountAsync(r => r.Status == "Approved");
            var rejectedAuthorEditRequests = await _db.AuthorEditRequests.CountAsync(r => r.Status == "Rejected");

            var pendingCategoryEditRequests = await _db.CategoryEditRequests.CountAsync(r => r.Status == "Pending");
            var approvedCategoryEditRequests = await _db.CategoryEditRequests.CountAsync(r => r.Status == "Approved");
            var rejectedCategoryEditRequests = await _db.CategoryEditRequests.CountAsync(r => r.Status == "Rejected");

            // Top books by reader count (and rating)
            var topBooksList = await _db.Books
                .Select(b => new TopBookDTO()
                {
                    BookId = b.Id,
                    Title = b.Title,
                    AuthorName = b.Author.Name,
                    ReaderCount = b.UserBooks.Count(ub => ub.IsRead),
                    AverageRating = b.UserBooks.Where(ub => ub.Rating != null)
                        .Average(ub => (double?)ub.Rating) ?? 0.0
                })
                .OrderByDescending(b => b.ReaderCount)
                .ThenByDescending(b => b.AverageRating)
                .Take(topBooks)
                .ToListAsync();

            // Top users by books read
            var topUsersList = await _db.Users
                .Select(u => new TopUserDTO()
                {
                    UserId = u.Id,
                    Username = u.Username,
                    ReadBooksCount = u.UserBooks.Count(ub => ub.IsRead)
                })
                .OrderByDescending(u => u.ReadBooksCount)
                .Take(topUsers)
                .ToListAsync();

            // Construct the result DTO
            return new SystemStatisticsDTO()
            {
                TotalUsers = totalUsers,
                BannedUsers = bannedUsers,
                TotalBooks = totalBooks,
                TotalAuthors = totalAuthors,
                TotalCategories = totalCategories,
                BooksReadCount = booksReadCount,
                AverageBookRating = Math.Round(avgBookRating, 2),
                PendingAuthorEditRequests = pendingAuthorEditRequests,
                ApprovedAuthorEditRequests = approvedAuthorEditRequests,
                RejectedAuthorEditRequests = rejectedAuthorEditRequests,
                PendingCategoryEditRequests = pendingCategoryEditRequests,
                ApprovedCategoryEditRequests = approvedCategoryEditRequests,
                RejectedCategoryEditRequests = rejectedCategoryEditRequests,
                TopBooks = topBooksList,
                TopUsers = topUsersList
            };
        }
    }
}
