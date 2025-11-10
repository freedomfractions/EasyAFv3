using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EasyAF.Core.Contracts;
using Serilog;

namespace EasyAF.Shell.Services;

/// <summary>
/// Aggregates help pages from all loaded modules implementing <see cref="IHelpProvider"/>.
/// </summary>
public interface IHelpCatalog
{
    /// <summary>
    /// All registered help pages.
    /// </summary>
    ReadOnlyObservableCollection<HelpPageDescriptor> Pages { get; }

    /// <summary>
    /// Adds help pages from a module if it implements <see cref="IHelpProvider"/>.
    /// </summary>
    /// <param name="module">The loaded module instance.</param>
    void RegisterModule(EasyAF.Core.Contracts.IModule module);

    /// <summary>
    /// Searches pages by keyword or title substring.
    /// </summary>
    IEnumerable<HelpPageDescriptor> Search(string query);
}

public class HelpCatalog : IHelpCatalog
{
    private readonly ObservableCollection<HelpPageDescriptor> _pages = new();
    public ReadOnlyObservableCollection<HelpPageDescriptor> Pages { get; }

    public HelpCatalog()
    {
        Pages = new ReadOnlyObservableCollection<HelpPageDescriptor>(_pages);
    }

    public void RegisterModule(EasyAF.Core.Contracts.IModule module)
    {
        if (module is IHelpProvider provider)
        {
            foreach (var page in provider.GetHelpPages() ?? Enumerable.Empty<HelpPageDescriptor>())
            {
                if (page != null && !_pages.Any(p => p.Id == page.Id))
                {
                    _pages.Add(page);
                    Log.Debug("Registered help page {Id} from module {Module}", page.Id, module.ModuleName);
                }
            }
        }
    }

    public IEnumerable<HelpPageDescriptor> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return Pages;
        query = query.Trim();
        return Pages.Where(p =>
            p.Title.Contains(query, System.StringComparison.OrdinalIgnoreCase) ||
            p.Category.Contains(query, System.StringComparison.OrdinalIgnoreCase) ||
            (p.Keywords != null && p.Keywords.Any(k => k.Contains(query, System.StringComparison.OrdinalIgnoreCase))));
    }
}
