using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SmartBook.Core.Data;
using SmartBook.Core.Interfaces;
using SmartBook.Core.Services;

namespace SmartBook.Views;

public partial class ConfirmResetPasswordView : Page
{
    private readonly DispatcherTimer _resendTimer;
    private int _countdownSeconds = 60;

    private IEmailService _emailService = EmailService.Instance;

    public ConfirmResetPasswordView()
    {
        InitializeComponent();

        _resendTimer = new DispatcherTimer();
        _resendTimer.Interval = TimeSpan.FromSeconds(1);
        _resendTimer.Tick += ResendTimer_Tick;
        StartResendCooldown();
    }

    private void StartResendCooldown()
    {
        ResendButton.IsEnabled = false;
        _countdownSeconds = 60;
        ResendButton.Content = $"Resend Email ({_countdownSeconds}s)";
        _resendTimer.Start();
    }

    private void ResendTimer_Tick(object? sender, EventArgs e)
    {
        _countdownSeconds--;
        ResendButton.Content = $"Resend Email ({_countdownSeconds}s)";

        if (_countdownSeconds <= 0)
        {
            _resendTimer.Stop();
            ResendButton.Content = "Resend Email";
            ResendButton.IsEnabled = true;
        }
    }

    private async void ResendEmail_Click(object sender, RoutedEventArgs e)
    {
        if (ContextManager.PasswordResetEmail is null || ContextManager.PasswordResetCode is null)
        {
            MessageBox.Show(
                "Invalid reset password request configuration. Please try again.",
                "Password Reset Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            return;
        }

        try
        {
            await _emailService.SendPasswordResetEmailAsync(ContextManager.PasswordResetEmail,
                ContextManager.PasswordResetCode);
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

        MessageBox.Show("Confirmation email resent!");
        StartResendCooldown();
    }

    private void VerifyCode_Click(object sender, RoutedEventArgs e)
    {
        if (ContextManager.PasswordResetEmail is null || ContextManager.PasswordResetCode is null)
        {
            MessageBox.Show(
                "Invalid reset password request configuration. Please try again.",
                "Password Reset Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            return;
        }

        string code = CodeInput.Text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            MessageBox.Show("Please enter the password reset code.");
            return;
        }

        if (!code.Equals(ContextManager.PasswordResetCode, StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(
                "Incorrect password reset code. Please try again.",
                "Password Reset Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return;
        }

        MainWindow.Instance.Title = "SmartBook - Change Password";
        MainWindow.Instance.Navigate(new ChangePasswordView());
    }
}