using Fluent;
using EasyAF.Shell.Services;

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
}