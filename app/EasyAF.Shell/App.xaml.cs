using System.Collections.ObjectModel;
using System.Windows;
using Prism.Unity;
using Prism.Ioc;
using Unity;
using EasyAF.Core.Contracts;
using EasyAF.Core.Services;
using EasyAF.Core.Logging;
using EasyAF.Shell.Services;
using EasyAF.Shell.ViewModels;
using Serilog;

namespace EasyAF.Shell;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : PrismApplication
{
    private static readonly ObservableCollection<LogEntry> _logEntries = new();

    protected override void OnStartup(StartupEventArgs e)
    {
        // Configure Serilog with multiple sinks including in-memory for UI display
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/easyaf-.log", rollingInterval: RollingInterval.Day)
            .WriteTo.Debug()
            .WriteTo.Sink(new InMemoryLogSink(_logEntries))
            .CreateLogger();

        Log.Information("EasyAF v3 starting up");

        base.OnStartup(e);

        // Load settings and apply saved theme
        var settingsService = Container.Resolve<ISettingsService>();
        var themeService = Container.Resolve<IThemeService>();
        
        var savedTheme = settingsService.GetSetting("Theme", "Light");
        themeService.ApplyTheme(savedTheme);
        
        Log.Information("Applied theme: {Theme}", savedTheme);
    }

    protected override Window CreateShell()
    {
        var mainWindow = Container.Resolve<MainWindow>();
        var viewModel = Container.Resolve<MainWindowViewModel>();
        mainWindow.DataContext = viewModel;
        return mainWindow;
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // Register core services
        containerRegistry.RegisterSingleton<EasyAF.Core.Contracts.IModuleCatalog, EasyAF.Core.Services.ModuleCatalog>();
        containerRegistry.RegisterSingleton<ISettingsService, SettingsManager>();
        containerRegistry.RegisterSingleton<IThemeService, ThemeService>();
        containerRegistry.RegisterSingleton<ILoggerService, LoggerService>();
        
        // Register shared log entries collection
        containerRegistry.RegisterInstance(_logEntries);
        
        // Register ViewModels
        containerRegistry.Register<MainWindowViewModel>();
        containerRegistry.Register<LogViewerViewModel>();
        
        Log.Information("Services registered with Unity container");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("EasyAF v3 shutting down");
        
        // Save settings on exit
        var settingsService = Container.Resolve<ISettingsService>();
        settingsService.Save();
        
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}

