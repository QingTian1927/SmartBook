using Microsoft.EntityFrameworkCore;
using SmartBook.Core.Data;
using SmartBook.Core.DTOs;
using SmartBook.Core.Interfaces;
using SmartBook.Core.Models;

namespace SmartBook.Core.Services;

public class BookService : IBookService
{
    private readonly SmartBookDbContext _db = ContextManager.Context;

    private static BookService? _instance;

    public static BookService Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new BookService();
            }

            return _instance;
        }
    }

    public async Task<IEnumerable<Book>> GetAllBooksAsync(int userId)
    {
        return await _db.UserBooks
            .Where(ub => ub.UserId == userId)
            .Include(ub => ub.Book)
            .ThenInclude(b => b.Author)
            .Include(ub => ub.Book)
            .ThenInclude(b => b.Category)
            .Select(ub => ub.Book)
            .Distinct() // In case user has multiple UserBook entries per Book (if ever)
            .ToListAsync();
    }

    public async Task<Book?> GetBookByIdAsync(int bookId)
    {
        return await _db.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == bookId);
    }

    public async Task AddBookAsync(Book book)
    {
        _db.Books.Add(book);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateBookAsync(Book book)
    {
        _db.Books.Update(book);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteBookAsync(int bookId)
    {
        var book = await _db.Books.FindAsync(bookId);
        if (book != null)
        {
            _db.Books.Remove(book);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Book>> FilterBooksAsync(int userId, string? category = null, bool? isRead = null)
    {
        var query = _db.UserBooks
            .Where(ub => ub.UserId == userId)
            .Include(ub => ub.Book)
            .ThenInclude(b => b.Author)
            .Include(ub => ub.Book)
            .ThenInclude(b => b.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(ub => ub.Book.Category.Name == category);
        }

        if (isRead.HasValue)
        {
            query = query.Where(ub => ub.IsRead == isRead.Value);
        }

        return await query
            .Select(ub => ub.Book)
            .Distinct()
            .ToListAsync();
    }

    public async Task<IEnumerable<BookDisplayModel>> GetAllBooksDisplayAsync()
    {
        var userBooks = await _db.UserBooks
            .Include(ub => ub.Book).ThenInclude(b => b.Author)
            .Include(ub => ub.Book).ThenInclude(b => b.Category)
            .ToListAsync();

        return userBooks.Select(ub => new BookDisplayModel
        {
            UserBookId = ub.Id,
            BookId = ub.Book.Id,
            Title = ub.Book.Title,
            AuthorName = ub.Book.Author.Name,
            CategoryName = ub.Book.Category.Name,
            IsRead = ub.IsRead,
            Rating = ub.Rating,
            CoverImagePath = "" // TODO: Add cover image path here if needed
        }).ToList();
    }

    public async Task<IEnumerable<BookDisplayModel>> GetAllBooksDisplayAsync(int userId)
    {
        var userBooks = await _db.UserBooks
            .Where(ub => ub.UserId == userId)
            .Include(ub => ub.Book).ThenInclude(b => b.Author)
            .Include(ub => ub.Book).ThenInclude(b => b.Category)
            .ToListAsync();

        return userBooks.Select(ub => new BookDisplayModel
        {
            UserBookId = ub.Id,
            BookId = ub.Book.Id,
            Title = ub.Book.Title,
            AuthorName = ub.Book.Author.Name,
            CategoryName = ub.Book.Category.Name,
            IsRead = ub.IsRead,
            Rating = ub.Rating,
            CoverImagePath = "" // TODO: Add cover image path here if needed
        }).ToList();
    }

    public async Task<IEnumerable<BookDisplayModel>> FilterBooksDisplayAsync(string? category = null, bool? isRead = null)
    {
        var query = _db.UserBooks
            .Include(ub => ub.Book).ThenInclude(b => b.Author)
            .Include(ub => ub.Book).ThenInclude(b => b.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(ub => ub.Book.Category.Name == category);
        }

        if (isRead.HasValue)
        {
            query = query.Where(ub => ub.IsRead == isRead.Value);
        }

        var userBooks = await query.ToListAsync();

        return userBooks.Select(ub => new BookDisplayModel
        {
            UserBookId = ub.Id,
            BookId = ub.Book.Id,
            Title = ub.Book.Title,
            AuthorName = ub.Book.Author.Name,
            CategoryName = ub.Book.Category.Name,
            IsRead = ub.IsRead,
            Rating = ub.Rating,
            CoverImagePath = "" // TODO: Add cover image path here if needed
        }).ToList();
    }

    public async Task<IEnumerable<BookDisplayModel>> FilterBooksDisplayAsync(int userId, string? category = null,
        bool? isRead = null)
    {
        var query = _db.UserBooks
            .Where(ub => ub.UserId == userId)
            .Include(ub => ub.Book).ThenInclude(b => b.Author)
            .Include(ub => ub.Book).ThenInclude(b => b.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(ub => ub.Book.Category.Name == category);
        }

        if (isRead.HasValue)
        {
            query = query.Where(ub => ub.IsRead == isRead.Value);
        }

        var userBooks = await query.ToListAsync();

        return userBooks.Select(ub => new BookDisplayModel
        {
            UserBookId = ub.Id,
            BookId = ub.Book.Id,
            Title = ub.Book.Title,
            AuthorName = ub.Book.Author.Name,
            CategoryName = ub.Book.Category.Name,
            IsRead = ub.IsRead,
            Rating = ub.Rating,
            CoverImagePath = "" // TODO: Add cover image path here if needed
        }).ToList();
    }
}