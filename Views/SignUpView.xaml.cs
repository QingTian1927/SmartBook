using System.Windows;
using System.Windows.Controls;

namespace SmartBook.Views;

public partial class SignUpView : Page
{
    public SignUpView()
    {
        InitializeComponent();
    }

    private void BtnCreateAccount_Click(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void BtnBackToLogin_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Title = "SmartBook - Sign Up";
        MainWindow.Instance.Navigate(new LoginView());
    }
}