using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SmartBook.Core.Data;
using SmartBook.Core.DTOs;
using SmartBook.Core.Interfaces;
using SmartBook.Core.Models;
using SmartBook.Core.Services;

namespace SmartBook.Views;

public partial class DashboardView : Page
{
    private readonly IBookService _bookService;
    private User? GetCurrentUser() => ContextManager.CurrentUser;

    public string CurrentUsername =>
        GetCurrentUser() == null ? "<UNKNOWN>" : GetCurrentUser().Username;

    public DashboardView()
    {
        InitializeComponent();
        _bookService = App.AppHost.Services.GetRequiredService<IBookService>();
        DataContext = this;
    }

    // ReSharper disable once AsyncVoidMethod
    private async void Page_Loaded(Object sender, RoutedEventArgs e)
    {
        await LoadBooksAsync();
        await LoadCategoriesAsync();
        
        MinRatingBox.TextChanged += FilterComboBox_Changed;
        MaxRatingBox.TextChanged += FilterComboBox_Changed;
    }

    // ReSharper disable once AsyncVoidMethod
    private async void CategoryComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        IEnumerable<BookDisplayModel>? books;
        var currentUser = GetCurrentUser();
        
        if (currentUser is null && ContextManager.IsAdmin)
        {
            books = await _bookService.FilterBooksDisplayAsync((int?)CategoryComboBox.SelectedIndex);
        }
        else if (currentUser is not null && !ContextManager.IsAdmin)
        {
            books = await _bookService.FilterBooksDisplayAsync(currentUser.Id, CategoryComboBox.SelectedIndex);
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
        var currentUser = GetCurrentUser();
        var keyword = SearchBox.Text;

        if (currentUser is null && ContextManager.IsAdmin)
        {
            books = await _bookService.SearchBooksDisplayAsync(keyword);
        }
        else if (currentUser is not null && !ContextManager.IsAdmin)
        {
            books = await _bookService.SearchBooksDisplayAsync(currentUser.Id, keyword,
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
        var currentUser = GetCurrentUser();
        
        if (currentUser is null && ContextManager.IsAdmin)
        {
            books = await _bookService.GetAllBooksDisplayAsync();
        }
        else if (currentUser is not null && !ContextManager.IsAdmin)
        {
            books = await _bookService.GetAllBooksDisplayAsync(currentUser.Id);
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
        IEnumerable<CategoryDisplayModel> categories;
        var currentUser = GetCurrentUser();
        
        if (ContextManager.IsAdmin)
        {
            categories = await _bookService.GetAllCategoriesAsync();
        }
        else
        {
            categories = await _bookService.GetAllCategoriesAsync(currentUser?.Id);
        }

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
            var userBooks = await _bookService.GetAllUserBooksAsync(userId);
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

    private void ApplyAllFilters(List<BookDisplayModel> books)
    {
        if (ReadStatusComboBox == null || MinRatingBox == null || MaxRatingBox == null)
        {
            // UI not fully loaded yet; skip filtering
            return;
        }

        // Filter by read status
        string readStatus = (ReadStatusComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All";
        if (readStatus == "Read")
            books = books.Where(b => b.IsRead).ToList();
        else if (readStatus == "Unread")
            books = books.Where(b => !b.IsRead).ToList();

        // Filter by rating
        bool hasMin = int.TryParse(MinRatingBox.Text, out int min);
        bool hasMax = int.TryParse(MaxRatingBox.Text, out int max);

        if (hasMin && hasMax)
            books = books.Where(b => b.Rating.HasValue && b.Rating.Value >= min && b.Rating.Value <= max).ToList();
        else if (hasMin)
            books = books.Where(b => b.Rating.HasValue && b.Rating.Value >= min).ToList();
        else if (hasMax)
            books = books.Where(b => b.Rating.HasValue && b.Rating.Value <= max).ToList();

        BookGrid.ItemsSource = books;
    }

    private async void FilterComboBox_Changed(object sender, EventArgs e)
    {
        // Ensure filter controls are ready
        if (ReadStatusComboBox == null || MinRatingBox == null || MaxRatingBox == null)
            return;

        IEnumerable<BookDisplayModel> books;

        // Always check user/admin state correctly
        if (ContextManager.IsAdmin)
        {
            books = await _bookService.GetAllBooksDisplayAsync();
        }
        else
        {
            var currentUser = GetCurrentUser();

            // Safety: Return if not logged in
            if (currentUser == null)
            {
                return; // Don't show error during early TextChanged calls
            }

            books = await _bookService.GetAllBooksDisplayAsync(currentUser.Id);
        }

        ApplyAllFilters(books.ToList());
    }

}