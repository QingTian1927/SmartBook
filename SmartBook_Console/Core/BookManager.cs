using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SmartBook.Core;
using SmartBook.Core.Data;
using SmartBook.Core.Models;

namespace SmartBook_Console.Core
{
    public class BookManager
    {
        private readonly SmartBookDbContext _dbContext;

        public BookManager(SmartBookDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public BookManager()
        {
            _dbContext = new SmartBookDbContext();
        }

        public List<Book> GetAllBooks()
        {
            return _dbContext.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .ToList();
        }

        public bool AddBook(string title, int authorId, int categoryId)
        {
            if (_dbContext.Books.Any(b => b.Title == title && b.AuthorId == authorId))
                return false;

            var newBook = new Book
            {
                Title = title,
                AuthorId = authorId,
                CategoryId = categoryId
            };

            _dbContext.Books.Add(newBook);
            _dbContext.SaveChanges();
            return true;
        }
        
        public bool UpdateBook(Book updatedBook)
        {
            var existingBook = _dbContext.Books.Find(updatedBook.Id);
            if (existingBook == null)
                return false;

            existingBook.Title = updatedBook.Title;
            existingBook.AuthorId = updatedBook.AuthorId;
            existingBook.CategoryId = updatedBook.CategoryId;

            _dbContext.SaveChanges();
            return true;
        }


        public List<Book> SearchBooksByTitle(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<Book>();

            var lowered = keyword.ToLower();
            return _dbContext.Books
                .Where(b => b.Title.ToLower().Contains(lowered))
                .Include(b => b.Author)
                .Include(b => b.Category)
                .ToList();
        }
        
        public bool DeleteBook(int bookId)
        {
            var book = _dbContext.Books.Find(bookId);
            if (book == null)
            {
                return false;
            }

            var userBooks = _dbContext.UserBooks.Where(ub => ub.BookId == bookId).ToList();
            if (userBooks.Count > 0)
            {
                _dbContext.UserBooks.RemoveRange(userBooks);
            }

            _dbContext.Books.Remove(book);
            _dbContext.SaveChanges();
            return true;
        }

    }
}
