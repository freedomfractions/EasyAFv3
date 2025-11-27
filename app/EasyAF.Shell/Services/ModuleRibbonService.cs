using System.Collections.ObjectModel;
using EasyAF.Core.Contracts;
using Fluent;
using System.Collections.Specialized;

namespace EasyAF.Shell.Services;

/// <summary>
/// Manages Ribbon tabs contributed by modules.
/// </summary>
public interface IModuleRibbonService
{
    ObservableCollection<RibbonTabItem> Tabs { get; }
    void AddModuleTabs(EasyAF.Core.Contracts.IModule module, IDocument? activeDocument = null);
    event EventHandler? TabsChanged;
}

public class ModuleRibbonService : IModuleRibbonService
{
    public ObservableCollection<RibbonTabItem> Tabs { get; } = new();
    public event EventHandler? TabsChanged;

    public ModuleRibbonService()
    {
        Tabs.CollectionChanged += OnTabsCollectionChanged;
    }

    private void OnTabsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        TabsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void AddModuleTabs(EasyAF.Core.Contracts.IModule module, IDocument? activeDocument = null)
    {
        if (module is IDocumentModule docModule)
        {
            var tabs = docModule.GetRibbonTabs(activeDocument);
            if (tabs != null)
            {
                foreach (var tab in tabs)
                {
                    if (tab != null)
                    {
                        // Check if a tab with the same header already exists
                        var existingTab = Tabs.FirstOrDefault(t => 
                            t.Header?.ToString()?.Equals(tab.Header?.ToString(), StringComparison.OrdinalIgnoreCase) == true);
                        
                        if (existingTab != null)
                        {
                            // Update existing tab's DataContext instead of adding duplicate
                            existingTab.DataContext = tab.DataContext;
                        }
                        else
                        {
                            // Add new tab
                            Tabs.Add(tab);
                        }
                    }
                }
            }
        }
    }
}
