using Prism.Commands;
using Prism.Mvvm;
using EasyAF.Core.Contracts;
using Serilog;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace EasyAF.Shell.ViewModels;

/// <summary>
/// Provides New/Open/Save/SaveAs commands for the shell, coordinating with DocumentManager and modules.
/// </summary>
public class FileCommandsViewModel : BindableBase
{
    private readonly IDocumentManager _documentManager;
    private readonly EasyAF.Core.Contracts.IModuleCatalog _moduleCatalog;
    private readonly IRecentFilesService _recentFiles;
    private readonly ISettingsService _settingsService;

    private const string LastDirectorySettingKey = "FileDialogs.LastDirectory";

    // CROSS-MODULE EDIT: 2025-01-11T17:30:00-06:00 Task 10
    // Modified for: Implement Open/Save/SaveAs dialog logic and dynamic module file type associations
    // Related modules: Core (IModule, IDocumentModule, IModuleCatalog, ISettingsService, IRecentFilesService), Shell (MainWindow bindings)
    // Rollback instructions: Remove dialog logic methods (ExecuteOpen/ExecuteSave/ExecuteSaveAs modifications), remove settings usage & related private members
    public FileCommandsViewModel(IDocumentManager documentManager, EasyAF.Core.Contracts.IModuleCatalog moduleCatalog, IRecentFilesService recentFilesService, ISettingsService settingsService)
    {
        _documentManager = documentManager;
        _moduleCatalog = moduleCatalog;
        _recentFiles = recentFilesService;
        _settingsService = settingsService;

        NewCommand = new DelegateCommand(ExecuteNew, CanExecuteNew).ObservesProperty(() => SelectedModule);
        OpenCommand = new DelegateCommand(ExecuteOpen);
        SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => _documentManager.ActiveDocument);
        SaveAsCommand = new DelegateCommand(ExecuteSaveAs, CanExecuteSave).ObservesProperty(() => _documentManager.ActiveDocument);
        OpenRecentCommand = new DelegateCommand<string?>(ExecuteOpenRecent, path => !string.IsNullOrWhiteSpace(path));

        // CROSS-MODULE EDIT: 2025-11-14 Backstage Open UX
        // Modified for: Add context actions for recent files (remove entry, open containing folder, clear list)
        // Related modules: Core (IRecentFilesService), Shell (MainWindow Open backstage)
        // Rollback instructions: Remove RemoveRecentCommand/OpenRecentLocationCommand/ClearRecentCommand and corresponding XAML bindings
        RemoveRecentCommand = new DelegateCommand<string?>(ExecuteRemoveRecent, path => !string.IsNullOrWhiteSpace(path));
        OpenRecentLocationCommand = new DelegateCommand<string?>(ExecuteOpenRecentLocation, path => !string.IsNullOrWhiteSpace(path));
        ClearRecentCommand = new DelegateCommand(ExecuteClearRecent, CanExecuteClearRecent);

        // Requery Clear when the list changes
        _recentFiles.RecentFiles.CollectionChanged += (_, __) => ClearRecentCommand.RaiseCanExecuteChanged();

        // Default selected module (first available) for convenience
        _selectedModule = AvailableDocumentModules.FirstOrDefault();
    }

    public IEnumerable<IDocumentModule> AvailableDocumentModules => _moduleCatalog.DocumentModules;

    private IDocumentModule? _selectedModule;
    public IDocumentModule? SelectedModule
    {
        get => _selectedModule;
        set => SetProperty(ref _selectedModule, value);
    }

    public ObservableCollection<string> RecentFiles => _recentFiles.RecentFiles;

    public DelegateCommand NewCommand { get; }
    public DelegateCommand OpenCommand { get; }
    public DelegateCommand SaveCommand { get; }
    public DelegateCommand SaveAsCommand { get; }
    public DelegateCommand<string?> OpenRecentCommand { get; }

    // New backstage commands
    public DelegateCommand<string?> RemoveRecentCommand { get; }
    public DelegateCommand<string?> OpenRecentLocationCommand { get; }
    public DelegateCommand ClearRecentCommand { get; }

    private bool CanExecuteNew() => SelectedModule != null;

    private void ExecuteNew()
    {
        if (SelectedModule == null) return;
        var doc = _documentManager.CreateNewDocument(SelectedModule);
        Log.Information("New document created: {Title}", doc.Title);
    }

    private bool CanExecuteSave() => _documentManager.ActiveDocument != null;

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

    private void ExecuteRemoveRecent(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        _recentFiles.RemoveRecentFile(path!);
    }

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

    private bool CanExecuteClearRecent() => _recentFiles.RecentFiles.Count > 0;

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

    private string SuggestFileName(IDocument doc)
    {
        // Use existing title or filename, ensure no invalid characters
        var baseName = doc.Title;
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            baseName = baseName.Replace(c, '_');
        }
        return baseName;
    }

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

    private string GetInitialDirectory()
    {
        var last = _settingsService.GetSetting(LastDirectorySettingKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(last) && Directory.Exists(last))
            return last;
        return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }

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

    private string GetDefaultExtension(IDocumentModule module)
    {
        // Get primary extension from module's supported file types
        var types = module.SupportedFileTypes;
        if (types != null && types.Count > 0)
            return types[0].Extension;

        // Fallback to first supported extension
        return module.SupportedFileExtensions.FirstOrDefault() ?? "dat";
    }
}
