using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SmartBook.Core.Data;
using SmartBook.Core.Models;
using Microsoft.EntityFrameworkCore;
using SmartBook.Core.DTOs;

namespace SmartBook.Views.Admin
{
    public partial class ManageUsersView : Page
    {
        private readonly SmartBookDbContext _dbContext;
        private List<UserDisplayModel> _allUsers = new();
        private List<UserDisplayModel> _filteredUsers = new();

        private readonly HashSet<UserDisplayModel> _editedUsers = new();

        public string CurrentUsername => ContextManager.CurrentUser?.Username ?? "<UNKNOWN>";

        public ManageUsersView()
        {
            InitializeComponent();
            _dbContext = App.AppHost.Services.GetRequiredService<SmartBookDbContext>();
            DataContext = this;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUsersAsync();
        }

        private async Task LoadUsersAsync()
        {
            var users = await _dbContext.Users
                .Include(u => u.UserBooks)
                .AsNoTracking()
                .ToListAsync();

            _allUsers = users.Select(u => new UserDisplayModel
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                IsBanned = u.IsBanned,
                UserBooksCount = u.UserBooks.Count
            }).ToList();

            _filteredUsers = new List<UserDisplayModel>(_allUsers);
            UsersDataGrid.ItemsSource = _filteredUsers;
            _editedUsers.Clear();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var keyword = SearchBox.Text.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                _filteredUsers = new List<UserDisplayModel>(_allUsers);
            }
            else
            {
                _filteredUsers = _allUsers
                    .Where(u =>
                        (u.Username != null && u.Username.ToLower().Contains(keyword)) ||
                        (u.Email != null && u.Email.ToLower().Contains(keyword))
                    ).ToList();
            }
            UsersDataGrid.ItemsSource = _filteredUsers;
        }

        private void UsersDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // Track edits only if IsBanned changed
            if (e.Column.Header.ToString() == "Banned" && e.Row.Item is UserDisplayModel user)
            {
                _editedUsers.Add(user);
            }
        }

        private async void SaveChangesBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_editedUsers.Count == 0)
            {
                MessageBox.Show("No changes to save.", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int updatedCount = 0;

            foreach (var editedUser in _editedUsers.ToList())
            {
                var userEntity = await _dbContext.Users
                    .Include(u => u.AuthorEditRequestRequestedByUsers)
                    .Include(u => u.CategoryEditRequestRequestedByUsers)
                    .FirstOrDefaultAsync(u => u.Id == editedUser.Id);

                if (userEntity == null)
                {
                    _editedUsers.Remove(editedUser);
                    continue;
                }

                if (userEntity.IsBanned != editedUser.IsBanned)
                {
                    userEntity.IsBanned = editedUser.IsBanned;

                    if (userEntity.IsBanned)
                    {
                        // Remove all edit requests requested by this user (author and category)
                        _dbContext.AuthorEditRequests.RemoveRange(
                            await _dbContext.AuthorEditRequests
                                .Where(r => r.RequestedByUserId == userEntity.Id)
                                .ToListAsync());
                        _dbContext.CategoryEditRequests.RemoveRange(
                            await _dbContext.CategoryEditRequests
                                .Where(r => r.RequestedByUserId == userEntity.Id)
                                .ToListAsync());
                    }

                    updatedCount++;
                }
                _editedUsers.Remove(editedUser);
            }

            try
            {
                await _dbContext.SaveChangesAsync();
                MessageBox.Show($"Saved changes for {updatedCount} user(s).", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadUsersAsync();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Failed to save changes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            await LoadUsersAsync();
        }
    }
}
