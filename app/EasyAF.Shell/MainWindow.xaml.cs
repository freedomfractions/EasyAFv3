using System.Windows.Input;
using Fluent;
using EasyAF.Shell.Services;
using EasyAF.Shell.ViewModels;

namespace EasyAF.Shell;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : RibbonWindow
{
    public MainWindow(IBackstageService backstageService)
    {
        InitializeComponent();
        
        // Wire up backstage close request handling
        backstageService.CloseRequested += (sender, e) =>
        {
            // Close the backstage menu (Fluent.Ribbon uses Backstage control)
            if (MainRibbon?.Menu is Backstage backstage)
            {
                backstage.IsOpen = false;
            }
        };
    }
    
    /// <summary>
    /// Handles double-click on module list to create document.
    /// </summary>
    private void ModuleList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        // Execute New command if a module is selected
        if (DataContext is MainWindowViewModel vm && vm.FileCommands.SelectedModule != null)
        {
            vm.FileCommands.NewCommand.Execute();
        }
    }
}