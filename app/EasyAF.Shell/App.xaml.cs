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
using Fluent;
using System.Linq;
using System; // for EventHandler

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

        // Hook module loaded event for service-side tab aggregation and help registration
        var ribbonService = Container.Resolve<IModuleRibbonService>();
        var loader = Container.Resolve<IModuleLoader>();
        
        // CROSS-MODULE EDIT: 2025-01-15 Map Module Integration
        // Modified for: Pass Unity container to ModuleLoader so modules can register services
        // Related modules: Core (ModuleLoader), Map (MapModule service registration)
        // Rollback instructions: Remove SetContainer call
        if (loader is Core.Services.ModuleLoader moduleLoader)
        {
            moduleLoader.SetContainer(Container.GetContainer());
        }
        
        loader.ModuleLoaded += (object? sender, EasyAF.Core.Contracts.IModule module) => 
        {
            ribbonService.AddModuleTabs(module, null);
            // Register help pages if provided
            var helpCatalog = Container.Resolve<IHelpCatalog>();
            helpCatalog.RegisterModule(module);
        };
        loader.DiscoverAndLoadModules();
    }

    protected override Window CreateShell()
    {
        var mainWindow = Container.Resolve<MainWindow>();
        var viewModel = Container.Resolve<MainWindowViewModel>();
        mainWindow.DataContext = viewModel;

        // CROSS-MODULE EDIT: 2025-11-10 SANITY CHECK (A1/A3)
        // Modified for: Dynamic module Ribbon tab injection & ensuring Help tab is last
        // Related modules: Shell (ModuleRibbonService, MainWindow), Core (IDocumentModule)
        // Rollback instructions: Remove SyncTabsToRibbon logic and TabsChanged subscription; rely on static XAML only
        var ribbonService = Container.Resolve<IModuleRibbonService>();

        void SyncTabsToRibbon()
        {
            var ribbon = mainWindow.MainRibbon; // Name from XAML
            if (ribbon == null) return;

            // Inject any new module-provided tabs
            foreach (var tab in ribbonService.Tabs)
            {
                if (!ribbon.Tabs.Contains(tab))
                {
                    ribbon.Tabs.Add(tab);
                }
            }

            // Ensure Help tab is last (if present)
            var helpTab = ribbon.Tabs.FirstOrDefault(t => (t.Header as string)?.Equals("Help", StringComparison.OrdinalIgnoreCase) == true);
            if (helpTab != null && ribbon.Tabs.LastOrDefault() != helpTab)
            {
                ribbon.Tabs.Remove(helpTab);
                ribbon.Tabs.Add(helpTab);
            }
        }

        // Initial sync and subscribe for future changes
        SyncTabsToRibbon();
        ribbonService.TabsChanged += (_, __) => SyncTabsToRibbon();

        return mainWindow;
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // Register core services
        containerRegistry.RegisterSingleton<EasyAF.Core.Contracts.IModuleCatalog, EasyAF.Core.Services.ModuleCatalog>();
        containerRegistry.RegisterSingleton<ISettingsService, SettingsManager>();
        containerRegistry.RegisterSingleton<IThemeService, ThemeService>();
        containerRegistry.RegisterSingleton<ILoggerService, LoggerService>();
        containerRegistry.RegisterSingleton<IModuleLoader, ModuleLoader>();
        containerRegistry.RegisterSingleton<IModuleRibbonService, ModuleRibbonService>();
        containerRegistry.RegisterSingleton<IRecentFilesService, RecentFilesService>();
        containerRegistry.RegisterSingleton<IDocumentManager, DocumentManager>();
        containerRegistry.RegisterSingleton<IDialogService, DialogService>();
        // CROSS-MODULE EDIT: 2025-01-11 Task 10
        // Modified for: Register UserDialogService for dirty-close confirmation dialogs
        // Related modules: Core (IUserDialogService), Shell (UserDialogService)
        // Rollback instructions: Remove UserDialogService registration line below
        containerRegistry.RegisterSingleton<IUserDialogService, UserDialogService>();
        // CROSS-MODULE EDIT: 2025-01-11 SANITY CHECK (Help system scaffold)
        // Modified for: Register IHelpCatalog to aggregate optional module help pages (IHelpProvider)
        // Related modules: Core (IHelpProvider), Shell (HelpCatalog)
        // Rollback instructions: Remove IHelpCatalog registration and delete HelpCatalog.cs
        containerRegistry.RegisterSingleton<IHelpCatalog, HelpCatalog>();
        containerRegistry.RegisterSingleton<IHelpContentLoader, HelpContentLoader>();

        // CROSS-MODULE EDIT: 2025-01-15 Option A Integration Step 1
        // Modified for: Register BackstageService to handle backstage close requests
        // Related modules: Shell (BackstageService, OpenBackstageViewModel, MainWindow)
        // Rollback instructions: Remove BackstageService registration line below
        containerRegistry.RegisterSingleton<IBackstageService, BackstageService>();

        // Register shared log entries collection
        containerRegistry.RegisterInstance(_logEntries);
        
        // Register ViewModels
        containerRegistry.Register<MainWindowViewModel>();
        containerRegistry.Register<LogViewerViewModel>();
        containerRegistry.Register<FileCommandsViewModel>();
        containerRegistry.Register<HelpDialogViewModel>();
        containerRegistry.Register<AboutDialogViewModel>();
        
        // CROSS-MODULE EDIT: 2025-11-14 Open Backstage Step 1
        // Modified for: Register OpenBackstageViewModel for the new Open backstage UI
        // Related modules: Shell (OpenBackstageViewModel, MainWindow)
        // Rollback instructions: Remove the registration line below
        containerRegistry.Register<EasyAF.Shell.ViewModels.Backstage.OpenBackstageViewModel>();

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

