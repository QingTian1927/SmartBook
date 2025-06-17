using System.Windows;
using System.Windows.Controls;
using SmartBook.Utils;

namespace SmartBook.Views;

public partial class LoginView : Page
{
    public LoginView()
    {
        InitializeComponent();
    }

    private void BtnSignIn_Click(object sender, RoutedEventArgs e)
    {
        string email = txtEmail.Text;
        string password = txtPassword.Password;

        if (!Validator.IsValidEmail(email))
        {
            MessageBox.Show(
                "Please enter a valid email address",
                "Login Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return;
        }

        if (!Validator.IsValidPassword(password))
        {
            MessageBox.Show(
                "Invalid password.\nPassword must be 3 to 32 characters long and can include letters, numbers, and special characters (!@#$%^&*()-_=+[]{}|:;'\",.<>?/).",
                "Login Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return;
        }
        
        
    }

    private void BtnRegister_Click(object sender, RoutedEventArgs e)
    {   
        MainWindow.Instance.Title = "SmartBook - Login";
        MainWindow.Instance.Navigate(new SignUpView());
    }
}