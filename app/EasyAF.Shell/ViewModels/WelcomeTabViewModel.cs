using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

namespace EasyAF.Shell.ViewModels;

/// <summary>
/// View model for the Welcome tab in the vertical file tab strip.
/// </summary>
/// <remarks>
/// <para>
/// The Welcome tab is a special tab that doesn't represent a document.
/// It's always displayed at the top of the file tab list (ungrouped).
/// </para>
/// <para>
/// Features:
/// - Shows/hides based on user preference
/// - Closable (remembers preference)
/// - Styled differently from file tabs
/// - Always appears at the top (index 0)
/// </para>
/// </remarks>
public class WelcomeTabViewModel : BindableBase
{
    private bool _isActive;
    private bool _isHovered;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="WelcomeTabViewModel"/> class.
    /// </summary>
    /// <param name="closeCommand">Command to execute when close button is clicked.</param>
    public WelcomeTabViewModel(ICommand closeCommand)
    {
        CloseCommand = closeCommand;
    }
    
    /// <summary>
    /// Gets the display name for the Welcome tab.
    /// </summary>
    public string DisplayName => "Welcome";
    
    /// <summary>
    /// Gets the description text for the Welcome tab.
    /// </summary>
    public string Description => "Get started with EasyAF";
    
    /// <summary>
    /// Gets or sets whether this tab is the active (selected) tab.
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }
    
    /// <summary>
    /// Gets or sets whether the mouse is hovering over this tab.
    /// </summary>
    public bool IsHovered
    {
        get => _isHovered;
        set
        {
            SetProperty(ref _isHovered, value);
            RaisePropertyChanged(nameof(ShowCloseButton));
        }
    }
    
    /// <summary>
    /// Gets whether the close button should be visible.
    /// </summary>
    /// <remarks>
    /// Always false - Welcome tab is not closable.
    /// </remarks>
    public bool ShowCloseButton => false;
    
    /// <summary>
    /// Gets the command to close this tab.
    /// </summary>
    public ICommand CloseCommand { get; }
    
    /// <summary>
    /// Gets the icon glyph for the Welcome tab.
    /// </summary>
    /// <remarks>
    /// Uses Segoe MDL2 Assets: Home icon (&#xE80F;)
    /// </remarks>
    public string IconGlyph => "\uE80F"; // Home icon
}
