using System;
using System.IO;
using System.Linq;
using EasyAF.Shell.Models.Backstage;

namespace EasyAF.Shell.ViewModels.Backstage;

public partial class OpenBackstageViewModel
{
    private void ExecuteAddToQuickAccess(object? parameter)
    {
        string? folderPath = null;
        int insertionIndex = -1;
        if (parameter is string s) folderPath = s;
        else if (parameter is ValueTuple<string, int> tuple) (folderPath, insertionIndex) = tuple;
        else { System.Diagnostics.Debug.WriteLine($"AddToQuickAccess invalid param: {parameter?.GetType()}"); return; }
        if (string.IsNullOrWhiteSpace(folderPath)) return;
        try
        {
            var fullPath = Path.GetFullPath(folderPath);
            if (File.Exists(fullPath))
            {
                var dir = Path.GetDirectoryName(fullPath);
                if (string.IsNullOrWhiteSpace(dir)) return;
                fullPath = dir;
            }
            if (!Directory.Exists(fullPath)) return;
            if (QuickAccessFolders.Any(f => string.Equals(f.FolderPath, fullPath, StringComparison.OrdinalIgnoreCase))) return;
            var name = Path.GetFileName(fullPath);
            if (string.IsNullOrEmpty(name)) name = fullPath;
            var qa = new QuickAccessFolder { FolderName = name, FolderPath = fullPath, IconGlyph = "\uE8B7" };
            if (insertionIndex >= 0 && insertionIndex < QuickAccessFolders.Count) QuickAccessFolders.Insert(insertionIndex, qa); else QuickAccessFolders.Add(qa);
            SaveQuickAccessFolders();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AddToQuickAccess error: {ex.Message}");
        }
    }

    private void ExecuteRemoveFromQuickAccess(QuickAccessFolder? folder)
    {
        if (folder == null) return;
        try
        {
            var idx = QuickAccessFolders.IndexOf(folder);
            var wasSelected = folder == SelectedQuickAccessFolder;
            folder.IsSelected = false;
            QuickAccessFolders.Remove(folder);
            SaveQuickAccessFolders();
            if (wasSelected)
            {
                if (QuickAccessFolders.Count > 0)
                {
                    var newIndex = System.Math.Min(idx, QuickAccessFolders.Count - 1);
                    ExecuteSelectQuickAccessFolder(QuickAccessFolders[newIndex]);
                }
                else
                {
                    ExecuteSelectRecent();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"RemoveQuickAccess error: {ex.Message}");
        }
    }

    public void SaveQuickAccessFolders()
    {
        var folders = QuickAccessFolders.Select(f => f.FolderPath).ToList();
        _settingsService.SetSetting("OpenBackstage.QuickAccessFolders", folders);
    }

    private void LoadQuickAccessFolders()
    {
        var saved = _settingsService.GetSetting<System.Collections.Generic.List<string>>("OpenBackstage.QuickAccessFolders", new System.Collections.Generic.List<string>());
        QuickAccessFolders.Clear();
        if (saved.Count == 0)
        {
            LoadDefaultQuickAccessFolders();
            return;
        }
        foreach (var path in saved)
        {
            if (Directory.Exists(path))
            {
                var name = Path.GetFileName(path);
                if (string.IsNullOrEmpty(name)) name = path;
                QuickAccessFolders.Add(new QuickAccessFolder { FolderName = name, FolderPath = path, IconGlyph = "\uE8B7" });
            }
        }
    }

    private void LoadDefaultQuickAccessFolders()
    {
        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        QuickAccessFolders.Add(new QuickAccessFolder { FolderName = "Documents", FolderPath = documents, IconGlyph = "\uE8B7" });
        QuickAccessFolders.Add(new QuickAccessFolder { FolderName = "Desktop", FolderPath = desktop, IconGlyph = "\uE8B7" });
        if (Directory.Exists(downloads)) QuickAccessFolders.Add(new QuickAccessFolder { FolderName = "Downloads", FolderPath = downloads, IconGlyph = "\uE8B7" });
        SaveQuickAccessFolders();
    }

    private void LoadSampleQuickAccessFolders() => LoadQuickAccessFolders();
}
