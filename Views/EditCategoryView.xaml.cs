using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SmartBook.Core.DTOs;
using SmartBook.Core.Interfaces;
using SmartBook.Core.Models;

namespace SmartBook.Views
{
    public partial class EditCategoryView : Page
    {
        private readonly IBookService _bookService;
        private readonly Page? _returnPage;

        public EditCategoryView(Page? returnPage = null)
        {
            InitializeComponent();
            _bookService = App.AppHost.Services.GetRequiredService<IBookService>();
            _returnPage = returnPage;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var categories = await _bookService.GetAllCategoriesAsync();
            CategoryComboBox.ItemsSource = categories;
        }

        private async void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            var name = NewCategoryNameBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Category name is required.", "Validation Error", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var added = await _bookService.TryAddCategoryAsync(name);
            if (!added)
            {
                MessageBox.Show("This category already exists.", "Duplicate Entry", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            MessageBox.Show("Category added successfully.", "Success", MessageBoxButton.OK,
                MessageBoxImage.Information);
            MainWindow.Instance.Navigate(new DashboardView());
        }

        private async void SubmitRequestButton_Click(object sender, RoutedEventArgs e)
        {
            if (CategoryComboBox.SelectedItem is not CategoryDisplayModel selectedCategory)
            {
                MessageBox.Show("Please select a category to edit.", "Validation Error", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var proposedName = EditRequestNameBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(proposedName))
            {
                MessageBox.Show("Please enter a new name for the category.", "Validation Error", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var result = await _bookService.SubmitCategoryEditRequestAsync(selectedCategory.Id, proposedName);
            if (!result)
            {
                MessageBox.Show("You must be logged in to submit an edit request.", "Authentication Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Edit request submitted successfully.", "Success", MessageBoxButton.OK,
                MessageBoxImage.Information);

            EditRequestNameBox.Text = "";
            CategoryComboBox.SelectedIndex = -1;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.Navigate(_returnPage ?? new DashboardView());
        }
    }
}