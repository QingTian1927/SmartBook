using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SmartBook.Core.Data;
using SmartBook.Core.DTOs;
using SmartBook.Core.Interfaces;

namespace SmartBook.Views
{
    public partial class CategoriesView : Page
    {
        private readonly IBookService _bookService;
        private List<CategoryDisplayModel> _allCategories = new();

        public string CurrentUsername => ContextManager.CurrentUser?.Username ?? "<UNKNOWN>";

        public CategoriesView()
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
            var categories = await _bookService.GetAllCategoriesAsync();
            _allCategories = categories.ToList();
            CategoryGrid.ItemsSource = _allCategories;
        }

        private void SearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var keyword = SearchBox.Text.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                CategoryGrid.ItemsSource = _allCategories;
            }
            else
            {
                CategoryGrid.ItemsSource = _allCategories.Where(c => c.Name.ToLower().Contains(keyword)).ToList();
            }
        }

        private void CategoryItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CategoryDisplayModel category)
            {
                MainWindow.Instance.Title = $"SmartBook - Category: {category.Name}";
                MainWindow.Instance.Navigate(new CategoryDetailView(category.Id));
            }
        }
    }
}
