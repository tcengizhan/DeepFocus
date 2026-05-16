using System.Windows;
using DeepFocus.Services;
using DeepFocus.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace DeepFocus;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    private void OnStartup(object sender, StartupEventArgs e)
    {
        var trCulture = new System.Globalization.CultureInfo("tr-TR");
        System.Threading.Thread.CurrentThread.CurrentCulture = trCulture;
        System.Threading.Thread.CurrentThread.CurrentUICulture = trCulture;
        FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(System.Windows.Markup.XmlLanguage.GetLanguage("tr-TR")));

        var services = new ServiceCollection();
        ConfigureServices(services);
        ApplicationThemeManager.Apply(ApplicationTheme.Dark, WindowBackdropType.Mica, updateAccent: false);

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
