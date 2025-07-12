using Microsoft.EntityFrameworkCore;
using SmartBook.Core.Data;
using SmartBook.Core.DTOs;
using SmartBook.Core.Interfaces;
using SmartBook.Core.Models;

namespace SmartBook.Core.Services;

public class RecommendationService : IRecommendationService
{
    private readonly SmartBookDbContext _db;

    public RecommendationService(SmartBookDbContext db)
    {
        _db = db;
    }

    public async Task<List<BookDisplayModel>> GetContentBasedRecommendationsAsync(int userId, int maxCount = 10)
    {
        var userReadBooks = await _db.UserBooks
            .Where(ub => ub.UserId == userId && ub.IsRead && ub.Rating >= 3) // changed from 4 to 3
            .Include(ub => ub.Book)
            .ThenInclude(b => b.Category)
            .Include(ub => ub.Book.Author)
            .ToListAsync();

        var preferredCategories = userReadBooks
            .GroupBy(ub => ub.Book.CategoryId)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .ToList();

        var preferredAuthors = userReadBooks
            .GroupBy(ub => ub.Book.AuthorId)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .ToList();

        var alreadyReadBookIds = userReadBooks.Select(ub => ub.BookId).ToHashSet();

        var recommendations = await _db.Books
            .Include(b => b.Category)
            .Include(b => b.Author)
            .Where(b => !alreadyReadBookIds.Contains(b.Id) &&
                        (preferredCategories.Contains(b.CategoryId) || preferredAuthors.Contains(b.AuthorId)))
            .Take(maxCount)
            .ToListAsync();

        var result = recommendations.Select(book => new BookDisplayModel
        {
            BookId = book.Id,
            Title = book.Title,
            AuthorName = book.Author.Name,
            CategoryName = book.Category.Name,
            CoverImagePath = "Assets/BookPlaceholder.png",
            Reason = preferredAuthors.Contains(book.AuthorId)
                ? "Because you like books from this author"
                : "Because you like this category"
        }).ToList();

        return result;
    }

    public async Task<List<BookDisplayModel>> GetCollaborativeRecommendationsAsync(int userId, int maxCount = 10)
    {
        var userRatedBooks = await _db.UserBooks
            .Where(ub => ub.UserId == userId && ub.IsRead && ub.Rating >= 3) // changed from 4 to 3
            .Select(ub => ub.BookId)
            .ToListAsync();

        var similarUsers = await _db.UserBooks
            .Where(ub =>
                userRatedBooks.Contains(ub.BookId) && ub.Rating >= 3 && ub.UserId != userId) // changed from 4 to 3
            .GroupBy(ub => ub.UserId)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .ToListAsync();

        var similarUserRecommendations = await _db.UserBooks
            .Where(ub =>
                similarUsers.Contains(ub.UserId) && ub.Rating >= 3 && ub.UserId != userId) // changed from 4 to 3
            .Include(ub => ub.Book)
            .ThenInclude(b => b.Author)
            .Include(ub => ub.Book)
            .ThenInclude(b => b.Category)
            .ToListAsync();

        var alreadyRead = await _db.UserBooks
            .Where(ub => ub.UserId == userId)
            .Select(ub => ub.BookId)
            .ToListAsync();

        var recommendedBooks = similarUserRecommendations
            .Where(ub => !alreadyRead.Contains(ub.BookId))
            .GroupBy(ub => ub.BookId)
            .OrderByDescending(g => g.Average(x => x.Rating ?? 0))
            .Select(g => g.First().Book)
            .Take(maxCount)
            .ToList();

        var result = recommendedBooks.Select(book => new BookDisplayModel
        {
            BookId = book.Id,
            Title = book.Title,
            AuthorName = book.Author.Name,
            CategoryName = book.Category.Name,
            CoverImagePath = "Assets/BookPlaceholder.png",
            Reason = "Because readers similar to you liked this"
        }).ToList();

        return result;
    }
}