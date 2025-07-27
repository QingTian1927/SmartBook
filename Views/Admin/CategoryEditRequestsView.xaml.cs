using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartBook.Core.DTOs;
using SmartBook.Core.Interfaces;
using SmartBook.Core.Data;
using SmartBook.Core.Services;
using SmartBook.Views;
using SmartBook.Views.Admin;

namespace SmartBook.Views.Admin
{
    public partial class CategoryEditRequestsView : Page
    {
        private readonly IEditRequestService _editRequestService;
        private readonly SmartBookDbContext _db;
        private readonly EmailService _emailService;

        private List<CategoryEditRequestDisplayModel> _requests = new();
        private CategoryEditRequestDisplayModel? _currentRequest = null;

        public CategoryEditRequestsView()
        {
            InitializeComponent();

            _editRequestService = App.AppHost.Services.GetRequiredService<IEditRequestService>();
            _db = App.AppHost.Services.GetRequiredService<SmartBookDbContext>();
            _emailService = EmailService.Instance;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadPendingRequestsAsync();

            ToggleHistoryButton.Visibility = Visibility.Visible;
            ToggleHistoryButton.IsChecked = false;
            ToggleHistoryButton.Content = "Show History";
            HistoryDataGrid.Visibility = Visibility.Collapsed;
            HistoryDataGrid.ItemsSource = null;
        }

        private async Task LoadPendingRequestsAsync()
        {
            _requests = await _editRequestService.GetCategoryEditRequestsAsync(status: "Pending");
            RequestsDataGrid.ItemsSource = _requests;
            RequestsDataGrid.SelectedItem = null;
            SetDetailPanelVisibility(Visibility.Collapsed);
        }

        private void RequestsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentRequest = RequestsDataGrid.SelectedItem as CategoryEditRequestDisplayModel;
            FillDetailPanel();
        }

        private void FillDetailPanel()
        {
            if (_currentRequest == null)
            {
                SetDetailPanelVisibility(Visibility.Collapsed);
                return;
            }

            SetDetailPanelVisibility(Visibility.Visible);

            CurrentNameText.Text = _currentRequest.CurrentCategoryName ?? "";
            ProposedNameText.Text = string.IsNullOrWhiteSpace(_currentRequest.ProposedName)
                ? "(no change)"
                : _currentRequest.ProposedName;
            StatusText.Text = _currentRequest.Status ?? "";

            RequestedByText.Text = _currentRequest.RequestedByUsername;
            RequestedAtText.Text = _currentRequest.RequestedAt.ToString("yyyy-MM-dd HH:mm");

            ReviewCommentBox.Text = _currentRequest.ReviewComment ?? "";

            bool canReview = _currentRequest.Status == "Pending";
            ApproveBtn.IsEnabled = canReview;
            RejectBtn.IsEnabled = canReview;
            ReviewCommentBox.IsEnabled = canReview;
        }

        private void SetDetailPanelVisibility(Visibility visibility)
        {
            if (DetailBorder != null)
                DetailBorder.Visibility = visibility;
        }

        private async Task LoadHistoryAsync()
        {
            var pastRequests = await _db.CategoryEditRequests
                .AsNoTracking()
                .Include(r => r.RequestedByUser)
                .Include(r => r.Category)
                .Where(r => r.Status != "Pending")
                .OrderByDescending(r => r.RequestedAt)
                .Select(r => new CategoryEditRequestDisplayModel
                {
                    Id = r.Id,
                    ProposedName = r.ProposedName,
                    Status = r.Status,
                    ReviewComment = r.ReviewComment,
                    RequestedAt = r.RequestedAt.ToLocalTime(),
                    RequestedByUsername = r.RequestedByUser.Username,
                    CurrentCategoryName = r.Category.Name,
                })
                .ToListAsync();

            HistoryDataGrid.ItemsSource = pastRequests;
        }

        private async void ToggleHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (ToggleHistoryButton.IsChecked == true)
            {
                HistoryDataGrid.Visibility = Visibility.Visible;
                ToggleHistoryButton.Content = "Hide History";
                await LoadHistoryAsync();
            }
            else
            {
                HistoryDataGrid.Visibility = Visibility.Collapsed;
                ToggleHistoryButton.Content = "Show History";
            }
        }

        private async Task<string?> GetUserEmailByUsernameAsync(string username)
        {
            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);
            return user?.Email;
        }

        private async void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentRequest == null)
                return;

            bool success = await _editRequestService.ReviewCategoryEditRequestAsync(
                _currentRequest.Id,
                true,
                ReviewCommentBox.Text);

            if (success)
            {
                MessageBox.Show("Request approved and category updated!", "Success", MessageBoxButton.OK,
                    MessageBoxImage.Information);

                await SendNotificationEmailAsync(true);

                await LoadPendingRequestsAsync();

                SetDetailPanelVisibility(Visibility.Collapsed);
                HistoryDataGrid.ItemsSource = null;
                HistoryDataGrid.Visibility = Visibility.Collapsed;
                ToggleHistoryButton.IsChecked = false;
                ToggleHistoryButton.Content = "Show History";
            }
            else
            {
                MessageBox.Show("Failed to approve the request. Please try again.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentRequest == null)
                return;

            bool success = await _editRequestService.ReviewCategoryEditRequestAsync(
                _currentRequest.Id,
                false,
                ReviewCommentBox.Text);

            if (success)
            {
                MessageBox.Show("Request rejected.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                await SendNotificationEmailAsync(false);

                await LoadPendingRequestsAsync();

                SetDetailPanelVisibility(Visibility.Collapsed);
                HistoryDataGrid.ItemsSource = null;
                HistoryDataGrid.Visibility = Visibility.Collapsed;
                ToggleHistoryButton.IsChecked = false;
                ToggleHistoryButton.Content = "Show History";
            }
            else
            {
                MessageBox.Show("Failed to reject the request. Please try again.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task SendNotificationEmailAsync(bool approved)
        {
            if (_currentRequest == null)
                return;

            try
            {
                string? userEmail = await GetUserEmailByUsernameAsync(_currentRequest.RequestedByUsername);
                if (string.IsNullOrWhiteSpace(userEmail))
                {
                    Console.WriteLine(
                        $"Cannot send notification email: Email for user '{_currentRequest.RequestedByUsername}' not found.");
                    return;
                }

                string status = approved ? "Approved" : "Rejected";

                await _emailService.SendEditRequestResultEmailAsync(
                    userEmail: userEmail,
                    requestType: "Category",
                    itemName: _currentRequest.CurrentCategoryName ?? "(unknown)",
                    status: status,
                    reviewComment: ReviewCommentBox.Text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send notification email: {ex}");
            }
        }
    }
}