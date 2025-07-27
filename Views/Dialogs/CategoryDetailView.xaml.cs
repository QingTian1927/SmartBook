using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartBook.Core.Data;
using SmartBook.Core.DTOs;
using SmartBook.Core.Interfaces;
using SmartBook.Core.Models;
using SmartBook.Views.Dialogs;

namespace SmartBook.Views
{
    public partial class CategoryDetailView : Page
    {
        private readonly IBookService _bookService;
        private readonly IGeminiService _geminiService;
        private readonly SmartBookDbContext _db;

        private int _categoryId;

        // Bindable property for category name
        public string CategoryName { get; set; } = string.Empty;

        private List<BookDisplayModel> _categoryBooks = new();

        public string CurrentUsername => ContextManager.CurrentUser?.Username ?? "<UNKNOWN>";

        public CategoryDetailView(int categoryId)
        {
            InitializeComponent();

            _bookService = App.AppHost.Services.GetRequiredService<IBookService>();
            _geminiService = App.AppHost.Services.GetRequiredService<IGeminiService>();
            _db = App.AppHost.Services.GetRequiredService<SmartBookDbContext>();

            DataContext = this;

            _categoryId = categoryId;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCategoryDetailsAsync();
            await LoadCategoryBooksAsync();
        }

        private async Task LoadCategoryDetailsAsync()
        {
            var category = await _bookService.GetCategoryByIdAsync(_categoryId);
            if (category == null)
            {
                MessageBox.Show("Category not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Optionally navigate back or elsewhere
                return;
            }

            CategoryName = category.Name;

            // Refresh bindings
            DataContext = null;
            DataContext = this;
        }

        private async Task LoadCategoryBooksAsync()
        {
            var books = await _db.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(book => book.CategoryId == _categoryId).ToListAsync();
            var bookDisplayModels = new List<BookDisplayModel>();

            foreach (var b in books)
            {
                var avgRating = await _bookService.CalculateAverageRatingAsync(b.Id);
                bookDisplayModels.Add(new BookDisplayModel
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

            _categoryBooks = bookDisplayModels;
            BooksGrid.ItemsSource = _categoryBooks;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to CategoriesView
            MainWindow.Instance.Title = "SmartBook - Categories";
            MainWindow.Instance.Navigate(new CategoriesView());
        }

        private async void BookItem_Click(object sender, RoutedEventArgs e)
        {
            if (ContextManager.CurrentUser == null) return;

            if (sender is Button btn && btn.DataContext is BookDisplayModel book)
            {
                var userId = ContextManager.CurrentUser.Id;

                bool alreadyExists = await _bookService.UserHasBookAsync(userId, book);
                if (alreadyExists)
                {
                    MessageBox.Show($"'{book.Title}' is already in your library.", "Information",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (book.Rating == null || book.Rating == -1)
                {
                    book.Rating = await _bookService.CalculateAverageRatingAsync(book.BookId);
                }

                var dialog = new BookDetailsDialog(book)
                {
                    Owner = Application.Current.MainWindow
                };

                dialog.Show();

                _ = Task.Run(async () =>
                {
                    if (string.IsNullOrWhiteSpace(book.Description) ||
                        book.Description == "No description available. Let the story surprise you!")
                    {
                        try
                        {
                            string prompt = $"Summarize the book '{book.Title}' by {book.AuthorName} in 1–2 sentences.";
                            string? jsonResponse = await _geminiService.GetGeminiResponseAsync(prompt);

                            if (!string.IsNullOrWhiteSpace(jsonResponse))
                            {
                                using var doc = System.Text.Json.JsonDocument.Parse(jsonResponse);
                                var text = doc.RootElement
                                    .GetProperty("candidates")[0]
                                    .GetProperty("content")
                                    .GetProperty("parts")[0]
                                    .GetProperty("text")
                                    .GetString();

                                if (!string.IsNullOrWhiteSpace(text))
                                {
                                    book.Description = text.Trim();
                                }
                            }
                            else
                            {
                                book.Description = "No description available. Let the story surprise you!";
                            }
                        }
                        catch
                        {
                            book.Description = "No description available. Let the story surprise you!";
                        }

                        dialog.Dispatcher.Invoke(dialog.HideLoadingState);
                    }
                });

                dialog.Closed += async (_, __) =>
                {
                    if (!dialog.IsConfirmed)
                        return;

                    try
                    {
                        var userBook = new UserBook
                        {
                            UserId = userId,
                            BookId = book.BookId,
                            IsRead = false,
                            Rating = null
                        };

                        await _bookService.AddUserBookAsync(userBook);

                        MessageBox.Show($"'{book.Title}' has been added to your library.", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to add book:\n{ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };
            }
        }
    }
}
