using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.Configuration;
using SmartBook.Core.Data;
using SmartBook.Core.Interfaces;
using SmartBook.Core.Models;
using SmartBook.Core.Services;
using SmartBook.Utils;

namespace SmartBook.Views;

public partial class LoginView : Page
{
    private readonly IAuthService _authService = AuthService.Instance;
    private readonly IConfiguration _configuration = ContextManager.Configuration;

    public LoginView()
    {
        InitializeComponent();
    }

    private async Task<bool> AuthenticateNormalUser(string email, string password)
    {
        User? user = await _authService.AuthenticateAsync(email, password);
        if (user is null)
        {
            MessageBox.Show(
                "Invalid login credentials. Please check your information.",
                "Login Unsuccessful",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return false;
        }

        ContextManager.CurrentUser = user;
        ContextManager.IsAdmin = false;
        MainWindow.Instance.Navigate(new DashboardView());

        return true;
    }

    // ReSharper disable once AsyncVoidMethod
    private async void BtnSignIn_Click(object sender, RoutedEventArgs e)
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

        string? adminEmail = _configuration["Admin:email"];
        string? adminPassword = _configuration["Admin:password"];

        if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword))
        {
            if (adminEmail.Equals(email) && AuthService.IsMatchingPassword(password, adminPassword))
            {
                ContextManager.IsAdmin = true;
                ContextManager.CurrentUser = null;

                MessageBox.Show("Admin Login Successful");
                MainWindow.Instance.Navigate(new DashboardView());
                return;
            }
        }

        await AuthenticateNormalUser(email, password);
    }

    private void BtnRegister_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Title = "SmartBook - Login";
        MainWindow.Instance.Navigate(new SignUpView());
    }

    private void ForgotPassword_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Title = "SmartBook - ForgotPassword";
        MainWindow.Instance.Navigate(new ForgotPasswordView());
    }
}