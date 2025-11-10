using System.Collections.ObjectModel;
using Prism.Mvvm;
using EasyAF.Core.Contracts;
using System.Windows.Input;
using Prism.Commands;
using Serilog;
using EasyAF.Shell.Services;

namespace EasyAF.Shell.ViewModels;

/// <summary>
/// ViewModel for the Help dialog.
/// </summary>
public class HelpDialogViewModel : BindableBase
{
    private readonly IHelpCatalog _catalog;
    private readonly IHelpContentLoader _loader;
    private HelpPageDescriptor? _selectedPage;
    private string _content = string.Empty;
    private string _searchQuery = string.Empty;

    public HelpDialogViewModel(IHelpCatalog catalog, IHelpContentLoader loader)
    {
        _catalog = catalog;
        _loader = loader;

        Pages = new ObservableCollection<HelpPageDescriptor>(_catalog.Pages);
        SearchCommand = new DelegateCommand(DoSearch);
        CloseCommand = new DelegateCommand(() => DialogResult = true);
    }

    public ObservableCollection<HelpPageDescriptor> Pages { get; }

    public HelpPageDescriptor? SelectedPage
    {
        get => _selectedPage;
        set
        {
            if (SetProperty(ref _selectedPage, value) && value != null)
            {
                Content = _loader.LoadContent(value);
                Log.Debug("Loaded help content for {Id}", value.Id);
            }
        }
    }

    public string Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set => SetProperty(ref _searchQuery, value);
    }

    public ICommand SearchCommand { get; }
    public ICommand CloseCommand { get; }

    public bool? DialogResult { get; set; }

    private void DoSearch()
    {
        Pages.Clear();
        foreach (var p in _catalog.Search(SearchQuery))
            Pages.Add(p);
    }
}
