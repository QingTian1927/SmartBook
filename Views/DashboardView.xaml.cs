using System.Windows;
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

    public string CurrentUsername => ContextManager.CurrentUser == null ? "<UNKNOWN>" : ContextManager.CurrentUser.Username;
    
    public DashboardView()
    {
        InitializeComponent();
        DataContext = this;
    }

    // ReSharper disable once AsyncVoidMethod
    private async void Page_Loaded(Object sender, RoutedEventArgs e)
    {
        await LoadBooksAsync();
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
}