using SmartBook.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartBook.Core.DTOs;

namespace SmartBook.Core.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<Book>> GetAllBooksAsync(int userId);
        Task<Book?> GetBookByIdAsync(int bookId);

        Task AddBookAsync(Book book);
        Task UpdateBookAsync(Book book);
        Task DeleteBookAsync(int bookId);

        Task AddUserBookAsync(UserBook userBook);

        Task<int?> CalculateAverageRatingAsync(int bookId);
        Task<IEnumerable<Book>> FilterBooksAsync(int userId, int? categoryId = null, bool? isRead = null);

        Task<IEnumerable<BookDisplayModel>> FilterBooksDisplayAsync(int? categoryId = null);

        Task<IEnumerable<BookDisplayModel>> FilterBooksDisplayAsync(int userId, int? categoryId = null,
            bool? isRead = null);

        Task<IEnumerable<BookDisplayModel>> SearchBooksDisplayAsync(string? keyword, int? categoryId = null);

        Task<IEnumerable<BookDisplayModel>> SearchBooksDisplayAsync(int userId, string? keyword, int? categoryId = null,
            bool? isRead = null);

        Task<IEnumerable<BookDisplayModel>> GetAllBooksDisplayAsync();
        Task<IEnumerable<BookDisplayModel>> GetAllBooksDisplayAsync(int userId);

        Task<IEnumerable<CategoryDisplayModel>> GetAllCategoriesAsync();
        Task<IEnumerable<AuthorDisplayModel>> GetAllAuthorsAsync();

        Task<Author?> GetAuthorByIdAsync(int authorId);
        Task<Category?> GetCategoryByIdAsync(int categoryId);
    }
}