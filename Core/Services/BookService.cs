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

    public async Task AddUserBookAsync(UserBook userBook)
    {
        _db.UserBooks.Add(userBook);
        await _db.SaveChangesAsync();
    }

    public async Task<int?> CalculateAverageRatingAsync(int bookId)
    {
        var query = _db.UserBooks
            .Where(ub => ub.BookId == bookId)
            .AsQueryable();

        return (int?) await query
            .AverageAsync(ub => ub.Rating);
    }

    public async Task<IEnumerable<Book>> FilterBooksAsync(int userId, int? categoryId = null, bool? isRead = null)
    {
        var query = _db.UserBooks
            .Where(ub => ub.UserId == userId)
            .Include(ub => ub.Book)
            .ThenInclude(b => b.Author)
            .Include(ub => ub.Book)
            .ThenInclude(b => b.Category)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(ub => ub.Book.Category.Id == categoryId);
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

    public async Task<IEnumerable<BookDisplayModel>> FilterBooksDisplayAsync(int? categoryId = null)
    {
        if (categoryId is 0)
        {
            return await GetAllBooksDisplayAsync();
        }

        var query = _db.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(b => b.CategoryId == categoryId);
        }

        var books = await query.ToListAsync();

        var result = new List<BookDisplayModel>();

        foreach (var b in books)
        {
            var avgRating = await CalculateAverageRatingAsync(b.Id);
            result.Add(new BookDisplayModel
            {
                BookId = b.Id,
                Title = b.Title,
                AuthorName = b.Author.Name,
                CategoryName = b.Category.Name,
                IsRead = false,
                Rating = avgRating,
                UserBookId = 0,
                CoverImagePath = ""
            });
        }

        return result;
    }

    public async Task<IEnumerable<BookDisplayModel>> FilterBooksDisplayAsync(int userId, int? categoryId = null,
        bool? isRead = null)
    {
        if (categoryId is 0)
        {
            return await GetAllBooksDisplayAsync(userId);
        }
        
        var query = _db.UserBooks
            .Where(ub => ub.UserId == userId)
            .Include(ub => ub.Book)
            .ThenInclude(b => b.Author)
            .Include(ub => ub.Book)
            .ThenInclude(b => b.Category)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(ub => ub.Book.Category.Id == categoryId);
        }

        if (isRead.HasValue)
        {
            query = query.Where(ub => ub.IsRead == isRead.Value);
        }

        return await query
            .Select(ub => new BookDisplayModel
            {
                BookId = ub.BookId,
                AuthorName = ub.Book.Author.Name,
                CategoryName = ub.Book.Category.Name,
                IsRead = ub.IsRead,
                Rating = ub.Rating,
                Title = ub.Book.Title,
                UserBookId = ub.BookId,
                CoverImagePath = "" // TODO: Add cover image path here if needed
            })
            .Distinct()
            .ToListAsync();
    }

    public async Task<IEnumerable<BookDisplayModel>> SearchBooksDisplayAsync(string? keyword, int? categoryId = null)
    {
        if (string.IsNullOrEmpty(keyword))
        {
            return await FilterBooksDisplayAsync(categoryId);
        }

        var query = _db.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .AsQueryable();

        if (categoryId.HasValue && categoryId != 0)
        {
            query = query.Where(b => b.CategoryId == categoryId);
        }

        var books = await query
            .Where(b => b.Title.ToLower().Contains(keyword.ToLower()))
            .ToListAsync();

        var result = new List<BookDisplayModel>();

        foreach (var b in books)
        {
            var avgRating = await CalculateAverageRatingAsync(b.Id);
            result.Add(new BookDisplayModel
            {
                BookId = b.Id,
                Title = b.Title,
                AuthorName = b.Author.Name,
                CategoryName = b.Category.Name,
                IsRead = false,
                Rating = avgRating,
                UserBookId = 0,
                CoverImagePath = ""
            });
        }

        return result;
    }

    public async Task<IEnumerable<BookDisplayModel>> SearchBooksDisplayAsync(int userId, string? keyword, int? categoryId = null, bool? isRead = null)
    {
        if (string.IsNullOrEmpty(keyword))
        {
            return await FilterBooksDisplayAsync(userId, categoryId, isRead);
        }
        
        var query = _db.UserBooks
            .Where(ub => ub.UserId == userId)
            .Include(ub => ub.Book)
            .ThenInclude(b => b.Author)
            .Include(ub => ub.Book)
            .ThenInclude(b => b.Category)
            .AsQueryable();

        if (categoryId.HasValue && categoryId != 0)
        {
            query = query.Where(ub => ub.Book.Category.Id == categoryId);
        }

        query = query.Where(ub => ub.Book.Title.ToLower().Contains(keyword.ToLower()));
        
        return await query
            .Select(ub => new BookDisplayModel
            {
                BookId = ub.BookId,
                AuthorName = ub.Book.Author.Name,
                CategoryName = ub.Book.Category.Name,
                IsRead = ub.IsRead,
                Rating = ub.Rating,
                Title = ub.Book.Title,
                UserBookId = ub.BookId,
                CoverImagePath = "" // TODO: Add cover image path here if needed
            })
            .Distinct()
            .ToListAsync();
    }

    public async Task<IEnumerable<BookDisplayModel>> GetAllBooksDisplayAsync()
    {
        var books = await _db.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .ToListAsync();
        
        return books.Select(b => new BookDisplayModel
        {
            UserBookId = b.Id,
            BookId = b.Id,
            Title = b.Title,
            AuthorName = b.Author.Name,
            CategoryName = b.Category.Name,
            IsRead = false,
            Rating = -1,
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

    public async Task<IEnumerable<CategoryDisplayModel>> GetAllCategoriesAsync()
    {
        return await _db.Categories.Select(c => new CategoryDisplayModel
            {
                Id = c.Id,
                Name = c.Name
            })
            .OrderBy(c => c.Name)
            .Distinct()
            .ToListAsync();
    }

    public async Task<IEnumerable<AuthorDisplayModel>> GetAllAuthorsAsync()
    {
        return await _db.Authors.Select(a => new AuthorDisplayModel()
        {
            Id = a.Id,
            Name = a.Name,
            Bio = a.Bio
        })
        .OrderBy(a => a.Name)
        .Distinct()
        .ToListAsync();
    }

    public async Task<Author?> GetAuthorByIdAsync(int authorId)
    {
        return await _db.Authors.FirstOrDefaultAsync(a => a.Id == authorId);
    }

    public async Task<Category?> GetCategoryByIdAsync(int categoryId)
    {
        return await _db.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
    }
}