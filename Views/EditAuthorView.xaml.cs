using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SmartBook.Core.Data;
using SmartBook.Core.Services;
using SmartBook.Core.DTOs;
using SmartBook.Core.Interfaces;
using SmartBook.Core.Models;

namespace SmartBook.Views;

public partial class EditAuthorView : Page
{
    private readonly IBookService _bookService;
    private readonly Page? _returnPage;

    public EditAuthorView(Page? returnPage = null)
    {
        InitializeComponent();
        _bookService = App.AppHost.Services.GetRequiredService<IBookService>();
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

        var success = await _bookService.TryAddAuthorAsync(name, bio);
        if (!success)
        {
            MessageBox.Show("Failed to add author.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

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

        var success = await _bookService.SubmitAuthorEditRequestAsync(selectedAuthor.Id, proposedName, proposedBio);
        if (!success)
        {
            MessageBox.Show("Failed to submit edit request. Make sure you're logged in.", "Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        MessageBox.Show("Edit request submitted successfully.", "Success", MessageBoxButton.OK,
            MessageBoxImage.Information);

        // Reset form
        AuthorComboBox.SelectedItem = null;
        EditRequestNameBox.Text = string.Empty;
        EditRequestBioBox.Text = string.Empty;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Navigate(_returnPage ?? new DashboardView());
    }
}