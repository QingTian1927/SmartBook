using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;
using SmartBook.Core.Data;
using SmartBook.Core.Interfaces;
using SmartBook.Core.Models;
using SmartBook.Core.Services;
using SmartBook.Utils;

namespace SmartBook.Views;

public partial class ForgotPasswordView : Page
{
    private readonly IAuthService _authService = AuthService.Instance;
    private readonly IEmailService _emailService = EmailService.Instance;

    public ForgotPasswordView()
    {
        InitializeComponent();
    }

    private void LoginLink_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Title = "SmartBook - Login";
        MainWindow.Instance.Navigate(new LoginView());
    }

    private static void ShowSentEmailMsgBox()
    {
        MessageBox.Show(
            "If there is an associated account with your email address, a password reset email will be sent to your email.\n Make sure to check your spam inbox as well.");
    }

    // ReSharper disable once AsyncVoidMethod
    private async void ResetPasswordBtn_Click(object sender, RoutedEventArgs e)
    {
        string? email = EmailTextBox.Text;
        if (string.IsNullOrEmpty(email))
        {
            MessageBox.Show("Please enter a valid email address.");
            return;
        }

        if (!await _authService.ExistsUser(email))
        {
            ShowSentEmailMsgBox();
            return;
        }

        ContextManager.PasswordResetCode =
            Randomizer.GenerateRandomString(EmailService.PasswordResetCodeLength, Randomizer.AlphaNumeric);

        try
        {
            await _emailService.SendPasswordResetEmailAsync(email, ContextManager.PasswordResetCode);
        }
        catch (SmtpException ex)
        {
            MessageBox.Show(
                "Failed to resend password reset email. Please try again.\n\n" + ex,
                "Password Reset Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            return;
        }
        
        ShowSentEmailMsgBox();
        ContextManager.PasswordResetEmail = email;
        
        MainWindow.Instance.Title = "SmartBook - Reset Password";
        MainWindow.Instance.Navigate(new ConfirmResetPasswordView());
    }
}