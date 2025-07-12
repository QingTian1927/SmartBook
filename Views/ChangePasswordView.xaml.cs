using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SmartBook.Core.Data;
using SmartBook.Core.Interfaces;
using SmartBook.Core.Models;
using SmartBook.Core.Services;

namespace SmartBook.Views;

public partial class ChangePasswordView : Page
{
    private readonly IAuthService _authService;
    
    public ChangePasswordView()
    {
        InitializeComponent();
        _authService = App.AppHost.Services.GetRequiredService<IAuthService>();
    }

    private async void ChangePassword_Click(object sender, RoutedEventArgs e)
    {
        if (ContextManager.PasswordResetEmail is null)
        {
            MessageBox.Show(
                "Invalid reset password request configuration. Please try again.",
                "Password Reset Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            return;
        }

        var password = NewPasswordBox.Password;
        var confirmPassword = ConfirmPasswordBox.Password;

        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            MessageBox.Show("Please enter your new password.");
            return;
        }

        if (!password.Equals(confirmPassword))
        {
            MessageBox.Show("Passwords do not match.");
            return;
        }

        User? user = await _authService.GetUserByEmailAsync(ContextManager.PasswordResetEmail);
        if (user is null)
        {
            MessageBox.Show(
                "No account associated with your email address was found",
                "Password Reset Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            return;
        }
        
        user.Password = BCrypt.Net.BCrypt.HashPassword(password);
        await _authService.UpdateUserAsync(user);

        MessageBox.Show("Successfully changed account password. Please try to log into your account now.");
        MainWindow.Instance.Title = "SmartBook - Login";
        MainWindow.Instance.Navigate(new LoginView());
    }
}