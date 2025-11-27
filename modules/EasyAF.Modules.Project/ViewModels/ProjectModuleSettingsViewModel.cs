using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Prism.Mvvm;
using Prism.Commands;
using EasyAF.Core.Contracts;
using EasyAF.Modules.Project.Models;
using EasyAF.Modules.Project.Services;
using Microsoft.Win32;
using Serilog;

namespace EasyAF.Modules.Project.ViewModels
{
    /// <summary>
    /// View model for Project Module settings in the Options dialog.
    /// </summary>
    public class ProjectModuleSettingsViewModel : BindableBase
    {
        private readonly ISettingsService _settingsService;
        private ProjectModuleSettings _settings;
        private string? _selectedDefaultMapPath;

        /// <summary>
        /// Initializes a new instance of the ProjectModuleSettingsViewModel.
        /// </summary>
        public ProjectModuleSettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _settings = _settingsService.GetProjectModuleSettings();

            // Initialize collections
            AvailableMapFiles = new ObservableCollection<MapFileOption>();

            // Commands
            BrowseMapCommand = new DelegateCommand(ExecuteBrowseMap);
            ClearDefaultMapCommand = new DelegateCommand(ExecuteClearDefaultMap, CanExecuteClearDefaultMap);

            // Load available maps
            LoadAvailableMapFiles();

            Log.Debug("ProjectModuleSettingsViewModel initialized");
        }

        #region Properties

        /// <summary>
        /// Gets the collection of available map files.
        /// </summary>
        public ObservableCollection<MapFileOption> AvailableMapFiles { get; }

        /// <summary>
        /// Gets or sets the selected default map path.
        /// </summary>
        public string? SelectedDefaultMapPath
        {
            get => _selectedDefaultMapPath;
            set
            {
                if (SetProperty(ref _selectedDefaultMapPath, value))
                {
                    _settings.DefaultImportMapPath = value;
                    (ClearDefaultMapCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to browse for a custom map file.
        /// </summary>
        public DelegateCommand BrowseMapCommand { get; }

        /// <summary>
        /// Command to clear the default map selection.
        /// </summary>
        public DelegateCommand ClearDefaultMapCommand { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Loads available map files from the default Maps folder.
        /// </summary>
        private void LoadAvailableMapFiles()
        {
            AvailableMapFiles.Clear();

            // Add "None (use most recent)" option
            AvailableMapFiles.Add(new MapFileOption
            {
                DisplayName = "(None - use most recent)",
                FilePath = null
            });

            try
            {
                var mapsFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "EasyAF",
                    "Maps");

                if (Directory.Exists(mapsFolder))
                {
                    var mapFiles = Directory.GetFiles(mapsFolder, "*.ezmap")
                        .Select(f => new FileInfo(f))
                        .OrderBy(f => f.Name) // Alphabetical
                        .ToList();

                    foreach (var file in mapFiles)
                    {
                        AvailableMapFiles.Add(new MapFileOption
                        {
                            DisplayName = Path.GetFileNameWithoutExtension(file.Name),
                            FilePath = file.FullName
                        });
                    }

                    Log.Debug("Loaded {Count} map files for settings", mapFiles.Count);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to scan Maps folder for settings");
            }

            // Add current setting if it's a custom path not in the folder
            if (!string.IsNullOrWhiteSpace(_settings.DefaultImportMapPath) &&
                !AvailableMapFiles.Any(m => string.Equals(m.FilePath, _settings.DefaultImportMapPath, StringComparison.OrdinalIgnoreCase)))
            {
                AvailableMapFiles.Add(new MapFileOption
                {
                    DisplayName = Path.GetFileNameWithoutExtension(_settings.DefaultImportMapPath) + " (custom)",
                    FilePath = _settings.DefaultImportMapPath
                });
            }

            // Set current selection
            var currentOption = AvailableMapFiles.FirstOrDefault(m => 
                string.Equals(m.FilePath, _settings.DefaultImportMapPath, StringComparison.OrdinalIgnoreCase));
            
            SelectedDefaultMapPath = currentOption?.FilePath ?? AvailableMapFiles.FirstOrDefault()?.FilePath;
        }

        /// <summary>
        /// Executes the browse map command.
        /// </summary>
        private void ExecuteBrowseMap()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Default Import Map",
                Filter = "Map Files (*.ezmap)|*.ezmap|All Files (*.*)|*.*",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                // Add to available maps if not already there
                if (!AvailableMapFiles.Any(m => string.Equals(m.FilePath, dialog.FileName, StringComparison.OrdinalIgnoreCase)))
                {
                    var displayName = Path.GetFileNameWithoutExtension(dialog.FileName) + " (custom)";
                    AvailableMapFiles.Add(new MapFileOption
                    {
                        DisplayName = displayName,
                        FilePath = dialog.FileName
                    });
                }

                // Select the browsed file
                SelectedDefaultMapPath = dialog.FileName;
                
                Log.Information("Custom default map selected: {Path}", dialog.FileName);
            }
        }

        /// <summary>
        /// Determines if the clear default map command can execute.
        /// </summary>
        private bool CanExecuteClearDefaultMap()
        {
            return !string.IsNullOrWhiteSpace(SelectedDefaultMapPath);
        }

        /// <summary>
        /// Executes the clear default map command.
        /// </summary>
        private void ExecuteClearDefaultMap()
        {
            SelectedDefaultMapPath = null;
            Log.Information("Default map cleared - will use most recent");
        }

        /// <summary>
        /// Saves the current settings.
        /// </summary>
        public void SaveSettings()
        {
            _settingsService.SetProjectModuleSettings(_settings);
            Log.Information("Project Module settings saved - Default map: {Path}", 
                _settings.DefaultImportMapPath ?? "(none - use most recent)");
        }

        #endregion

        #region Nested Types

        /// <summary>
        /// Represents a map file option in the dropdown.
        /// </summary>
        public class MapFileOption
        {
            /// <summary>
            /// Display name for the dropdown.
            /// </summary>
            public string DisplayName { get; set; } = string.Empty;

            /// <summary>
            /// Full file path (null for "None" option).
            /// </summary>
            public string? FilePath { get; set; }
        }

        #endregion
    }
}
