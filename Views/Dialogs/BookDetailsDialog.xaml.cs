using System.Windows;
using SmartBook.Core.DTOs;

namespace SmartBook.Views.Dialogs
{
    public partial class BookDetailsDialog : Window
    {
        public BookDisplayModel SelectedBook { get; }

        public bool IsConfirmed { get; private set; }

        public BookDetailsDialog(BookDisplayModel book)
        {
            InitializeComponent();
            SelectedBook = book;
            DataContext = SelectedBook;

            if (string.IsNullOrWhiteSpace(SelectedBook.Description) ||
                SelectedBook.Description == "No description available. Let the story surprise you!")
            {
                ShowLoadingState();
            }
            else
            {
                HideLoadingState();
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            this.Close();
        }

        public void ShowLoadingState()
        {
            if (LoadingPanel != null && DescriptionTextBlock != null)
            {
                LoadingPanel.Visibility = Visibility.Visible;
                DescriptionTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        public void HideLoadingState()
        {
            if (LoadingPanel != null && DescriptionTextBlock != null)
            {
                LoadingPanel.Visibility = Visibility.Collapsed;
                DescriptionTextBlock.Visibility = Visibility.Visible;
            }
        }
    }
}