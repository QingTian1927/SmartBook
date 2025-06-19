using System.Windows;
using System.Windows.Controls;

namespace SmartBook.Views;

public partial class ForgotPasswordView : Page
{
    public ForgotPasswordView()
    {
        InitializeComponent();
    }

    private void LoginLink_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Title = "SmartBook - Login";
        MainWindow.Instance.Navigate(new LoginView());
    }
}