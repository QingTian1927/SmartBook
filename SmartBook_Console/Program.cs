using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using BCrypt.Net;
using SmartBook_Console.Core;
using SmartBook_Console.Models;

namespace SmartBook_Console
{
    public class Program
    {
        private static IConfigurationRoot _configuration = null!;
        private static readonly BookManager BookManager = new BookManager();

        static void Main(string[] args)
        {
            LoadConfiguration();

            if (!Login())
            {
                Console.WriteLine("Failed login. Exiting.");
                return;
            }

            ShowMainMenu();
        }

        private static void LoadConfiguration()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
        }

        private static bool Login()
        {
            Console.WriteLine("=== SmartBook Admin Login ===");

            var adminEmail = _configuration["Admin:email"];
            var adminPasswordHash = _configuration["Admin:password"];

            while (true)
            {
                Console.Write("Email: ");
                var emailInput = Console.ReadLine()?.Trim();

                Console.Write("Password: ");
                var passInput = ReadPassword();

                if (string.IsNullOrEmpty(emailInput) || string.IsNullOrEmpty(passInput))
                {
                    Console.WriteLine("Email and password cannot be empty. Try again.");
                    continue;
                }

                if (emailInput.Equals(adminEmail, StringComparison.OrdinalIgnoreCase) &&
                    BCrypt.Net.BCrypt.Verify(passInput, adminPasswordHash))
                {
                    Console.WriteLine("Login successful.\n");
                    return true;
                }

                Console.WriteLine("Invalid email or password. Try again.\n");
            }
        }

        private static string ReadPassword()
        {
            var passwordBuilder = new StringBuilder();
            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey(intercept: true);
                if (keyInfo.Key == ConsoleKey.Backspace && passwordBuilder.Length > 0)
                {
                    passwordBuilder.Length--;
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    passwordBuilder.Append(keyInfo.KeyChar);
                    Console.Write("*");
                }
            } while (keyInfo.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return passwordBuilder.ToString();
        }

        private static void ShowMainMenu()
        {
            while (true)
            {
                Console.WriteLine("=== SmartBook Admin Menu ===");
                Console.WriteLine("1. List all books");
                Console.WriteLine("2. Add a new book");
                Console.WriteLine("3. Edit a book by ID");
                Console.WriteLine("4. Delete a book by ID");
                Console.WriteLine("0. Exit");
                Console.Write("Enter your choice: ");

                var choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1":
                        ListBooks();
                        break;
                    case "2":
                        AddBook();
                        break;
                    case "3":
                        EditBook();
                        break;
                    case "4":
                        DeleteBook();
                        break;
                    case "0":
                        Console.WriteLine("Exiting...");
                        return;
                    default:
                        Console.WriteLine("Invalid choice, please enter a number 0 - 4.");
                        break;
                }

                Console.WriteLine(); // Blank line between operations
            }
        }

        private static void ListBooks()
        {
            var books = BookManager.GetAllBooks();

            if (books.Count == 0)
            {
                Console.WriteLine("No books found.");
                return;
            }

            // Adjust column widths in header and lines
            Console.WriteLine(
                "+----+----------------------------------------------------+----------------------+----------------------+");
            Console.WriteLine(
                "| ID | Title                                              | Author               | Category             |");
            Console.WriteLine(
                "+----+----------------------------------------------------+----------------------+----------------------+");

            foreach (var b in books)
            {
                Console.WriteLine(
                    $"| {b.Id,-2} | {Truncate(b.Title, 50),-50} | {Truncate(b.Author.Name, 20),-20} | {Truncate(b.Category.Name, 20),-20} |");
            }

            Console.WriteLine(
                "+----+----------------------------------------------------+----------------------+----------------------+");
        }

        private static void AddBook()
        {
            Console.WriteLine("=== Add a New Book ===");

            string title = ReadNonEmptyString("Enter book title: ");

            int authorId = ReadValidInt("Enter author ID: ");

            int categoryId = ReadValidInt("Enter category ID: ");

            bool added = BookManager.AddBook(title.Trim(), authorId, categoryId);

            if (added)
            {
                Console.WriteLine($"Book '{title}' added successfully.");
            }
            else
            {
                Console.WriteLine("Failed to add book. A book with the same title and author might already exist.");
            }
        }

        private static void EditBook()
        {
            Console.WriteLine("=== Edit a Book ===");

            int bookId = ReadValidInt("Enter the ID of the book to edit: ");

            var books = BookManager.GetAllBooks();
            var book = books.FirstOrDefault(b => b.Id == bookId);

            if (book == null)
            {
                Console.WriteLine($"Book with ID {bookId} not found.");
                return;
            }

            Console.WriteLine($"Current title: {book.Title}");
            string newTitle = ReadNonEmptyString("New Title (or ENTER to keep current): ", allowEmpty: true);
            if (!string.IsNullOrEmpty(newTitle))
                book.Title = newTitle.Trim();

            Console.WriteLine($"Current author ID: {book.AuthorId} ({book.Author.Name})");
            string authorInput = Prompt($"New author ID (or ENTER to keep current): ");
            if (!string.IsNullOrWhiteSpace(authorInput) && int.TryParse(authorInput, out int newAuthorId))
                book.AuthorId = newAuthorId;

            Console.WriteLine($"Current category ID: {book.CategoryId} ({book.Category.Name})");
            string catInput = Prompt($"New category ID (or ENTER to keep current): ");
            if (!string.IsNullOrWhiteSpace(catInput) && int.TryParse(catInput, out int newCatId))
                book.CategoryId = newCatId;

            bool success = UpdateBook(book);

            Console.WriteLine(success ? "Book updated successfully." : "Failed to update book.");
        }

        private static void DeleteBook()
        {
            Console.WriteLine("=== Delete a Book ===");

            int bookId = ReadValidInt("Enter the ID of the book to delete: ");

            bool deleted = BookManager.DeleteBook(bookId);

            if (deleted)
                Console.WriteLine("Book deleted successfully.");
            else
                Console.WriteLine("Book not found or could not delete.");
        }

        private static bool UpdateBook(Book book)
        {
            try
            {
                bool success = BookManager.UpdateBook(book);
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating book: {ex.Message}");
                return false;
            }
        }


        private static string ReadNonEmptyString(string prompt, bool allowEmpty = false)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();

                if (input == null)
                {
                    Console.WriteLine("Input cannot be empty.");
                    continue;
                }

                if (allowEmpty && string.IsNullOrEmpty(input))
                    return "";

                if (!string.IsNullOrWhiteSpace(input))
                    return input.Trim();

                Console.WriteLine("Input cannot be empty.");
            }
        }

        private static int ReadValidInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();

                if (int.TryParse(input, out int value))
                    return value;

                Console.WriteLine("Invalid number, please try again.");
            }
        }

        private static string Prompt(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine() ?? "";
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value ?? "";
            }

            return value.Length <= maxLength ? value : value.Substring(0, maxLength - 3) + "...";
        }
    }
}