using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SmartBook.Core.DTOs;
using SmartBook.Core.Interfaces;

namespace SmartBook.Views.Admin
{
    public partial class ManageCategoriesView : Page
    {
        private readonly IBookService _bookService;

        private ObservableCollection<CategoryViewModel> _categories = new ObservableCollection<CategoryViewModel>();
        private ObservableCollection<CategoryViewModel> _filteredCategories = new ObservableCollection<CategoryViewModel>();

        private readonly HashSet<CategoryViewModel> _editedCategories = new HashSet<CategoryViewModel>();
        private readonly HashSet<CategoryViewModel> _newCategories = new HashSet<CategoryViewModel>();

        public ManageCategoriesView()
        {
            InitializeComponent();
            _bookService = App.AppHost.Services.GetRequiredService<IBookService>();
            DataContext = this;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCategoriesAsync();
        }

        private async Task LoadCategoriesAsync()
        {
            var categoriesFromService = await _bookService.GetAllCategoriesAsync();
            _categories = new ObservableCollection<CategoryViewModel>(categoriesFromService.Select(c => new CategoryViewModel(c)));
            _filteredCategories = new ObservableCollection<CategoryViewModel>(_categories);
            CategoriesDataGrid.ItemsSource = _filteredCategories;

            _editedCategories.Clear();
            _newCategories.Clear();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var keyword = SearchBox.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(keyword))
            {
                _filteredCategories = new ObservableCollection<CategoryViewModel>(_categories);
            }
            else
            {
                _filteredCategories = new ObservableCollection<CategoryViewModel>(
                    _categories.Where(c => c.Name.ToLower().Contains(keyword))
                );
            }
            CategoriesDataGrid.ItemsSource = _filteredCategories;
        }

        private void CategoriesDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if(e.EditAction == DataGridEditAction.Commit && e.Row.Item is CategoryViewModel edited)
            {
                if (string.IsNullOrWhiteSpace(edited.Name))
                {
                    MessageBox.Show("Category name cannot be empty.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _ = LoadCategoriesAsync();
                    return;
                }

                if (edited.Id == 0)
                {
                    // New category edited, mark for addition
                    _newCategories.Add(edited);
                }
                else
                {
                    // Existing category edited
                    _editedCategories.Add(edited);
                }
            }
        }

        private void CategoriesDataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            // Provide a new empty CategoryViewModel for new row
            var newCategory = new CategoryViewModel()
            {
                Id = 0,
                Name = string.Empty,
                BookCount = 0
            };
            e.NewItem = newCategory;
        }

        private async void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            int addedCount = 0;
            int updatedCount = 0;

            // Save new categories
            foreach (var newCat in _newCategories.ToList())
            {
                if (string.IsNullOrWhiteSpace(newCat.Name))
                {
                    MessageBox.Show("New category name cannot be empty.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue;
                }
                bool added = await AddCategoryAsync(newCat.Name);
                if (added)
                {
                    addedCount++;
                    _newCategories.Remove(newCat);
                }
                else
                {
                    MessageBox.Show($"Category '{newCat.Name}' already exists or failed to add.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // Save edited categories
            foreach (var editedCat in _editedCategories.ToList())
            {
                bool updated = await UpdateCategoryAsync(editedCat);
                if (updated)
                {
                    updatedCount++;
                    _editedCategories.Remove(editedCat);
                }
            }

            MessageBox.Show($"Added {addedCount} new categories and updated {updatedCount} categories.", "Save Completed", MessageBoxButton.OK, MessageBoxImage.Information);
            await LoadCategoriesAsync();
        }

        private async Task<bool> AddCategoryAsync(string name)
        {
            var success = await _bookService.TryAddCategory(name.Trim());
            return success;
        }

        private async Task<bool> UpdateCategoryAsync(CategoryViewModel categoryViewModel)
        {
            try
            {
                var catEntity = await _bookService.GetCategoryByIdAsync(categoryViewModel.Id);
                if (catEntity == null)
                {
                    MessageBox.Show($"Category (Id {categoryViewModel.Id}) not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                var duplicate = _categories.FirstOrDefault(c => c.Name.ToLower() == categoryViewModel.Name.ToLower() && c.Id != categoryViewModel.Id);
                if (duplicate != null)
                {
                    MessageBox.Show($"Category name '{categoryViewModel.Name}' is already in use.", "Duplicate Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                catEntity.Name = categoryViewModel.Name.Trim();
                await _bookService.UpdateCategory(catEntity);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (CategoriesDataGrid.SelectedItem is not CategoryViewModel selected)
            {
                MessageBox.Show("Please select a category to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete '{selected.Name}' and all its associated books and data?",
                                         "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                await _bookService.DeleteCategoryCascade(selected.Id);
                MessageBox.Show($"Category '{selected.Name}' and its related data deleted.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadCategoriesAsync();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Failed to delete category: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Helper view model that supports change tracking etc.
        public class CategoryViewModel : CategoryDisplayModel
        {
            // Implement INotifyPropertyChanged if desired for further enhancements

            // No additional properties now
            public CategoryViewModel() : base() { }

            public CategoryViewModel(CategoryDisplayModel dm)
            {
                Id = dm.Id;
                Name = dm.Name;
                BookCount = dm.BookCount;
            }
        }
    }
}
