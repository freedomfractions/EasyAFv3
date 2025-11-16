using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyAF.Data.Models
{
    /// <summary>
    /// Represents persisted global application settings (user scope) such as default date format and MRU paths.
    /// Stored at %APPDATA%/EasyAF/settings.json (Windows) or ~/.config/EasyAF/settings.json (Unix).
    /// </summary>
    public sealed class GlobalSettings
    {
        private static readonly object _sync = new();
        private static GlobalSettings? _current;
        private static string? _settingsPath;

        /// <summary>Default date format applied when a Project lacks PreferredDateFormat (fallback after hard-coded constant).</summary>
        public string DateFormat { get; set; } = "MMMM yyyy"; // legacy/global default
        /// <summary>Display/serialization format for Study Date fields.</summary>
        public string StudyDateFormat { get; set; } = "MMM d, yyyy";
        /// <summary>Display/serialization format for Study Revision date.</summary>
        public string RevisionDateFormat { get; set; } = "MMM yyyy";
        /// <summary>Most recently used project file paths (most recent first).</summary>
        public List<string> RecentProjects { get; set; } = new();
        /// <summary>Last directory browsed for specs.</summary>
        public string? LastSpecDir { get; set; }
        /// <summary>Last directory browsed for mappings.</summary>
        public string? LastMapDir { get; set; }
        /// <summary>Last directory browsed for projects.</summary>
        public string? LastProjectDir { get; set; }
        /// <summary>Arbitrary additional preferences (future‑proof).</summary>
        public Dictionary<string,string> Preferences { get; set; } = new();

        [JsonIgnore]
        public DateTime LoadedUtc { get; private set; } = DateTime.UtcNow;

        public static GlobalSettings Current
        {
            get
            {
                if (_current != null) return _current;
                lock (_sync)
                {
                    if (_current == null)
                    {
                        _settingsPath = ResolveSettingsPath();
                        _current = LoadInternal(_settingsPath);
                    }
                }
                return _current!;
            }
        }

        /// <summary>Add a project to the MRU list, de-duplicating and bounding size (max 15).</summary>
        public void AddRecentProject(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            path = Path.GetFullPath(path);
            RecentProjects.RemoveAll(p => string.Equals(Path.GetFullPath(p), path, StringComparison.OrdinalIgnoreCase));
            RecentProjects.Insert(0, path);
            if (RecentProjects.Count > 15) RecentProjects.RemoveRange(15, RecentProjects.Count - 15);
        }

        /// <summary>Persist current settings to disk.</summary>
        public void Save()
        {
            try
            {
                if (_settingsPath == null) _settingsPath = ResolveSettingsPath();
                Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsPath, json);
            }
            catch { /* swallow – non-fatal */ }
        }

        private static GlobalSettings LoadInternal(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    var gs = JsonSerializer.Deserialize<GlobalSettings>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (gs != null) { gs.LoadedUtc = DateTime.UtcNow; return gs; }
                }
            }
            catch { /* ignore corrupt file and recreate */ }
            return new GlobalSettings();
        }

        private static string ResolveSettingsPath()
        {
            string baseDir;
            try
            {
                baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (string.IsNullOrWhiteSpace(baseDir)) baseDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
            catch { baseDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile); }
            if (string.IsNullOrWhiteSpace(baseDir)) baseDir = ".";
            return Path.Combine(baseDir, "EasyAF", "settings.json");
        }
    }
}
