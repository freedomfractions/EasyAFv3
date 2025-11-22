using System.Collections.ObjectModel;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using EasyAF.Core.Contracts;

namespace EasyAF.Shell.ViewModels;

/// <summary>
/// View model for a group of file tabs of the same type.
/// </summary>
/// <remarks>
/// <para>
/// Groups files by their owning module (e.g., "Map Files", "Project Files").
/// Supports expand/collapse with triangle indicator.
/// </para>
/// </remarks>
public class FileTabGroupViewModel : BindableBase
{
    private bool _isExpanded = true;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FileTabGroupViewModel"/> class.
    /// </summary>
    /// <param name="module">The module that owns this group of files.</param>
    public FileTabGroupViewModel(IDocumentModule module)
    {
        Module = module;
        Items = new ObservableCollection<FileTabItemViewModel>();
        
        // Command to toggle expand/collapse
        ToggleExpandCommand = new DelegateCommand(() => IsExpanded = !IsExpanded);
    }
    
    /// <summary>
    /// Gets the module that owns this group.
    /// </summary>
    public IDocumentModule Module { get; }
    
    /// <summary>
    /// Gets the group header text (module name).
    /// </summary>
    public string Header => Module.ModuleName;
    
    /// <summary>
    /// Gets the collection of file tab items in this group.
    /// </summary>
    public ObservableCollection<FileTabItemViewModel> Items { get; }
    
    /// <summary>
    /// Gets or sets whether the group is expanded.
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (SetProperty(ref _isExpanded, value))
            {
                RaisePropertyChanged(nameof(ExpanderIcon));
            }
        }
    }
    
    /// <summary>
    /// Gets the triangle icon for expand/collapse.
    /// </summary>
    /// <remarks>
    /// Uses Segoe MDL2 Assets glyphs:
    /// - ChevronRight (&#xE76C;) when collapsed
    /// - ChevronDown (&#xE70D;) when expanded
    /// </remarks>
    public string ExpanderIcon => IsExpanded ? "\uE70D" : "\uE76C"; // ChevronDown : ChevronRight
    
    /// <summary>
    /// Gets the command to toggle expand/collapse.
    /// </summary>
    public ICommand ToggleExpandCommand { get; }
}
