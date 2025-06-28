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

    public EditCategoryView()
    {
        InitializeComponent();
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
            MessageBox.Show("Category name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Check for duplicates
        var exists = ContextManager.Context.Categories.Any(c => c.Name.ToLower() == name.ToLower());
        if (exists)
        {
            MessageBox.Show("This category already exists.", "Duplicate Entry", MessageBoxButton.OK, MessageBoxImage.Information);
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
        if (CategoryComboBox.SelectedItem is not CategoryDisplayModel selectedCategory ||
            string.IsNullOrWhiteSpace(EditRequestNameBox.Text))
        {
            MessageBox.Show("Please select a category and enter a new name.", "Incomplete Data", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var newName = EditRequestNameBox.Text.Trim();

        // You could log this request to a table or notify an admin — for now, just display confirmation
        MessageBox.Show(
            $"Request submitted to rename category \"{selectedCategory.Name}\" to \"{newName}\".\n(This feature is not yet implemented.)",
            "Request Submitted",
            MessageBoxButton.OK,
            MessageBoxImage.Information);

        // Optionally clear fields
        EditRequestNameBox.Text = "";
        CategoryComboBox.SelectedIndex = -1;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Navigate(new DashboardView());
    }
}
