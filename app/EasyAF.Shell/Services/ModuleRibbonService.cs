using System.Collections.ObjectModel;
using EasyAF.Core.Contracts;
using Fluent;

namespace EasyAF.Shell.Services;

/// <summary>
/// Manages Ribbon tabs contributed by modules.
/// </summary>
public interface IModuleRibbonService
{
    ObservableCollection<RibbonTabItem> Tabs { get; }
    void AddModuleTabs(EasyAF.Core.Contracts.IModule module, IDocument? activeDocument = null);
}

public class ModuleRibbonService : IModuleRibbonService
{
    public ObservableCollection<RibbonTabItem> Tabs { get; } = new();

    public void AddModuleTabs(EasyAF.Core.Contracts.IModule module, IDocument? activeDocument = null)
    {
        if (module is IDocumentModule docModule)
        {
            var tabs = docModule.GetRibbonTabs(activeDocument);
            if (tabs != null)
            {
                foreach (var tab in tabs)
                {
                    if (tab != null) Tabs.Add(tab);
                }
            }
        }
    }
}
