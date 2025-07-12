using System;
using System.Linq;
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

            // Get recommendations
            var contentBased = await _recommendationService.GetContentBasedRecommendationsAsync(userId, maxCount: 20);
            var collaborative = await _recommendationService.GetCollaborativeRecommendationsAsync(userId, maxCount: 10);

            // Group content-based by reason
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

            // Bind to each section
            AuthorBasedList.ItemsSource = fromAuthor;
            CategoryBasedList.ItemsSource = fromCategory;
            SimilarUsersList.ItemsSource = fromSimilarUsers;
        }
    }
}
