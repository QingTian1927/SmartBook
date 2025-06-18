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
        Task<IEnumerable<Book>> FilterBooksAsync(int userId, string? category = null, bool? isRead = null);
        Task<IEnumerable<BookDisplayModel>> GetAllBooksDisplayAsync();
        Task<IEnumerable<BookDisplayModel>> GetAllBooksDisplayAsync(int userId);
        Task<IEnumerable<BookDisplayModel>> FilterBooksDisplayAsync(string? category = null, bool? isRead = null);
        Task<IEnumerable<BookDisplayModel>> FilterBooksDisplayAsync(int userId, string? category = null, bool? isRead = null);
    }
}
