using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SmartBook.Core.Data;
using SmartBook.Core.DTOs;
using SmartBook.Core.Interfaces;

namespace SmartBook.Views
{
    public partial class RecommendationView : Page
    {
        private readonly IRecommendationService _recommendationService;

        public string CurrentUsername =>
            ContextManager.CurrentUser?.Username ?? "<UNKNOWN>";

        // Carousel state
        private List<BookDisplayModel> _geminiCarouselItems = new();
        private int _geminiCurrentIndex = 0;

        public RecommendationView()
        {
            InitializeComponent();
            _recommendationService = App.AppHost.Services.GetRequiredService<IRecommendationService>();
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

            // Update dot indicators
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
    }
}
