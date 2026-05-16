using System.Windows;
using DeepFocus.Services;
using DeepFocus.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DeepFocus;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    private void OnStartup(object sender, StartupEventArgs e)
    {
        var services = new ServiceCollection();
        ConfigureServices(services);

        _serviceProvider = services.BuildServiceProvider();
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ITimerService, TimerService>();
        services.AddSingleton<ISessionService, SessionService>();
        services.AddSingleton<IGoalService, GoalService>();

        services.AddSingleton<ClockViewModel>();
        services.AddSingleton<StopwatchViewModel>();
        services.AddSingleton<CountdownViewModel>();
        services.AddSingleton<StatisticsViewModel>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>();
    }

    private void OnExit(object sender, ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
    }
}
