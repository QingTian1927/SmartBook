using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SmartBook.Core.Interfaces;
using SmartBook.Core.Models;
using SmartBook.Core.Services;
using SmartBook.Utils;

namespace SmartBook.Views;

public partial class SignUpView : Page
{
    private readonly IEmailService _emailService = EmailService.Instance;
    private readonly IAuthService _authService;

    public SignUpView()
    {
        InitializeComponent();
        _authService = App.AppHost.Services.GetRequiredService<IAuthService>();
    }

    // ReSharper disable once AsyncVoidMethod
    private async void BtnCreateAccount_Click(object sender, RoutedEventArgs e)
    {
        string username = txtUsername.Text;
        string email = txtEmail.Text;
        string password = txtPassword.Password;

        if (!Validator.IsValidUsername(username))
        {
            MessageBox.Show(
                "Invalid username.\nUsername must be 4 to 99 characters long and may contain only letters, numbers, and underscores (A-Z, a-z, 0-9, _).",
                "Sign Up Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return;
        }

        if (!Validator.IsValidEmail(email))
        {
            MessageBox.Show(
                "Please enter a valid email address",
                "Sign Up Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return;
        }

        if (!Validator.IsValidPassword(password))
        {
            MessageBox.Show(
                "Invalid password.\nPassword must be 3 to 32 characters long and can include letters, numbers, and special characters (!@#$%^&*()-_=+[]{}|:;'\",.<>?/).",
                "Sign Up Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return;
        }

        User user = new User()
        {
            Username = username,
            Email = email,
            Password = BCrypt.Net.BCrypt.HashPassword(password)
        };

        bool registered = await _authService.RegisterUserAsync(user);
        if (registered)
        {
            try
            {
                await _emailService.SendAccountCreatedEmailAsync(user.Email);
                MainWindow.Instance.Navigate(new LoginView());
                MessageBox.Show("Registration successful!");
            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show(
                    "Failed to send confirmation email:\n\n" + ex,
                    "Registration Failure",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            return;
        }

        MessageBox.Show("Registration failed! Please try again.");
    }

    private void BtnBackToLogin_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Title = "SmartBook - Sign Up";
        MainWindow.Instance.Navigate(new LoginView());
    }
}