using Prism.Ioc;
using Prism.Mvvm;
using System.Windows.Input;
using Prism.Commands;
using Serilog;
using EasyAF.Core.Contracts;
using System.Text;

namespace EasyAF.Shell.ViewModels;

/// <summary>
/// ViewModel for About dialog.
/// </summary>
public class AboutDialogViewModel : BindableBase
{
    private readonly EasyAF.Core.Contracts.IModuleCatalog _catalog;
    public AboutDialogViewModel(EasyAF.Core.Contracts.IModuleCatalog catalog)
    {
        _catalog = catalog;
        CloseCommand = new DelegateCommand(() => DialogResult = true);
        BuildInfo = BuildAboutText();
    }

    public string BuildInfo { get; }
    public ICommand CloseCommand { get; }
    public bool? DialogResult { get; set; }

    private string BuildAboutText()
    {
        var sb = new StringBuilder();
        sb.AppendLine("EasyAF v3");
        sb.AppendLine($".NET {Environment.Version}");
        sb.AppendLine("Loaded Modules:");
        foreach (var m in _catalog.Modules)
        {
            sb.AppendLine($" - {m.ModuleName} v{m.ModuleVersion}");
        }
        return sb.ToString();
    }
}
