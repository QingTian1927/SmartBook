using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SmartBook.Core.Data;
using SmartBook.Core.DTOs;
using SmartBook.Core.Interfaces;
using SmartBook.Core.Models;
using SmartBook.Core.Services;

namespace SmartBook.Views;

public partial class AddBookView : Page
{
    private readonly IBookService _bookService;
    private readonly IAuthService _authService;

    public AddBookView()
    {
        InitializeComponent();
        _bookService = App.AppHost.Services.GetRequiredService<IBookService>();
        _authService = App.AppHost.Services.GetRequiredService<IAuthService>();
    }

    // ReSharper disable once AsyncVoidMethod
    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (AuthorComboBox.SelectedItem is not AuthorDisplayModel selectedAuthor ||
            CategoryComboBox.SelectedItem is not CategoryDisplayModel selectedCategory || IsReadComboBox.SelectedValue is null)
        {
            MessageBox.Show(
                "Please fill all required fields.",
                "Add Book Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return;
        }

        if (ContextManager.CurrentUser is null)
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

        var userBook = new UserBook
        {
            UserId = ContextManager.CurrentUser.Id,
            User = ContextManager.CurrentUser,
            IsRead = bool.Parse(IsReadComboBox.SelectedValue.ToString()!),
            Rating = RatingComboBox.SelectedValue is null ? null : int.Parse(RatingComboBox.SelectedValue.ToString()!),
        };

        if (SearchResultsList.SelectedItem is BookDisplayModel selectedBook)
        {
            userBook.Book = (await _bookService.GetBookByIdAsync(selectedBook.BookId))!;   
        }
        else
        {
            var book = new Book
            {
                Title = TitleBox.Text,
                AuthorId = selectedAuthor.Id,
                CategoryId = selectedCategory.Id,
            };
            
            await _bookService.AddBookAsync(book);
            userBook.Book = book;
        }
        await _bookService.AddUserBookAsync(userBook);
        
        MessageBox.Show("Book added successfully!");
        MainWindow.Instance.Title = "SmartBook - Dashboard";
        MainWindow.Instance.Navigate(new DashboardView());
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Title = "SmartBook - Dashboard";
        MainWindow.Instance.Navigate(new DashboardView());
    }

    // ReSharper disable once AsyncVoidMethod
    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadAuthorsAsync();
        await LoadCategoriesAsync();

        SearchResultsList.ItemsSource = await _bookService.GetAllBooksDisplayAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        var categories = await _bookService.GetAllCategoriesAsync();
        CategoryComboBox.ItemsSource = categories;
    }

    private async Task LoadAuthorsAsync()
    {
        var authors = await _bookService.GetAllAuthorsAsync();
        AuthorComboBox.ItemsSource = authors;
    }

    private void AddAuthorButton_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Title = "SmartBook - Add Author";
        MainWindow.Instance.Navigate(new EditAuthorView(this));
    }

    private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Title = "SmartBook - Add Category";
        MainWindow.Instance.Navigate(new EditCategoryView(this));
    }

    // ReSharper disable once AsyncVoidMethod
    private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (ContextManager.CurrentUser is null && ContextManager.IsAdmin)
        {
            MessageBox.Show(
                "No login detected. Please login first before viewing books",
                "Listing Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );

            MainWindow.Instance.Title = "SmartBook - Login";
            MainWindow.Instance.Navigate(new LoginView());
            return;
        }

        IEnumerable<BookDisplayModel>? books;
        var keyword = SearchBox.Text;
        
        books = await _bookService.SearchBooksDisplayAsync(keyword);
        SearchResultsList.ItemsSource = books;
    }

    private void SearchResultsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SearchResultsList.SelectedItem is null)
        {
            return;
        }

        BookDisplayModel selectedBook = (BookDisplayModel)SearchResultsList.SelectedItem;
        TitleBox.Text = selectedBook.Title;

        foreach (var item in AuthorComboBox.Items)
        {
            if (item is AuthorDisplayModel author && author.Name == selectedBook.AuthorName)
            {
                AuthorComboBox.SelectedItem = author;
                break;
            }
        }

        foreach (var item in CategoryComboBox.Items)
        {
            if (item is CategoryDisplayModel category && category.Name == selectedBook.CategoryName)
            {
                CategoryComboBox.SelectedItem = category;
                break;
            }
        }
    }
}