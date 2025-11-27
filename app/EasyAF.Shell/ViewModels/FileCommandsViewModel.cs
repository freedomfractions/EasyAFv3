using Prism.Commands;
using Prism.Mvvm;
using EasyAF.Core.Contracts;
using Serilog;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.Linq;
using System.IO;
using System.Diagnostics;
using EasyAF.Shell.Services; // For IBackstageService

namespace EasyAF.Shell.ViewModels;

/// <summary>
/// Provides New/Open/Save/SaveAs commands for the shell, coordinating with DocumentManager and modules.
/// </summary>
/// <remarks>
/// <para>
/// This ViewModel serves as the primary coordinator for file operations in the EasyAF Shell.
/// It bridges the gap between user actions (ribbon buttons, backstage interactions) and the
/// underlying document management system.
/// </para>
/// <para>
/// Key responsibilities:
/// - Building dynamic file type filters from loaded modules
/// - Managing recent files list via IRecentFilesService
/// - Remembering last used directory for file dialogs
/// - Coordinating with IDocumentManager for document lifecycle
/// </para>
/// <para>
/// File dialogs use module-provided file type definitions to build filters, ensuring that
/// only compatible file types are shown for each module.
/// </para>
/// </remarks>
public class FileCommandsViewModel : BindableBase
{
    private readonly IDocumentManager _documentManager;
    private readonly EasyAF.Core.Contracts.IModuleCatalog _moduleCatalog;
    private readonly IRecentFilesService _recentFiles;
    private readonly ISettingsService _settingsService;
    private readonly IBackstageService _backstageService;

    private const string LastDirectorySettingKey = "FileDialogs.LastDirectory";

    // CROSS-MODULE EDIT: 2025-01-11T17:30:00-06:00 Task 10
    // Modified for: Implement Open/Save/SaveAs dialog logic and dynamic module file type associations
    // Related modules: Core (IModule, IDocumentModule, IModuleCatalog, ISettingsService, IRecentFilesService), Shell (MainWindow bindings)
    // Rollback instructions: Remove dialog logic methods (ExecuteOpen/ExecuteSave/ExecuteSaveAs modifications), remove settings usage & related private members
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FileCommandsViewModel"/> class.
    /// </summary>
    /// <param name="documentManager">Document lifecycle manager.</param>
    /// <param name="moduleCatalog">Catalog of loaded modules for file type discovery.</param>
    /// <param name="recentFilesService">Service for tracking recent files.</param>
    /// <param name="settingsService">Service for persisting user preferences.</param>
    /// <param name="moduleLoader">Module loader to subscribe to module loading events.</param>
    /// <param name="backstageService">Service for backstage control.</param>
    public FileCommandsViewModel(IDocumentManager documentManager, EasyAF.Core.Contracts.IModuleCatalog moduleCatalog, IRecentFilesService recentFilesService, ISettingsService settingsService, IModuleLoader moduleLoader, IBackstageService backstageService)
    {
        _documentManager = documentManager;
        _moduleCatalog = moduleCatalog;
        _recentFiles = recentFilesService;
        _settingsService = settingsService;
        _backstageService = backstageService;

        NewCommand = new DelegateCommand(ExecuteNew, CanExecuteNew).ObservesProperty(() => SelectedModule);
        OpenCommand = new DelegateCommand(ExecuteOpen);
        SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave);
        SaveAsCommand = new DelegateCommand(ExecuteSaveAs, CanExecuteSave);
        OpenRecentCommand = new DelegateCommand<string?>(ExecuteOpenRecent, path => !string.IsNullOrWhiteSpace(path));

        // CROSS-MODULE EDIT: 2025-01-16 Save Command Reactive Fix
        // Modified for: Subscribe to ActiveDocumentChanged to update Save command state
        // Related modules: Core (IDocumentManager), Shell (FileCommandsViewModel)
        // Rollback instructions: Remove subscription and restore ObservesProperty approach
        _documentManager.ActiveDocumentChanged += (_, __) =>
        {
            SaveCommand.RaiseCanExecuteChanged();
            SaveAsCommand.RaiseCanExecuteChanged();
        };

        // CROSS-MODULE EDIT: 2025-11-14 Backstage Open UX
        // Modified for: Add context actions for recent files (remove entry, open containing folder, clear list)
        // Related modules: Core (IRecentFilesService), Shell (MainWindow Open backstage)
        // Rollback instructions: Remove RemoveRecentCommand/OpenRecentLocationCommand/ClearRecentCommand and corresponding XAML bindings
        RemoveRecentCommand = new DelegateCommand<string?>(ExecuteRemoveRecent, path => !string.IsNullOrWhiteSpace(path));
        OpenRecentLocationCommand = new DelegateCommand<string?>(ExecuteOpenRecentLocation, path => !string.IsNullOrWhiteSpace(path));
        ClearRecentCommand = new DelegateCommand(ExecuteClearRecent, CanExecuteClearRecent);

        // Requery Clear when the list changes
        _recentFiles.RecentFiles.CollectionChanged += (_, __) => ClearRecentCommand.RaiseCanExecuteChanged();

        // CROSS-MODULE EDIT: 2025-01-15 Map Module New Command Fix
        // Modified for: Subscribe to module loaded events to update SelectedModule when modules are discovered
        // Related modules: Core (IModuleLoader, IModuleCatalog), Map (MapModule)
        // Rollback instructions: Remove moduleLoader parameter and ModuleLoaded subscription below
        
        // Subscribe to module loading to update available modules
        moduleLoader.ModuleLoaded += (_, __) =>
        {
            // When a new module is loaded, notify that AvailableDocumentModules changed
            RaisePropertyChanged(nameof(AvailableDocumentModules));
            
            // If no module is selected yet, try to select the configured default
            if (SelectedModule == null)
            {
                SelectedModule = GetDefaultDocumentModule() ?? AvailableDocumentModules.FirstOrDefault();
            }
        };

        // CROSS-MODULE EDIT: 2025-01-20 Task 20 - Project Module Default
        // Modified for: Set Project Editor as default document type via internal config setting
        // Related modules: Project (ProjectModule)
        // Rollback instructions: Remove GetDefaultDocumentModule call, use first available module
        
        // Select default module (delay until modules are loaded to avoid warning)
        // GetDefaultDocumentModule() will be called after first module loads
        _selectedModule = null; // Will be set by ModuleLoaded event
    }

    /// <summary>
    /// Gets the list of document modules that can create new documents.
    /// </summary>
    /// <remarks>
    /// This list is dynamically populated from the ModuleCatalog and updates automatically
    /// when modules are loaded or unloaded.
    /// </remarks>
    public IEnumerable<IDocumentModule> AvailableDocumentModules => _moduleCatalog.DocumentModules;

    private IDocumentModule? _selectedModule;
    
    /// <summary>
    /// Gets or sets the currently selected module for New document operations.
    /// </summary>
    /// <remarks>
    /// This property is typically bound to a ComboBox in the Backstage "New" tab,
    /// allowing users to choose which type of document to create.
    /// </remarks>
    public IDocumentModule? SelectedModule
    {
        get => _selectedModule;
        set => SetProperty(ref _selectedModule, value);
    }

    /// <summary>
    /// Gets the collection of recent file paths.
    /// </summary>
    /// <remarks>
    /// This collection is bound to UI elements in the Backstage "Open" tab and welcome screen.
    /// It automatically updates when files are opened or removed.
    /// </remarks>
    public ObservableCollection<string> RecentFiles => _recentFiles.RecentFiles;

    // Commands
    
    /// <summary>
    /// Gets the command to create a new document of the selected module type.
    /// </summary>
    public DelegateCommand NewCommand { get; }
    
    /// <summary>
    /// Gets the command to open a document from disk.
    /// </summary>
    public DelegateCommand OpenCommand { get; }
    
    /// <summary>
    /// Gets the command to save the active document.
    /// </summary>
    public DelegateCommand SaveCommand { get; }
    
    /// <summary>
    /// Gets the command to save the active document with a new filename.
    /// </summary>
    public DelegateCommand SaveAsCommand { get; }
    
    /// <summary>
    /// Gets the command to open a document from the recent files list.
    /// </summary>
    public DelegateCommand<string?> OpenRecentCommand { get; }

    // Backstage context menu commands
    
    /// <summary>
    /// Gets the command to remove a file from the recent files list.
    /// </summary>
    public DelegateCommand<string?> RemoveRecentCommand { get; }
    
    /// <summary>
    /// Gets the command to open the containing folder of a recent file in Windows Explorer.
    /// </summary>
    public DelegateCommand<string?> OpenRecentLocationCommand { get; }
    
    /// <summary>
    /// Gets the command to clear all recent files.
    /// </summary>
    public DelegateCommand ClearRecentCommand { get; }

    /// <summary>
    /// Determines whether the New command can execute.
    /// </summary>
    /// <returns>True if a module is selected; otherwise false.</returns>
    private bool CanExecuteNew() => SelectedModule != null;

    /// <summary>
    /// Executes the New command, creating a new document of the selected module type.
    /// </summary>
    private void ExecuteNew()
    {
        if (SelectedModule == null) return;
        
        // CROSS-MODULE EDIT: 2025-01-16 New Document Backstage Close
        // Modified for: Close backstage after creating new document
        // Related modules: Shell (BackstageService), Core (IDocumentManager)
        // Rollback instructions: Remove backstageService parameter and RequestClose call
        
        var doc = _documentManager.CreateNewDocument(SelectedModule);
        Log.Information("New document created: {Title}", doc.Title);
        
        // Close backstage after creating document
        _backstageService.RequestClose();
    }

    /// <summary>
    /// Determines whether Save/SaveAs commands can execute.
    /// </summary>
    /// <returns>True if there is an active document; otherwise false.</returns>
    private bool CanExecuteSave() => _documentManager.ActiveDocument != null;

    /// <summary>
    /// Executes the Save command.
    /// </summary>
    /// <remarks>
    /// If the document has never been saved (no FilePath), this delegates to SaveAs.
    /// Otherwise, it saves to the existing file path.
    /// </remarks>
    private void ExecuteSave()
    {
        var doc = _documentManager.ActiveDocument;
        if (doc == null) return;

        // If document has no path, treat as Save As
        if (string.IsNullOrWhiteSpace(doc.FilePath))
        {
            ExecuteSaveAs();
            return;
        }

        if (!_documentManager.SaveDocument(doc))
        {
            Log.Warning("Save failed for document {Title}", doc.Title);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(doc.FilePath))
                _recentFiles.AddRecentFile(doc.FilePath);
        }
    }

    /// <summary>
    /// Executes the Open command, showing an OpenFileDialog.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The file dialog filter is built dynamically from all loaded modules' supported file types.
    /// The initial directory is the last directory used in any file dialog, or MyDocuments.
    /// </para>
    /// <para>
    /// On success, the file is opened via DocumentManager and added to recent files.
    /// </para>
    /// </remarks>
    private void ExecuteOpen()
    {
        try
        {
            var dialog = new OpenFileDialog
            {
                Title = "Open Document",
                Filter = BuildOpenFilter(),
                Multiselect = false,
                InitialDirectory = GetInitialDirectory()
            };

            var result = dialog.ShowDialog();
            if (result != true) return;

            var path = dialog.FileName;
            var doc = _documentManager.OpenDocument(path);
            _recentFiles.AddRecentFile(path);
            RememberDirectory(path);
            Log.Information("Opened document via dialog: {Path}", path);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Open dialog failed");
        }
    }

    /// <summary>
    /// Executes the OpenRecent command, opening a file from the recent files list.
    /// </summary>
    /// <param name="path">Full path to the file to open.</param>
    private void ExecuteOpenRecent(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        try
        {
            var doc = _documentManager.OpenDocument(path);
            _recentFiles.AddRecentFile(path);
            RememberDirectory(path);
            Log.Information("Opened recent file: {Path}", path);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open recent file: {Path}", path);
        }
    }

    /// <summary>
    /// Executes the SaveAs command, showing a SaveFileDialog.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The file dialog filter is built from the active document's owning module's file types.
    /// The suggested filename is derived from the document's Title property.
    /// </para>
    /// <para>
    /// On success, the document is saved to the new location and added to recent files.
    /// </para>
    /// </remarks>
    private void ExecuteSaveAs()
    {
        var doc = _documentManager.ActiveDocument;
        if (doc == null) return;
        try
        {
            var dialog = new SaveFileDialog
            {
                Title = "Save Document As",
                Filter = BuildSaveFilter(doc.OwnerModule),
                AddExtension = true,
                OverwritePrompt = true,
                InitialDirectory = GetInitialDirectory(),
                FileName = SuggestFileName(doc),
                // CROSS-MODULE EDIT: 2025-01-11 Task 10
                // Modified for: Set default extension from module's primary file type
                // Related modules: Core (IModule, FileTypeDefinition)
                // Rollback instructions: Remove DefaultExt assignment below
                DefaultExt = GetDefaultExtension(doc.OwnerModule)
            };
            var result = dialog.ShowDialog();
            if (result != true) return;

            var path = dialog.FileName;
            if (!_documentManager.SaveDocument(doc, path))
            {
                Log.Warning("SaveAs failed for document {Title}", doc.Title);
                return;
            }

            _recentFiles.AddRecentFile(path);
            RememberDirectory(path);
            Log.Information("Document saved as: {Path}", path);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "SaveAs dialog failed for document {Title}", doc.Title);
        }
    }

    /// <summary>
    /// Removes a file from the recent files list.
    /// </summary>
    /// <param name="path">Full path of the file to remove.</param>
    private void ExecuteRemoveRecent(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        _recentFiles.RemoveRecentFile(path!);
    }

    /// <summary>
    /// Opens Windows Explorer to the containing folder of a recent file.
    /// </summary>
    /// <param name="path">Full path of the file.</param>
    /// <remarks>
    /// If the file exists, Explorer will open with the file selected.
    /// If the file doesn't exist but the directory does, Explorer opens to the directory.
    /// </remarks>
    private void ExecuteOpenRecentLocation(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        try
        {
            var full = Path.GetFullPath(path);
            if (File.Exists(full))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{full}\"",
                    UseShellExecute = true
                });
            }
            else
            {
                var dir = Path.GetDirectoryName(full);
                if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
                {
                    Process.Start(new ProcessStartInfo { FileName = dir, UseShellExecute = true });
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open recent file location: {Path}", path);
        }
    }

    /// <summary>
    /// Determines whether the ClearRecent command can execute.
    /// </summary>
    /// <returns>True if there are recent files; otherwise false.</returns>
    private bool CanExecuteClearRecent() => _recentFiles.RecentFiles.Count > 0;

    /// <summary>
    /// Clears all recent files.
    /// </summary>
    private void ExecuteClearRecent()
    {
        // Prefer service to handle persistence
        if (_recentFiles is { } svc)
        {
            // If service implements Clear, call it; otherwise clear and persist via remove loop
            try
            {
                // dynamic cast in case of interface not updated, but in our code we add Clear to interface
                svc.Clear();
            }
            catch
            {
                foreach (var path in _recentFiles.RecentFiles.ToList())
                {
                    _recentFiles.RemoveRecentFile(path);
                }
            }
        }
        ClearRecentCommand.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Suggests a filename based on the document's Title, replacing invalid characters.
    /// </summary>
    /// <param name="doc">The document to suggest a name for.</param>
    /// <returns>A valid filename (without extension).</returns>
    private string SuggestFileName(IDocument doc)
    {
        // CROSS-MODULE EDIT: 2025-01-20 Default Filename Convention
        // Modified for: Use module's suggested filename if available (e.g., "[LB Project Number] - [Site Name]")
        // Related modules: Core (IDocumentModule.GetSuggestedFileName), Project (ProjectModule impl)
        // Rollback instructions: Remove module.GetSuggestedFileName call, restore Title-only logic
        
        // Try to get module-specific suggested filename
        if (doc.OwnerModule is IDocumentModule module)
        {
            var suggested = module.GetSuggestedFileName(doc);
            if (!string.IsNullOrWhiteSpace(suggested))
            {
                // Module provided a valid suggestion, sanitize and return
                foreach (var c in Path.GetInvalidFileNameChars())
                {
                    suggested = suggested.Replace(c, '_');
                }
                return suggested;
            }
        }

        // Fallback to Title-based naming
        var baseName = doc.Title;
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            baseName = baseName.Replace(c, '_');
        }
        return baseName;
    }

    /// <summary>
    /// Builds an OpenFileDialog filter string from all loaded modules' file types.
    /// </summary>
    /// <returns>Filter string in format: "Description (*.ext)|*.ext|..."</returns>
    /// <remarks>
    /// <para>
    /// The filter includes:
    /// - An "All Supported Files" entry combining all extensions
    /// - Individual entries for each module's file types
    /// - An "All Files (*.*)" entry
    /// </para>
    /// </remarks>
    private string BuildOpenFilter()
    {
        // Build per-module filters + global combined filter
        var moduleFilters = new List<string>();
        var allExtensions = new List<string>();
        foreach (var mod in _moduleCatalog.DocumentModules)
        {
            var types = mod.SupportedFileTypes;
            if (types != null && types.Count > 0)
            {
                foreach (var t in types)
                {
                    var pattern = $"*.{t.Extension}";
                    moduleFilters.Add($"{t.Description} ({pattern})|{pattern}");
                    allExtensions.Add(pattern);
                }
            }
            else
            {
                foreach (var ext in mod.SupportedFileExtensions)
                {
                    var pattern = $"*.{ext}";
                    moduleFilters.Add($"{mod.ModuleName} ({pattern})|{pattern}");
                    allExtensions.Add(pattern);
                }
            }
        }
        if (allExtensions.Count > 0)
        {
            moduleFilters.Insert(0, $"All Supported ({string.Join(';', allExtensions)})|{string.Join(';', allExtensions)}");
        }
        moduleFilters.Add("All Files (*.*)|*.*");
        return string.Join("|", moduleFilters);
    }

    /// <summary>
    /// Builds a SaveFileDialog filter string for a specific module's file types.
    /// </summary>
    /// <param name="ownerModule">The module that owns the document being saved.</param>
    /// <returns>Filter string with the module's supported file types.</returns>
    private string BuildSaveFilter(IDocumentModule ownerModule)
    {
        var filters = new List<string>();
        var types = ownerModule.SupportedFileTypes;
        if (types != null && types.Count > 0)
        {
            foreach (var t in types)
            {
                var pattern = $"*.{t.Extension}";
                filters.Add($"{t.Description} ({pattern})|{pattern}");
            }
        }
        else
        {
            foreach (var ext in ownerModule.SupportedFileExtensions)
            {
                var pattern = $"*.{ext}";
                filters.Add($"{ownerModule.ModuleName} ({pattern})|{pattern}");
            }
        }
        return string.Join("|", filters);
    }

    /// <summary>
    /// Gets the initial directory for file dialogs.
    /// </summary>
    /// <returns>
    /// Module-specific subfolder in Documents\EasyAF\, falling back to last used directory if in EasyAF hierarchy.
    /// </returns>
    /// <remarks>
    /// CROSS-MODULE EDIT: 2025-01-27 Professional File Organization
    /// Modified for: Use structured EasyAF subfolders instead of flat Documents folder
    /// Related modules: All document modules (Map, Project, Spec)
    /// Rollback instructions: Restore original MyDocuments fallback
    /// 
    /// Directory structure for one-click installer deployment:
    /// - %USERPROFILE%\Documents\EasyAF\Maps\     - Map files (.ezmap)
    /// - %USERPROFILE%\Documents\EasyAF\Projects\ - Project files (.ezproj)
    /// - %USERPROFILE%\Documents\EasyAF\Specs\    - Spec files (.ezspec)
    /// - %USERPROFILE%\Documents\EasyAF\Templates\ - Template files
    /// 
    /// This provides clean organization and allows backup/sync of entire EasyAF folder.
    /// 
    /// PRIORITY LOGIC:
    /// 1. Module-specific directory (Maps/, Projects/, Specs/) - PREFERRED
    /// 2. Last used directory IF it's within EasyAF hierarchy - fallback for flexibility
    /// 3. EasyAF root - safe fallback
    /// 4. Documents - ultimate fallback
    /// </remarks>
    private string GetInitialDirectory()
    {
        // PRIORITY 1: Get module-specific directory (always preferred for new saves)
        var activeDoc = _documentManager.ActiveDocument;
        if (activeDoc?.OwnerModule != null)
        {
            var moduleSpecificDir = GetModuleDefaultDirectory(activeDoc.OwnerModule);
            if (!string.IsNullOrEmpty(moduleSpecificDir))
            {
                // Create directory if it doesn't exist
                try
                {
                    if (!Directory.Exists(moduleSpecificDir))
                    {
                        Directory.CreateDirectory(moduleSpecificDir);
                        Log.Information("Created module directory: {Path}", moduleSpecificDir);
                    }
                    Log.Debug("Using module-specific directory: {Path} for {Module}", 
                        moduleSpecificDir, activeDoc.OwnerModule.ModuleName);
                    return moduleSpecificDir;
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to create module directory: {Path}", moduleSpecificDir);
                    // Fall through to other options
                }
            }
        }
        
        // PRIORITY 2: Check for last used directory (only if within EasyAF hierarchy)
        var last = _settingsService.GetSetting(LastDirectorySettingKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(last) && Directory.Exists(last))
        {
            // Only use last directory if it's within the EasyAF folder structure
            var easyAFPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "EasyAF");
            
            if (last.StartsWith(easyAFPath, StringComparison.OrdinalIgnoreCase))
            {
                Log.Debug("Using last directory (within EasyAF): {Path}", last);
                return last;
            }
            else
            {
                Log.Debug("Ignoring last directory (outside EasyAF): {Path}", last);
            }
        }
        
        // PRIORITY 3: Final fallback - Documents\EasyAF\ folder
        try
        {
            var easyAFRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "EasyAF");
            
            if (!Directory.Exists(easyAFRoot))
            {
                Directory.CreateDirectory(easyAFRoot);
                Log.Information("Created EasyAF root directory: {Path}", easyAFRoot);
            }
            
            Log.Debug("Using EasyAF root directory: {Path}", easyAFRoot);
            return easyAFRoot;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create EasyAF root directory, falling back to Documents");
            // Ultimate fallback: just Documents
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
    }

    /// <summary>
    /// Gets the default directory for a specific module type.
    /// </summary>
    /// <param name="module">The document module.</param>
    /// <returns>Module-specific subfolder path, or null if module type unknown.</returns>
    private string? GetModuleDefaultDirectory(IDocumentModule module)
    {
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var basePath = Path.Combine(documentsPath, "EasyAF");
        
        // Map module names to subdirectories
        return module.ModuleName switch
        {
            "Map Editor" => Path.Combine(basePath, "Maps"),
            "Project Editor" => Path.Combine(basePath, "Projects"),
            "Spec Editor" => Path.Combine(basePath, "Specs"),
            _ => basePath // Unknown module type - use root EasyAF folder
        };
    }
    
    /// <summary>
    /// Remembers the directory of a file path for future file dialogs.
    /// </summary>
    /// <param name="path">Full file path.</param>
    private void RememberDirectory(string path)
    {
        try
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                _settingsService.SetSetting(LastDirectorySettingKey, dir);
            }
        }
        catch { /* swallow */ }
    }

    /// <summary>
    /// Gets the default file extension for a module.
    /// </summary>
    /// <param name="module">The module to get the extension for.</param>
    /// <returns>The primary file extension (without the dot), or "dat" if none defined.</returns>
    private string GetDefaultExtension(IDocumentModule module)
    {
        // Get primary extension from module's supported file types
        var types = module.SupportedFileTypes;
        if (types != null && types.Count > 0)
            return types[0].Extension;

        // Fallback to first supported extension
        return module.SupportedFileExtensions.FirstOrDefault() ?? "dat";
    }

    /// <summary>
    /// Gets the default document module to use for New command.
    /// </summary>
    /// <returns>The configured default module, or null if not configured.</returns>
    /// <remarks>
    /// <para>
    /// This is an internal configuration setting not exposed in the Options UI.
    /// It allows changing the default document type (e.g., from Map to Project)
    /// without user intervention.
    /// </para>
    /// <para>
    /// <strong>Configuration:</strong>
    /// Setting key: "FileCommands.DefaultDocumentModule"
    /// Value: Module name (e.g., "Project Editor", "Map Editor")
    /// Default: null (uses first available module)
    /// </para>
    /// <para>
    /// <strong>Usage:</strong>
    /// Set via appsettings.json or programmatically during startup.
    /// </para>
    /// </remarks>
    private IDocumentModule? GetDefaultDocumentModule()
    {
        try
        {
            // Get configured default module name from settings
            var defaultModuleName = _settingsService.GetSetting("FileCommands.DefaultDocumentModule", string.Empty);
            
            if (string.IsNullOrWhiteSpace(defaultModuleName))
            {
                Log.Debug("No default document module configured, will use first available");
                return null;
            }

            // Find module by name
            var defaultModule = AvailableDocumentModules.FirstOrDefault(m => 
                string.Equals(m.ModuleName, defaultModuleName, StringComparison.OrdinalIgnoreCase));

            if (defaultModule != null)
            {
                Log.Information("Default document module set to: {ModuleName}", defaultModuleName);
                return defaultModule;
            }

            Log.Debug("Configured default module '{ModuleName}' not found yet, will retry when modules load", defaultModuleName);
            return null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get default document module from settings");
            return null;
        }
    }
}
