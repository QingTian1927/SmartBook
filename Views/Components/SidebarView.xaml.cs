using System.Windows;
using System.Windows.Controls;
using SmartBook.Core.Data;

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
        throw new NotImplementedException();
    }

    private void HistoryBtn_Click(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}