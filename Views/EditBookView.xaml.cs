using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SmartBook.Core.Data;
using SmartBook.Core.Models;
using SmartBook.Core.Services;
using SmartBook.Core.DTOs;

namespace SmartBook.Views
{
    public partial class EditBookView : Page
    {
        private readonly BookService _bookService = BookService.Instance;
        private readonly UserBook _userBook;
        private Book _book;

        public EditBookView(UserBook userBook)
        {
            InitializeComponent();
            _userBook = userBook;
            _book = userBook.Book;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAuthorsAsync();
            await LoadCategoriesAsync();

            IsReadComboBox.SelectedValue = _userBook.IsRead.ToString();
            RatingComboBox.SelectedValue = _userBook.Rating?.ToString();

            RequestedTitleBox.Text = _book.Title;
            RequestedAuthorComboBox.SelectedValue = _book.AuthorId;
            RequestedCategoryComboBox.SelectedValue = _book.CategoryId;
        }

        // ReSharper disable once AsyncVoidMethod
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsReadComboBox.SelectedValue is null)
            {
                MessageBox.Show("Please fill all required fields.", "Edit Book Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _userBook.IsRead = bool.Parse(IsReadComboBox.SelectedValue.ToString()!);
            _userBook.Rating = RatingComboBox.SelectedValue is null ? null : int.Parse(RatingComboBox.SelectedValue.ToString()!);
            await _bookService.UpdateUserBookAsync(_userBook);

            MessageBox.Show("Book updated successfully!");
            MainWindow.Instance.Navigate(new DashboardView());
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.Navigate(new DashboardView());
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                ContextManager.IsAdmin
                    ? $"Are you sure you want to permanently delete the book \"{_book.Title}\"?"
                    : $"Are you sure you want to remove this book from your library?",
                "Confirm Deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return;

            if (ContextManager.IsAdmin)
            {
                await _bookService.DeleteBookAsync(_book.Id);
                MessageBox.Show("Book deleted successfully.");
            }
            else
            {
                await _bookService.DeleteUserBookAsync(_userBook.Id);
                MessageBox.Show("Book removed from your library.");
            }

            MainWindow.Instance.Navigate(new DashboardView());
        }

        private async Task LoadCategoriesAsync()
        {
            var categories = await _bookService.GetAllCategoriesAsync();
            RequestedCategoryComboBox.ItemsSource = categories;
        }

        private async Task LoadAuthorsAsync()
        {
            var authors = await _bookService.GetAllAuthorsAsync();
            RequestedAuthorComboBox.ItemsSource = authors;
        }

        private void SubmitRequestButton_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for future request logic
            MessageBox.Show("Request submitted to admin (not implemented).", "Request Sent", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
