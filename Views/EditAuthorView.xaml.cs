using System.Windows;
using System.Windows.Controls;
using SmartBook.Core.Data;
using SmartBook.Core.Services;
using SmartBook.Core.DTOs;

namespace SmartBook.Views;

public partial class EditAuthorView : Page
{
    private readonly BookService _bookService = BookService.Instance;

    public EditAuthorView()
    {
        InitializeComponent();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        var authors = await _bookService.GetAllAuthorsAsync();
        AuthorComboBox.ItemsSource = authors;
    }

    private async void AddAuthorButton_Click(object sender, RoutedEventArgs e)
    {
        var name = NewAuthorNameBox.Text.Trim();
        var bio = NewAuthorBioBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Author name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var author = new SmartBook.Core.Models.Author
        {
            Name = name,
            Bio = string.IsNullOrWhiteSpace(bio) ? null : bio
        };

        await ContextManager.Context.Authors.AddAsync(author);
        await ContextManager.Context.SaveChangesAsync();

        MessageBox.Show("Author added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        MainWindow.Instance.Navigate(new DashboardView());
    }

    private void SubmitRequestButton_Click(object sender, RoutedEventArgs e)
    {
        // Placeholder for future request logic
        MessageBox.Show("Author edit request submitted (not implemented).", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Navigate(new DashboardView());
    }

}