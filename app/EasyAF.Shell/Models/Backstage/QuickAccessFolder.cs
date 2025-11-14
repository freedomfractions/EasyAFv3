namespace EasyAF.Shell.Models.Backstage;

/// <summary>
/// Represents a pinned folder in the Quick Access section of the Open backstage.
/// </summary>
public class QuickAccessFolder
{
    /// <summary>
    /// Display name of the folder (e.g., "Projects", "Templates").
    /// </summary>
    public string FolderName { get; set; } = string.Empty;

    /// <summary>
    /// Full path to the folder on disk.
    /// </summary>
    public string FolderPath { get; set; } = string.Empty;

    /// <summary>
    /// Icon glyph for the folder (Segoe MDL2 Assets code).
    /// </summary>
    public string IconGlyph { get; set; } = "\uE8B7"; // Folder icon by default
}
