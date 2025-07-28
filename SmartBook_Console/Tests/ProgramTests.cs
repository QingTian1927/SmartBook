using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartBook_Console;
using SmartBook_Console.Core;
using SmartBook.Core.Data;
using SmartBook.Core.Models;
using Xunit;

namespace SmartBook_Console.Tests
{
    public class ProgramTests : IDisposable
    {
        private readonly BookManager _bookManager;
        private readonly SmartBookDbContext _db;

        public ProgramTests()
        {
            var options = new DbContextOptionsBuilder<SmartBookDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new SmartBookDbContext(options);

            _db.Authors.AddRange(
                new Author { Id = 1, Name = "Author 1" },
                new Author { Id = 2, Name = "Author 2" });
            _db.Categories.AddRange(
                new Category { Id = 1, Name = "Category 1" },
                new Category { Id = 2, Name = "Category 2" });
            _db.SaveChanges();

            _bookManager = new BookManager(_db);
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        [Fact]
        public void AddBook_ValidData_AddsBook()
        {
            bool result = _bookManager.AddBook("Test Book", 1, 1);

            Assert.True(result);
            Assert.Equal(1, _db.Books.Count());

            var book = _db.Books.Include(b => b.Author).Include(b => b.Category).First();
            Assert.Equal("Test Book", book.Title);
            Assert.Equal(1, book.AuthorId);
            Assert.Equal(1, book.CategoryId);
        }

        [Fact]
        public void AddBook_DuplicateTitleAuthor_Fails()
        {
            _bookManager.AddBook("Test Book", 1, 1);
            bool result = _bookManager.AddBook("Test Book", 1, 1);

            Assert.False(result);
            Assert.Equal(1, _db.Books.Count());
        }

        [Fact]
        public void GetAllBooks_ReturnsBooksWithIncludes()
        {
            _bookManager.AddBook("Book One", 1, 1);
            _bookManager.AddBook("Book Two", 2, 2);

            var books = _bookManager.GetAllBooks();

            Assert.Equal(2, books.Count);
            Assert.All(books, b => Assert.NotNull(b.Author));
            Assert.All(books, b => Assert.NotNull(b.Category));
        }

        [Fact]
        public void UpdateBook_ExistingBook_UpdatesSuccessfully()
        {
            _bookManager.AddBook("Old Title", 1, 1);
            var book = _db.Books.First();

            book.Title = "New Title";
            book.AuthorId = 2;
            book.CategoryId = 2;

            bool updated = _bookManager.UpdateBook(book);

            Assert.True(updated);

            var updatedBook = _db.Books.First();
            Assert.Equal("New Title", updatedBook.Title);
            Assert.Equal(2, updatedBook.AuthorId);
            Assert.Equal(2, updatedBook.CategoryId);
        }

        [Fact]
        public void UpdateBook_NonExistingBook_ReturnsFalse()
        {
            var ghostBook = new Book { Id = 999, Title = "Ghost", AuthorId = 1, CategoryId = 1 };
            bool updated = _bookManager.UpdateBook(ghostBook);

            Assert.False(updated);
        }

        [Fact]
        public void DeleteBook_ExistingBook_DeletesBookAndUserBooks()
        {
            _bookManager.AddBook("Book To Delete", 1, 1);
            var book = _db.Books.First();

            _db.UserBooks.Add(new UserBook { BookId = book.Id, UserId = 1, IsRead = false });
            _db.SaveChanges();

            bool deleted = _bookManager.DeleteBook(book.Id);

            Assert.True(deleted);
            Assert.Empty(_db.Books);
            Assert.Empty(_db.UserBooks);
        }

        [Fact]
        public void DeleteBook_NonExistingBook_ReturnsFalse()
        {
            bool deleted = _bookManager.DeleteBook(12345);

            Assert.False(deleted);
        }

        [Fact]
        public void ReadNonEmptyString_ReturnsTrimmedNonEmptyInput()
        {
            var input = new StringReader("\n  \nValid Input  \n");
            Console.SetIn(input);

            string result = InvokeReadNonEmptyString("prompt: ");

            Assert.Equal("Valid Input", result);
        }

        [Fact]
        public void ReadValidInt_ReturnsParsedInt()
        {
            var input = new StringReader("abc\n3.14\n42\n");
            Console.SetIn(input);

            int result = InvokeReadValidInt("prompt: ");

            Assert.Equal(42, result);
        }

        [Theory]
        [InlineData("admin@smartbook.com", "adminpassword", true)]
        [InlineData("admin@smartbook.com", "wrongpassword", false)]
        [InlineData("user@smartbook.com", "adminpassword", false)]
        [InlineData("", "", false)]
        public void Login_ValidatesCredentials(string emailInput, string passwordInput, bool expectedResult)
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Admin:email"] = "admin@smartbook.com",
                    // hash for "adminpassword"
                    ["Admin:password"] = BCrypt.Net.BCrypt.HashPassword("adminpassword")
                }!)
                .Build();

            bool actual = ProgramTestHelper.LoginUsingCredentials(emailInput, passwordInput, config);

            Assert.Equal(expectedResult, actual);
        }

        private string InvokeReadNonEmptyString(string prompt)
        {
            var method = typeof(Program).GetMethod("ReadNonEmptyString", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var result = method?.Invoke(null, new object[] { prompt, false });
            return (string)result!;
        }

        private int InvokeReadValidInt(string prompt)
        {
            var method = typeof(Program).GetMethod("ReadValidInt", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var result = method?.Invoke(null, new object[] { prompt });
            return (int)result!;
        }
    }

    public static class ProgramTestHelper
    {
        public static bool LoginUsingCredentials(string emailInput, string passwordInput, IConfiguration config)
        {
            var adminEmail = config["Admin:email"];
            var adminPasswordHash = config["Admin:password"];

            if (string.IsNullOrEmpty(emailInput) || string.IsNullOrEmpty(passwordInput))
                return false;

            if (emailInput.Equals(adminEmail, StringComparison.OrdinalIgnoreCase) &&
                BCrypt.Net.BCrypt.Verify(passwordInput, adminPasswordHash))
            {
                return true;
            }

            return false;
        }
    }
}
