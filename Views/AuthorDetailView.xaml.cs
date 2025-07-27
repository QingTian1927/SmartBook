using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SmartBook.Core.Data;
using SmartBook.Core.DTOs;
using SmartBook.Core.Interfaces;
using SmartBook.Core.Models;
using SmartBook.Views.Dialogs;

namespace SmartBook.Views
{
    public partial class AuthorDetailView : Page
    {
        private readonly IBookService _bookService;
        private readonly IGeminiService _geminiService;

        private int _authorId;

        // Bindable properties for author info
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorBio { get; set; } = string.Empty;

        private List<BookDisplayModel> _authorBooks = new();

        public string CurrentUsername => ContextManager.CurrentUser?.Username ?? "<UNKNOWN>";

        public AuthorDetailView(int authorId)
        {
            InitializeComponent();

            // Dependency Injection for services
            _bookService = App.AppHost.Services.GetRequiredService<IBookService>();
            _geminiService = App.AppHost.Services.GetRequiredService<IGeminiService>();

            DataContext = this;

            _authorId = authorId;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAuthorDetailsAsync();
            await LoadAuthorBooksAsync();
        }

        private async Task LoadAuthorDetailsAsync()
        {
            var author = await _bookService.GetAuthorByIdAsync(_authorId);
            if (author == null)
            {
                MessageBox.Show("Author not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Optionally navigate back or elsewhere
                return;
            }

            AuthorName = author.Name;
            AuthorBio = author.Bio ?? "(No biography available)";

            // Refresh bindings to update UI
            DataContext = null;
            DataContext = this;
        }

        private async Task LoadAuthorBooksAsync()
        {
            var books = await _bookService.GetBooksByAuthorIdAsync(_authorId);
            _authorBooks = books.ToList();

            BooksGrid.ItemsSource = _authorBooks;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to AuthorsView page
            MainWindow.Instance.Title = "SmartBook - Authors";
            MainWindow.Instance.Navigate(new AuthorsView());
        }

        private async void BookItem_Click(object sender, RoutedEventArgs e)
        {
            if (ContextManager.CurrentUser == null) return;

            if (sender is Button btn && btn.DataContext is BookDisplayModel book)
            {
                var userId = ContextManager.CurrentUser.Id;

                // Check if the book is already in user's library
                bool alreadyExists = await _bookService.UserHasBookAsync(userId, book);
                if (alreadyExists)
                {
                    MessageBox.Show($"'{book.Title}' is already in your library.", "Information",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Calculate average rating if missing
                if (book.Rating == null || book.Rating == -1)
                {
                    book.Rating = await _bookService.CalculateAverageRatingAsync(book.BookId);
                }

                var dialog = new BookDetailsDialog(book)
                {
                    Owner = Application.Current.MainWindow
                };

                // Show dialog non-blocking
                dialog.Show();

                // Fetch AI-generated description in background if needed
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

                        // Update dialog UI on UI thread
                        dialog.Dispatcher.Invoke(dialog.HideLoadingState);
                    }
                });

                // Handle result after dialog closes
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
