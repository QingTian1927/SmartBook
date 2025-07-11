using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SmartBook.Core.Data;
using SmartBook.Core.DTOs;
using SmartBook.Core.Models;
using SmartBook.Core.Services;

namespace SmartBook.Views;

public partial class EditCategoryView : Page
{
    private readonly BookService _bookService = BookService.Instance;
    private readonly Page? _returnPage;

    public EditCategoryView(Page? returnPage = null)
    {
        InitializeComponent();
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

        // Check for duplicates
        var exists = ContextManager.Context.Categories.Any(c => c.Name.ToLower() == name.ToLower());
        if (exists)
        {
            MessageBox.Show("This category already exists.", "Duplicate Entry", MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var category = new Category { Name = name };
        await ContextManager.Context.Categories.AddAsync(category);
        await ContextManager.Context.SaveChangesAsync();

        MessageBox.Show("Category added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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

        var currentUser = ContextManager.CurrentUser;
        if (currentUser == null)
        {
            MessageBox.Show("You must be logged in to submit an edit request.", "Authentication Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var request = new CategoryEditRequest
        {
            CategoryId = selectedCategory.Id,
            ProposedName = proposedName,
            RequestedByUserId = currentUser.Id,
            RequestedAt = DateTime.UtcNow,
            Status = "Pending"
        };

        await ContextManager.Context.CategoryEditRequests.AddAsync(request);
        await ContextManager.Context.SaveChangesAsync();

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