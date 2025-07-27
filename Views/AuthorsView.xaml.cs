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

namespace SmartBook.Views;

public partial class AuthorsView : Page
{
    private readonly IBookService _bookService;
    private List<AuthorDisplayModel> _allAuthors = new();

    public string CurrentUsername =>
        ContextManager.CurrentUser?.Username ?? "<UNKNOWN>";

    public AuthorsView()
    {
        InitializeComponent();
        _bookService = App.AppHost.Services.GetRequiredService<IBookService>();
        DataContext = this;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadAuthorsAsync();
    }

    private async Task LoadAuthorsAsync()
    {
        var authors = await _bookService.GetAllAuthorsAsync();
        _allAuthors = authors.ToList();
        AuthorGrid.ItemsSource = _allAuthors;
    }

    private void SearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        string keyword = SearchBox.Text.ToLower();
        if (string.IsNullOrWhiteSpace(keyword))
        {
            AuthorGrid.ItemsSource = _allAuthors;
        }
        else
        {
            AuthorGrid.ItemsSource = _allAuthors.Where(a =>
                (!string.IsNullOrEmpty(a.Name) && a.Name.ToLower().Contains(keyword)) ||
                (!string.IsNullOrEmpty(a.Bio) && a.Bio.ToLower().Contains(keyword))
            ).ToList();
        }
    }

    private void AddAuthorBtn_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Title = "SmartBook - Add Author";
        MainWindow.Instance.Navigate(new EditAuthorView(this));
    }

    private void AuthorItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is AuthorDisplayModel author)
        {
            // Navigate to AuthorDetailView passing the author.Id
            MainWindow.Instance.Title = $"SmartBook - Author: {author.Name}";
            MainWindow.Instance.Navigate(new AuthorDetailView(author.Id));
        }
    }
}
