using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartBook.Core.Data;
using SmartBook.Core.DTOs;
using SmartBook.Core.Interfaces;
using SmartBook.Core.Models;
using SmartBook.Views.Dialogs;

namespace SmartBook.Views
{
    public partial class RecommendationView : Page
    {
        private readonly IRecommendationService _recommendationService;
        private readonly IBookService _bookService;
        private readonly IGeminiService _geminiService;
        private readonly SmartBookDbContext _db;

        public string CurrentUsername =>
            ContextManager.CurrentUser?.Username ?? "<UNKNOWN>";

        // Carousel state
        private List<BookDisplayModel> _geminiCarouselItems = new();
        private int _geminiCurrentIndex = 0;

        public RecommendationView()
        {
            InitializeComponent();
            _recommendationService = App.AppHost.Services.GetRequiredService<IRecommendationService>();
            _bookService = App.AppHost.Services.GetRequiredService<IBookService>();
            _geminiService = App.AppHost.Services.GetRequiredService<IGeminiService>();
            _db = App.AppHost.Services.GetRequiredService<SmartBookDbContext>();
            DataContext = this;
        }


        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (ContextManager.CurrentUser is null)
            {
                MessageBox.Show("Please log in first.", "Authentication Required",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                MainWindow.Instance.Navigate(new LoginView());
                return;
            }

            var userId = ContextManager.CurrentUser.Id;

            // Load traditional first
            await LoadTraditionalRecommendations(userId);

            // Load Gemini carousel in background
            _ = LoadGeminiRecommendations(userId);
        }

        private async Task LoadTraditionalRecommendations(int userId)
        {
            var contentBased = await _recommendationService.GetContentBasedRecommendationsAsync(userId, maxCount: 20);
            var collaborative = await _recommendationService.GetCollaborativeRecommendationsAsync(userId, maxCount: 10);

            var fromAuthor = contentBased
                .Where(b => b.Reason?.Contains("author", StringComparison.OrdinalIgnoreCase) == true)
                .GroupBy(b => b.BookId)
                .Select(g => g.First())
                .ToList();

            var fromCategory = contentBased
                .Where(b => b.Reason?.Contains("category", StringComparison.OrdinalIgnoreCase) == true)
                .GroupBy(b => b.BookId)
                .Select(g => g.First())
                .ToList();

            var fromSimilarUsers = collaborative
                .GroupBy(b => b.BookId)
                .Select(g => g.First())
                .ToList();

            AuthorBasedList.ItemsSource = fromAuthor;
            CategoryBasedList.ItemsSource = fromCategory;
            SimilarUsersList.ItemsSource = fromSimilarUsers;
        }

        private async Task LoadGeminiRecommendations(int userId)
        {
            GeminiCarouselContent.DataContext = null;
            GeminiSpinner.Visibility = Visibility.Visible;
            RefreshGeminiButton.IsEnabled = false;

            try
            {
                var geminiResults = await _recommendationService.GetGeminiRecommendationsAsync(userId);

                _geminiCarouselItems = geminiResults
                    .GroupBy(b => b.BookId)
                    .Select(g => g.First())
                    .ToList();

                _geminiCurrentIndex = 0;
                UpdateGeminiCarousel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load AI recommendations:\n{ex.Message}", "Gemini Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                GeminiSpinner.Visibility = Visibility.Collapsed;
                RefreshGeminiButton.IsEnabled = true;
            }
        }

        private void UpdateGeminiCarousel()
        {
            if (_geminiCarouselItems.Count == 0)
            {
                GeminiCarouselContent.DataContext = null;
                GeminiCarouselContent.Visibility = Visibility.Collapsed;
                GeminiDots.ItemsSource = null;
                return;
            }

            _geminiCurrentIndex = Math.Clamp(_geminiCurrentIndex, 0, _geminiCarouselItems.Count - 1);
            GeminiCarouselContent.DataContext = _geminiCarouselItems[_geminiCurrentIndex];
            GeminiCarouselContent.Visibility = Visibility.Visible;

            GeminiDots.ItemsSource = Enumerable.Range(0, _geminiCarouselItems.Count)
                .Select(i => new { Display = i == _geminiCurrentIndex ? "●" : "○", Index = i })
                .ToList();
        }

        private void GeminiDot_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int index)
            {
                _geminiCurrentIndex = index;
                UpdateGeminiCarousel();
            }
        }

        private async void RefreshGemini_Click(object sender, RoutedEventArgs e)
        {
            if (ContextManager.CurrentUser is null) return;

            GeminiCarouselContent.DataContext = null;
            GeminiSpinner.Visibility = Visibility.Visible;
            RefreshGeminiButton.IsEnabled = false;

            await Task.Delay(300); // Short delay for UI feedback
            await LoadGeminiRecommendations(ContextManager.CurrentUser.Id);
        }

        private void BookItem_Click(object sender, RoutedEventArgs e)
        {
            HandleBookItemClick(sender);
        }

        private void BookBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HandleBookItemClick(sender);
        }

        private async void HandleBookItemClick(object sender)
        {
            if (ContextManager.CurrentUser == null) return;

            if (sender is FrameworkElement element && element.Tag is BookDisplayModel book)
            {
                var userId = ContextManager.CurrentUser.Id;

                bool alreadyExists = await _db.UserBooks
                    .AnyAsync(ub => ub.UserId == userId && ub.BookId == book.BookId);

                if (alreadyExists)
                {
                    MessageBox.Show($"'{book.Title}' is already in your library.", "Info",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (book.Rating == null || book.Rating == -1)
                {
                    book.Rating = await _bookService.CalculateAverageRatingAsync(book.BookId);
                }

                var dialog = new BookDetailsDialog(book)
                {
                    Owner = Window.GetWindow(this)
                };

                // show the dialog (non-blocking)
                dialog.Show();

                // run Gemini generation in background
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
                                using var doc = JsonDocument.Parse(jsonResponse);
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

                        // Switch back to UI thread to hide the loading spinner
                        dialog.Dispatcher.Invoke(dialog.HideLoadingState);
                    }
                });

                // handle result after dialog closes
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

                        MessageBox.Show($"'{book.Title}' has been added to your library.",
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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