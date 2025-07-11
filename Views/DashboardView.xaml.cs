﻿using System.Windows;
using System.Windows.Controls;
using SmartBook.Core.Data;
using SmartBook.Core.DTOs;
using SmartBook.Core.Interfaces;
using SmartBook.Core.Models;
using SmartBook.Core.Services;

namespace SmartBook.Views;

public partial class DashboardView : Page
{
    private readonly IBookService _bookService = BookService.Instance;
    private readonly User? _currentUser = ContextManager.CurrentUser;

    public string CurrentUsername =>
        ContextManager.CurrentUser == null ? "<UNKNOWN>" : ContextManager.CurrentUser.Username;

    public DashboardView()
    {
        InitializeComponent();
        DataContext = this;
    }

    // ReSharper disable once AsyncVoidMethod
    private async void Page_Loaded(Object sender, RoutedEventArgs e)
    {
        await LoadBooksAsync();
        await LoadCategoriesAsync();
    }

    // ReSharper disable once AsyncVoidMethod
    private async void CategoryComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        IEnumerable<BookDisplayModel>? books;
        if (_currentUser is null && ContextManager.IsAdmin)
        {
            books = await _bookService.FilterBooksDisplayAsync((int?)CategoryComboBox.SelectedIndex);
        }
        else if (_currentUser is not null && !ContextManager.IsAdmin)
        {
            books = await _bookService.FilterBooksDisplayAsync(_currentUser.Id, CategoryComboBox.SelectedIndex);
        }
        else
        {
            MessageBox.Show(
                "No login detected. Please login first before viewing books",
                "Listing Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );

            MainWindow.Instance.Navigate(new LoginView());
            return;
        }

        BookGrid.ItemsSource = books;
    }

    // ReSharper disable once AsyncVoidMethod
    private async void SearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        IEnumerable<BookDisplayModel>? books;
        var keyword = SearchBox.Text;

        if (_currentUser is null && ContextManager.IsAdmin)
        {
            books = await _bookService.SearchBooksDisplayAsync(keyword);
        }
        else if (_currentUser is not null && !ContextManager.IsAdmin)
        {
            books = await _bookService.SearchBooksDisplayAsync(_currentUser.Id, keyword,
                CategoryComboBox.SelectedIndex);
        }
        else
        {
            MessageBox.Show(
                "No login detected. Please login first before viewing books",
                "Listing Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );

            MainWindow.Instance.Navigate(new LoginView());
            return;
        }

        BookGrid.ItemsSource = books;
    }

    private async Task LoadBooksAsync()
    {
        IEnumerable<BookDisplayModel>? books;
        if (_currentUser is null && ContextManager.IsAdmin)
        {
            books = await _bookService.GetAllBooksDisplayAsync();
        }
        else if (_currentUser is not null && !ContextManager.IsAdmin)
        {
            books = await _bookService.GetAllBooksDisplayAsync(_currentUser.Id);
        }
        else
        {
            MessageBox.Show(
                "No login detected. Please login first before viewing books",
                "Listing Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );

            MainWindow.Instance.Navigate(new LoginView());
            return;
        }

        BookGrid.ItemsSource = books;
    }

    private async Task LoadCategoriesAsync()
    {
        IEnumerable<CategoryDisplayModel>? categories = await _bookService.GetAllCategoriesAsync();
        List<CategoryDisplayModel> categoryList = new List<CategoryDisplayModel>
            { new CategoryDisplayModel { Id = 0, Name = "All Categories" } };
        categoryList.AddRange(categories);

        CategoryComboBox.ItemsSource = categoryList;
        CategoryComboBox.SelectedIndex = 0;
    }

    private void AddBookBtn_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Title = "SmartBook - Add Book";
        MainWindow.Instance.Navigate(new AddBookView());
    }

    // ReSharper disable once AsyncVoidMethod
    private async void BookItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is BookDisplayModel bookDisplay)
        {
            if (ContextManager.CurrentUser is null)
            {
                MessageBox.Show("No logged-in user found.");
                return;
            }

            var userId = ContextManager.CurrentUser.Id;
            var userBooks = await BookService.Instance.GetAllUserBooksAsync(userId);
            var userBook = userBooks.FirstOrDefault(ub => ub.BookId == bookDisplay.BookId);

            if (userBook == null)
            {
                MessageBox.Show("Could not find UserBook for the selected book.");
                return;
            }

            MainWindow.Instance.Title = $"SmartBook - Edit Book {bookDisplay.Title}";
            MainWindow.Instance.Navigate(new EditBookView(userBook));
        }
    }
}