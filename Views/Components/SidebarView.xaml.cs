using System.Windows;
using System.Windows.Controls;
using SmartBook.Core.Data;
using SmartBook.Views.Admin;

namespace SmartBook.Views.Components;

public partial class SidebarView : UserControl
{
    public SidebarView()
    {
        InitializeComponent();
    }
    
    public string CurrentUsername
    {
        get => (string)GetValue(CurrentUsernameProperty);
        set => SetValue(CurrentUsernameProperty, value);
    }
    
    private void SidebarView_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (ContextManager.IsAdmin)
        {
            CurrentUsername = "SmartBook Admin";
            
            RecommendationBtn.Visibility = Visibility.Collapsed;
            SettingsBtn.Visibility = Visibility.Collapsed;
            return;
        }
        
        AuthorEditRequestsBtn.Visibility = Visibility.Collapsed;
        CategoryEditRequestsBtn.Visibility = Visibility.Collapsed;
    }

    public static readonly DependencyProperty CurrentUsernameProperty =
        DependencyProperty.Register(nameof(CurrentUsername), typeof(string), typeof(SidebarView),
            new PropertyMetadata(string.Empty));

    private void LogoutBtn_Click(object sender, RoutedEventArgs e)
    {
        ContextManager.CurrentUser = null;
        ContextManager.IsAdmin = false;

        MainWindow.Instance.Navigate(new LoginView());
        MessageBox.Show(
            "You are logged out!",
            "Logout Successful",
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );
    }

    private void SettingsBtn_Click(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void RecommendationBtn_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Title = "SmartBook - Recommendations";
        MainWindow.Instance.Navigate(new RecommendationView());
    }

    private void HomeBtn_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Title = "SmartBook - Dashboard";
        MainWindow.Instance.Navigate(new DashboardView());
    }

    private void AuthorEditRequestsBtn_OnClickBtn_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Navigate(new AuthorEditRequestsView());
    }

    private void CategoryEditRequestsBtn_OnClickBtn_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Navigate(new CategoryEditRequestsView());
    }

    private void AuthorsBtn_OnClickBtn_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Navigate(new AuthorsView());
    }

    private void CategoriesBtn_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Navigate(new CategoriesView());
    }
}