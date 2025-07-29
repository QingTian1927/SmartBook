using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SmartBook.Core.Data;
using SmartBook.Core.DTOs;
using SmartBook.Core.Interfaces;
using SmartBook.Core.Models;

namespace SmartBook.Views.Admin
{
    public partial class AdminDashboardView : Page
    {
        private readonly IStatisticsService _statisticsService;

        public SystemStatisticsDTO Statistics { get; set; } = new(); // Bind this in XAML

        public string CurrentUsername =>
            ContextManager.CurrentUser != null ? ContextManager.CurrentUser.Username : "<UNKNOWN>";

        public AdminDashboardView()
        {
            InitializeComponent();

            _statisticsService = App.AppHost.Services.GetRequiredService<IStatisticsService>();
            DataContext = this;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Statistics = await _statisticsService.GetStatisticsAsync();
            DataContext = null;
            DataContext = this;
        }
    }
}