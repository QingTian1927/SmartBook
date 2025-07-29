using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartBook.Core.Data;
using SmartBook.Core.Interfaces;
using SmartBook.Core.Services;

namespace SmartBook
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IHost AppHost { get; private set; }
        
        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<SmartBookDbContext>();
                    services.AddSingleton<IBookService, BookService>();
                    services.AddSingleton<IAuthService, AuthService>();
                    services.AddSingleton<IRecommendationService, RecommendationService>();
                    services.AddSingleton<IGeminiService, GeminiService>();
                    services.AddSingleton<IEditRequestService, EditRequestService>();
                    services.AddSingleton<IStatisticsService, StatisticsService>();
                })
                .Build();
        }
        
        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost.StartAsync();
            base.OnStartup(e);
            DotNetEnv.Env.Load();
        }
        
        protected override async void OnExit(ExitEventArgs e)
        {
            using (AppHost)
            {
                await AppHost.StopAsync(TimeSpan.FromSeconds(5));
            }
            base.OnExit(e);
        }
    }
}
