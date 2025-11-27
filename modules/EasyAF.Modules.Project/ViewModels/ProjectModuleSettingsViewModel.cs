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
        private string _templatesDirectory = string.Empty;
        private string _outputDirectory = string.Empty;

        /// <summary>
        /// Initializes a new instance of the ProjectModuleSettingsViewModel.
        /// </summary>
        public ProjectModuleSettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _settings = _settingsService.GetProjectModuleSettings();

            // Initialize collections
            AvailableMapFiles = new ObservableCollection<MapFileOption>();

            // CROSS-MODULE EDIT: 2025-01-27 Module Directory Settings Refactoring
            // Modified for: Initialize directory settings from saved preferences
            // Related modules: Core (CrossModuleSettingsExtensions)
            // Rollback instructions: Remove directory initialization
            
            // Load directory settings with fallback to defaults
            _templatesDirectory = _settings.TemplatesDirectory ?? GetDefaultTemplatesDirectory();
            _outputDirectory = _settings.OutputDirectory ?? GetDefaultOutputDirectory();

            // Commands
            BrowseMapCommand = new DelegateCommand(ExecuteBrowseMap);
            ClearDefaultMapCommand = new DelegateCommand(ExecuteClearDefaultMap, CanExecuteClearDefaultMap);
            BrowseTemplatesDirectoryCommand = new DelegateCommand(ExecuteBrowseTemplatesDirectory);
            BrowseOutputDirectoryCommand = new DelegateCommand(ExecuteBrowseOutputDirectory);

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
        
        /// <summary>
        /// Gets or sets the Templates directory path.
        /// </summary>
        /// <remarks>
        /// CROSS-MODULE EDIT: 2025-01-27 Module Directory Settings Refactoring
        /// Modified for: Templates directory moved from Shell to Project module
        /// Related modules: Core (future GetTemplatesDirectory() extension)
        /// Rollback instructions: Remove this property
        /// </remarks>
        public string TemplatesDirectory
        {
            get => _templatesDirectory;
            set
            {
                if (SetProperty(ref _templatesDirectory, value))
                {
                    _settings.TemplatesDirectory = value;
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the Output directory path.
        /// </summary>
        /// <remarks>
        /// CROSS-MODULE EDIT: 2025-01-27 Module Directory Settings Refactoring
        /// Modified for: Output directory moved from Shell to Project module
        /// Related modules: Core (future GetOutputDirectory() extension)
        /// Rollback instructions: Remove this property
        /// </remarks>
        public string OutputDirectory
        {
            get => _outputDirectory;
            set
            {
                if (SetProperty(ref _outputDirectory, value))
                {
                    _settings.OutputDirectory = value;
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
        
        /// <summary>
        /// Command to browse for Templates directory.
        /// </summary>
        /// <remarks>
        /// CROSS-MODULE EDIT: 2025-01-27 Module Directory Settings Refactoring
        /// Modified for: Add Templates directory selection to Project module settings
        /// Rollback instructions: Remove this command
        /// </remarks>
        public DelegateCommand BrowseTemplatesDirectoryCommand { get; }
        
        /// <summary>
        /// Command to browse for Output directory.
        /// </summary>
        /// <remarks>
        /// CROSS-MODULE EDIT: 2025-01-27 Module Directory Settings Refactoring
        /// Modified for: Add Output directory selection to Project module settings
        /// Rollback instructions: Remove this command
        /// </remarks>
        public DelegateCommand BrowseOutputDirectoryCommand { get; }

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
            
            // Only set selection if we found a match for the saved setting
            // Don't default to first item if no setting exists - let user explicitly choose
            if (currentOption != null)
            {
                SelectedDefaultMapPath = currentOption.FilePath;
            }
            else if (_settings.DefaultImportMapPath == null)
            {
                // If setting is explicitly null (never set), select "None" option
                SelectedDefaultMapPath = null;
            }
            // else: saved path doesn't exist in dropdown - leave unselected for now
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
        
        /// <summary>
        /// Executes the browse Templates directory command.
        /// </summary>
        /// <remarks>
        /// CROSS-MODULE EDIT: 2025-01-27 Module Directory Settings Refactoring
        /// Modified for: Allow users to select custom Templates directory
        /// Rollback instructions: Remove this method
        /// </remarks>
        private void ExecuteBrowseTemplatesDirectory()
        {
            try
            {
                var dialog = new Microsoft.Win32.OpenFolderDialog
                {
                    Title = "Select Templates Directory",
                    InitialDirectory = Directory.Exists(_templatesDirectory) 
                        ? _templatesDirectory 
                        : GetDefaultTemplatesDirectory()
                };

                if (dialog.ShowDialog() == true)
                {
                    TemplatesDirectory = dialog.FolderName;
                    Log.Information("Templates directory changed to: {Path}", dialog.FolderName);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error browsing for Templates directory");
            }
        }
        
        /// <summary>
        /// Executes the browse Output directory command.
        /// </summary>
        /// <remarks>
        /// CROSS-MODULE EDIT: 2025-01-27 Module Directory Settings Refactoring
        /// Modified for: Allow users to select custom Output directory
        /// Rollback instructions: Remove this method
        /// </remarks>
        private void ExecuteBrowseOutputDirectory()
        {
            try
            {
                var dialog = new Microsoft.Win32.OpenFolderDialog
                {
                    Title = "Select Output Directory",
                    InitialDirectory = Directory.Exists(_outputDirectory) 
                        ? _outputDirectory 
                        : GetDefaultOutputDirectory()
                };

                if (dialog.ShowDialog() == true)
                {
                    OutputDirectory = dialog.FolderName;
                    Log.Information("Output directory changed to: {Path}", dialog.FolderName);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error browsing for Output directory");
            }
        }
        
        /// <summary>
        /// Gets the default Templates directory path.
        /// </summary>
        /// <remarks>
        /// CROSS-MODULE EDIT: 2025-01-27 Module Directory Settings Refactoring
        /// Modified for: Provide fallback directory location
        /// Rollback instructions: Remove this method
        /// </remarks>
        private static string GetDefaultTemplatesDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "EasyAF",
                "Templates");
        }
        
        /// <summary>
        /// Gets the default Output directory path.
        /// </summary>
        /// <remarks>
        /// CROSS-MODULE EDIT: 2025-01-27 Module Directory Settings Refactoring
        /// Modified for: Provide fallback directory location
        /// Rollback instructions: Remove this method
        /// </remarks>
        private static string GetDefaultOutputDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "EasyAF",
                "Output");
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
