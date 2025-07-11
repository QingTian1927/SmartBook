using System.Windows;
using System.Windows.Controls;
using SmartBook.Core.Data;
using SmartBook.Core.Services;
using SmartBook.Core.DTOs;
using SmartBook.Core.Models;

namespace SmartBook.Views;

public partial class EditAuthorView : Page
{
    private readonly BookService _bookService = BookService.Instance;
    private readonly Page? _returnPage;

    public EditAuthorView(Page? returnPage = null)
    {
        InitializeComponent();
        _returnPage = returnPage;
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
            MessageBox.Show("Author name is required.", "Validation Error", MessageBoxButton.OK,
                MessageBoxImage.Warning);
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

    private async void SubmitRequestButton_Click(object sender, RoutedEventArgs e)
    {
        if (AuthorComboBox.SelectedItem is not AuthorDisplayModel selectedAuthor)
        {
            MessageBox.Show("Please select an author to edit.", "Validation Error", MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var proposedName = EditRequestNameBox.Text.Trim();
        var proposedBio = EditRequestBioBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(proposedName) && string.IsNullOrWhiteSpace(proposedBio))
        {
            MessageBox.Show("You must enter at least one field to edit.", "Validation Error", MessageBoxButton.OK,
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

        var request = new AuthorEditRequest
        {
            AuthorId = selectedAuthor.Id,
            ProposedName = string.IsNullOrWhiteSpace(proposedName) ? null : proposedName,
            ProposedBio = string.IsNullOrWhiteSpace(proposedBio) ? null : proposedBio,
            RequestedByUserId = currentUser.Id,
            RequestedAt = DateTime.UtcNow,
            Status = "Pending"
        };

        await ContextManager.Context.AuthorEditRequests.AddAsync(request);
        await ContextManager.Context.SaveChangesAsync();

        MessageBox.Show("Edit request submitted successfully.", "Success", MessageBoxButton.OK,
            MessageBoxImage.Information);

        // Optionally reset the form
        AuthorComboBox.SelectedItem = null;
        EditRequestNameBox.Text = string.Empty;
        EditRequestBioBox.Text = string.Empty;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Navigate(_returnPage ?? new DashboardView());
    }
}