using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Threading;
using Prism.Commands;
using Prism.Mvvm;
using EasyAF.Core.Contracts;
using Serilog;

namespace EasyAF.Shell.ViewModels;

/// <summary>
/// Manages the vertical file tab list, grouping files by type with expand/collapse support.
/// </summary>
/// <remarks>
/// <para>
/// This manager orchestrates the file tab UI on the left side of the shell.
/// It automatically groups files by their owning module and maintains the Welcome tab.
/// </para>
/// <para>
/// Features:
/// - Welcome tab (always first, ungrouped)
/// - File groups by module type (collapsible)
/// - Automatic status updates (dirty/saved/clean)
/// - Timestamp refresh (every minute)
/// </para>
/// </remarks>
public class FileTabManagerViewModel : BindableBase, IDisposable
{
    private readonly IDocumentManager _documentManager;
    private readonly DispatcherTimer _timestampUpdateTimer;
    private bool _disposed;
    private object? _selectedTab;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FileTabManagerViewModel"/> class.
    /// </summary>
    /// <param name="documentManager">The document manager tracking open documents.</param>
    public FileTabManagerViewModel(IDocumentManager documentManager)
    {
        _documentManager = documentManager ?? throw new ArgumentNullException(nameof(documentManager));
        
        FileTabItems = new ObservableCollection<object>(); // Holds Welcome tab + groups
        
        // Subscribe to document manager events
        _documentManager.OpenDocuments.CollectionChanged += OnDocumentsCollectionChanged;
        _documentManager.ActiveDocumentChanged += OnActiveDocumentChanged;
        _documentManager.DocumentClosed += OnDocumentClosed;
        
        // Timer to refresh "last saved" timestamps every minute
        _timestampUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1)
        };
        _timestampUpdateTimer.Tick += (_, __) => RefreshTimestamps();
        _timestampUpdateTimer.Start();
        
        // Initialize with Welcome tab
        RebuildFileTabList();
        
        Log.Debug("FileTabManagerViewModel initialized");
    }
    
    /// <summary>
    /// Gets the collection of file tab items (Welcome tab + file groups).
    /// </summary>
    /// <remarks>
    /// This collection contains:
    /// - WelcomeTabViewModel (for Welcome tab)
    /// - FileTabGroupViewModel (for each module type with files)
    /// </remarks>
    public ObservableCollection<object> FileTabItems { get; }
    
    /// <summary>
    /// Gets or sets the selected tab (can be FileTabItemViewModel or WelcomeTabViewModel).
    /// </summary>
    public object? SelectedTab
    {
        get => _selectedTab;
        set
        {
            if (SetProperty(ref _selectedTab, value))
            {
                // Handle FileTabItemViewModel selection
                if (value is FileTabItemViewModel fileTab)
                {
                    if (fileTab.Document != _documentManager.ActiveDocument)
                    {
                        _documentManager.ActiveDocument = fileTab.Document;
                    }
                }
                // Welcome tab is handled by its click behavior (sets ActiveDocument to null)
            }
        }
    }
    
    /// <summary>
    /// Rebuilds the entire file tab list from scratch.
    /// </summary>
    /// <remarks>
    /// Called when documents are opened/closed to reorganize groups.
    /// </remarks>
    private void RebuildFileTabList()
    {
        FileTabItems.Clear();
        
        Log.Debug("RebuildFileTabList called. Document count: {Count}", _documentManager.OpenDocuments.Count);
        
        // Add Welcome tab first (always at top, ungrouped)
        var welcomeTab = new WelcomeTabViewModel(new DelegateCommand(CloseWelcomeTab));
        FileTabItems.Add(welcomeTab);
        Log.Debug("Added Welcome tab");
        
        // Don't add file groups if there are no documents
        if (_documentManager.OpenDocuments.Count == 0)
        {
            Log.Debug("No documents open, only Welcome tab visible");
            return;
        }
        
        // Group documents by module
        var groups = _documentManager.OpenDocuments
            .GroupBy(doc => doc.OwnerModule)
            .OrderBy(g => g.Key.ModuleName)
            .ToList();
        
        Log.Debug("Grouped documents into {GroupCount} groups", groups.Count);
        
        foreach (var group in groups)
        {
            var groupVm = new FileTabGroupViewModel(group.Key);
            
            foreach (var doc in group.OrderBy(d => d.Title))
            {
                var tabItem = new FileTabItemViewModel(doc, new DelegateCommand<IDocument>(CloseDocument));
                
                // Set active state
                if (doc == _documentManager.ActiveDocument)
                {
                    tabItem.IsActive = true;
                    SelectedTab = tabItem;
                    
                    // Deactivate Welcome tab when a document is active
                    welcomeTab.IsActive = false;
                }
                
                groupVm.Items.Add(tabItem);
                Log.Debug("Added tab item: {FileName}", tabItem.FileName);
            }
            
            // Only add group if it has items
            if (groupVm.Items.Count > 0)
            {
                FileTabItems.Add(groupVm);
                Log.Debug("Added group: {GroupName} with {ItemCount} items", groupVm.Header, groupVm.Items.Count);
            }
        }
        
        // If no document is active, activate Welcome tab
        if (_documentManager.ActiveDocument == null)
        {
            welcomeTab.IsActive = true;
        }
        
        Log.Debug("Rebuilt file tab list: {GroupCount} groups, {DocCount} documents", 
            groups.Count, _documentManager.OpenDocuments.Count);
    }
    
    /// <summary>
    /// Closes the Welcome tab.
    /// </summary>
    /// <remarks>
    /// TODO: Save user preference to not show Welcome tab on startup.
    /// </remarks>
    private void CloseWelcomeTab()
    {
        // Find and remove Welcome tab
        var welcomeTab = FileTabItems.OfType<WelcomeTabViewModel>().FirstOrDefault();
        if (welcomeTab != null)
        {
            FileTabItems.Remove(welcomeTab);
            Log.Information("Welcome tab closed (will not show on next startup)");
            
            // TODO: Save preference to settings
            // _settingsService.SetSetting("Shell.ShowWelcomeTab", false);
        }
    }
    
    /// <summary>
    /// Handles documents collection changes (add/remove).
    /// </summary>
    private void OnDocumentsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Rebuild entire list when documents change
        // This is simpler than incremental updates and still performant
        RebuildFileTabList();
    }
    
    /// <summary>
    /// Handles changes to the active document.
    /// </summary>
    private void OnActiveDocumentChanged(object? sender, IDocument? activeDocument)
    {
        // Update IsActive state for all file tabs
        foreach (var item in FileTabItems)
        {
            if (item is FileTabItemViewModel fileTab)
            {
                fileTab.IsActive = fileTab.Document == activeDocument;
                if (fileTab.IsActive)
                {
                    SelectedTab = fileTab;
                }
            }
            else if (item is FileTabGroupViewModel group)
            {
                foreach (var groupTab in group.Items)
                {
                    groupTab.IsActive = groupTab.Document == activeDocument;
                    if (groupTab.IsActive)
                    {
                        SelectedTab = groupTab;
                    }
                }
            }
            else if (item is WelcomeTabViewModel welcomeTab)
            {
                // Welcome tab is active when no document is active
                welcomeTab.IsActive = activeDocument == null;
                if (welcomeTab.IsActive)
                {
                    SelectedTab = welcomeTab;
                }
            }
        }
        
        Log.Debug("Active document changed: {Title}", activeDocument?.Title ?? "None (Welcome)");
    }
    
    /// <summary>
    /// Handles document closed event to trigger save indicator.
    /// </summary>
    private void OnDocumentClosed(object? sender, IDocument document)
    {
        // Find the tab for this document and notify it was saved
        foreach (var item in FileTabItems)
        {
            if (item is FileTabGroupViewModel group)
            {
                var tabItem = group.Items.FirstOrDefault(t => ReferenceEquals(t.Document, document));
                if (tabItem != null && !document.IsDirty)
                {
                    tabItem.NotifySaved();
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// Closes a document via the close button.
    /// </summary>
    private void CloseDocument(IDocument? document)
    {
        if (document == null) return;
        
        // Delegate to document manager (will trigger rebuild via event)
        _documentManager.CloseDocument(document, null);
    }
    
    /// <summary>
    /// Refreshes "last saved" timestamps for all tabs.
    /// </summary>
    private void RefreshTimestamps()
    {
        foreach (var item in FileTabItems)
        {
            if (item is FileTabGroupViewModel group)
            {
                foreach (var tabItem in group.Items)
                {
                    tabItem.RefreshTimestamp();
                }
            }
        }
    }
    
    /// <summary>
    /// Disposes resources and stops timers.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        
        _timestampUpdateTimer.Stop();
        _documentManager.OpenDocuments.CollectionChanged -= OnDocumentsCollectionChanged;
        _documentManager.ActiveDocumentChanged -= OnActiveDocumentChanged;
        _documentManager.DocumentClosed -= OnDocumentClosed;
        
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
