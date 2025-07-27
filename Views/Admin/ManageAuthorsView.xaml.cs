using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SmartBook.Core.Data;
using SmartBook.Core.DTOs;
using SmartBook.Core.Interfaces;

namespace SmartBook.Views.Admin
{
    public partial class ManageAuthorsView : Page
    {
        private readonly IBookService _bookService;
        private List<AuthorDisplayModel> _allAuthors = new();
        private List<AuthorDisplayModel> _filteredAuthors = new();

        private readonly HashSet<AuthorDisplayModel> _editedAuthors = new();
        private readonly HashSet<AuthorDisplayModel> _newAuthors = new();

        public string CurrentUsername => ContextManager.CurrentUser?.Username ?? "<UNKNOWN>";

        public ManageAuthorsView()
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
            _filteredAuthors = new List<AuthorDisplayModel>(_allAuthors);
            AuthorsDataGrid.ItemsSource = _filteredAuthors;
            _editedAuthors.Clear();
            _newAuthors.Clear();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var keyword = SearchBox.Text.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                _filteredAuthors = new List<AuthorDisplayModel>(_allAuthors);
            }
            else
            {
                _filteredAuthors = _allAuthors
                    .Where(a =>
                        (a.Name != null && a.Name.ToLower().Contains(keyword)) ||
                        (a.Bio != null && a.Bio.ToLower().Contains(keyword))
                    ).ToList();
            }
            AuthorsDataGrid.ItemsSource = _filteredAuthors;
        }

        private void AuthorsDataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            e.NewItem = new AuthorDisplayModel
            {
                Id = 0,
                Name = "",
                Bio = "",
                BookCount = 0
            };
        }

        private void AuthorsDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit && e.Row.Item is AuthorDisplayModel author)
            {
                if (string.IsNullOrWhiteSpace(author.Name))
                {
                    MessageBox.Show("Author name cannot be empty.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _ = LoadAuthorsAsync();
                    return;
                }

                if (author.Id == 0)
                {
                    // New author
                    _newAuthors.Add(author);
                }
                else
                {
                    // Existing author edited
                    _editedAuthors.Add(author);
                }
            }
        }

        private async void SaveChangesBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_editedAuthors.Count == 0 && _newAuthors.Count == 0)
            {
                MessageBox.Show("No changes to save.", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int addedCount = 0;
            int updatedCount = 0;

            // Add new authors
            foreach (var newAuthor in _newAuthors.ToList())
            {
                if (string.IsNullOrWhiteSpace(newAuthor.Name))
                {
                    MessageBox.Show("New author name cannot be empty.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue;
                }

                bool added = await TryAddAuthorAsync(newAuthor);
                if (added)
                {
                    addedCount++;
                    _newAuthors.Remove(newAuthor);
                }
                else
                {
                    MessageBox.Show($"Author '{newAuthor.Name}' already exists or failed to add.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // Update edited authors
            foreach (var editedAuthor in _editedAuthors.ToList())
            {
                bool updated = await TryUpdateAuthorAsync(editedAuthor);
                if (updated)
                {
                    updatedCount++;
                    _editedAuthors.Remove(editedAuthor);
                }
            }

            MessageBox.Show($"Added {addedCount} new authors and updated {updatedCount} authors.", "Save Completed", MessageBoxButton.OK, MessageBoxImage.Information);
            await LoadAuthorsAsync();
        }

        private async Task<bool> TryAddAuthorAsync(AuthorDisplayModel newAuthor)
        {
            var success = await _bookService.TryAddAuthorAsync(newAuthor.Name.Trim(), string.IsNullOrWhiteSpace(newAuthor.Bio) ? null : newAuthor.Bio.Trim());
            return success;
        }

        private async Task<bool> TryUpdateAuthorAsync(AuthorDisplayModel author)
        {
            try
            {
                var duplicate = _allAuthors.FirstOrDefault(a => a.Name.ToLower() == author.Name.ToLower() && a.Id != author.Id);
                if (duplicate != null)
                {
                    MessageBox.Show($"Author name '{author.Name}' is already in use.", "Duplicate Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                var authorEntity = await _bookService.GetAuthorByIdAsync(author.Id);
                if (authorEntity == null)
                {
                    MessageBox.Show($"Author with ID {author.Id} not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                authorEntity.Name = author.Name.Trim();
                authorEntity.Bio = string.IsNullOrWhiteSpace(author.Bio) ? null : author.Bio.Trim();

                await _bookService.UpdateAuthorAsync(authorEntity);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async void DeleteSelectedBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AuthorsDataGrid.SelectedItem is not AuthorDisplayModel selectedAuthor)
            {
                MessageBox.Show("Please select an author to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirm = MessageBox.Show($"Are you sure you want to delete the author '{selectedAuthor.Name}' and all associated books?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                await _bookService.DeleteAuthorCascadeAsync(selectedAuthor.Id);
                MessageBox.Show($"Author '{selectedAuthor.Name}' and related data have been deleted.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadAuthorsAsync();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Failed to delete author: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
