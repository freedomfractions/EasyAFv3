using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using EasyAF.Modules.Project.Models;
using Serilog;

namespace EasyAF.Modules.Project.ViewModels
{
    /// <summary>
    /// View model for the project summary tab showing metadata and statistics.
    /// </summary>
    /// <remarks>
    /// This tab provides:
    /// - Project metadata editing (LB Project Number, Site Name, Client, etc.)
    /// - File management (add/remove referenced files)
    /// - Data statistics (equipment counts for New/Old data)
    /// </remarks>
    public class ProjectSummaryViewModel : BindableBase, IDisposable
    {
        private readonly ProjectDocument _document;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the ProjectSummaryViewModel.
        /// </summary>
        /// <param name="document">The project document.</param>
        /// <exception cref="ArgumentNullException">If document is null.</exception>
        public ProjectSummaryViewModel(ProjectDocument document)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));

            // Commands
            AddFileCommand = new DelegateCommand(ExecuteAddFile);
            RemoveFileCommand = new DelegateCommand(ExecuteRemoveFile, CanExecuteRemoveFile);
            BrowseMapCommand = new DelegateCommand(ExecuteBrowseMap);
            BrowseSpecCommand = new DelegateCommand(ExecuteBrowseSpec);
            BrowseTemplateCommand = new DelegateCommand(ExecuteBrowseTemplate);
            BrowseOutputCommand = new DelegateCommand(ExecuteBrowseOutput);

            Log.Debug("ProjectSummaryViewModel initialized");
        }

        #region Metadata Properties

        /// <summary>
        /// Gets or sets the LB Project Number.
        /// </summary>
        public string? LBProjectNumber
        {
            get => _document.Project.LBProjectNumber;
            set
            {
                if (_document.Project.LBProjectNumber != value)
                {
                    _document.Project.LBProjectNumber = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Site Name.
        /// </summary>
        public string? SiteName
        {
            get => _document.Project.SiteName;
            set
            {
                if (_document.Project.SiteName != value)
                {
                    _document.Project.SiteName = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Client name.
        /// </summary>
        public string? Client
        {
            get => _document.Project.Client;
            set
            {
                if (_document.Project.Client != value)
                {
                    _document.Project.Client = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Study Engineer.
        /// </summary>
        public string? StudyEngineer
        {
            get => _document.Project.StudyEngineer;
            set
            {
                if (_document.Project.StudyEngineer != value)
                {
                    _document.Project.StudyEngineer = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets Address Line 1.
        /// </summary>
        public string? AddressLine1
        {
            get => _document.Project.AddressLine1;
            set
            {
                if (_document.Project.AddressLine1 != value)
                {
                    _document.Project.AddressLine1 = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets Address Line 2.
        /// </summary>
        public string? AddressLine2
        {
            get => _document.Project.AddressLine2;
            set
            {
                if (_document.Project.AddressLine2 != value)
                {
                    _document.Project.AddressLine2 = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets Address Line 3.
        /// </summary>
        public string? AddressLine3
        {
            get => _document.Project.AddressLine3;
            set
            {
                if (_document.Project.AddressLine3 != value)
                {
                    _document.Project.AddressLine3 = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the City.
        /// </summary>
        public string? City
        {
            get => _document.Project.City;
            set
            {
                if (_document.Project.City != value)
                {
                    _document.Project.City = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the State (2-letter abbreviation).
        /// </summary>
        public string? State
        {
            get => _document.Project.State;
            set
            {
                if (_document.Project.State != value)
                {
                    _document.Project.State = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Zip Code.
        /// </summary>
        public string? Zip
        {
            get => _document.Project.Zip;
            set
            {
                if (_document.Project.Zip != value)
                {
                    _document.Project.Zip = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Study Date.
        /// </summary>
        public string? StudyDate
        {
            get => _document.Project.StudyDate;
            set
            {
                if (_document.Project.StudyDate != value)
                {
                    _document.Project.StudyDate = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(StudyDateValue)); // Notify DatePicker binding
                }
            }
        }

        /// <summary>
        /// Gets or sets the Study Date as a DateTime for DatePicker binding.
        /// </summary>
        /// <remarks>
        /// This property converts between the string StudyDate (stored in Project)
        /// and DateTime? (required by DatePicker).
        /// Always displays in "MMM dd, yyyy" format regardless of user's regional settings.
        /// When a date is selected and Revision is blank, auto-populates Revision with "MMMM yyyy".
        /// </remarks>
        public DateTime? StudyDateValue
        {
            get
            {
                if (string.IsNullOrWhiteSpace(StudyDate))
                    return null;
                
                if (DateTime.TryParse(StudyDate, out var date))
                    return date;
                
                return null;
            }
            set
            {
                // Store in consistent "MMM dd, yyyy" format for all users
                StudyDate = value?.ToString("MMM dd, yyyy", System.Globalization.CultureInfo.InvariantCulture);
                
                // Auto-populate Revision (Study Month) if blank and date is selected
                if (value.HasValue && string.IsNullOrWhiteSpace(Revision))
                {
                    Revision = value.Value.ToString("MMMM yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    Log.Debug("Auto-populated Revision from Study Date: {Revision}", Revision);
                }
            }
        }

        /// <summary>
        /// Gets or sets the Revision (Revision Month).
        /// </summary>
        public string? Revision
        {
            get => _document.Project.Revision;
            set
            {
                if (_document.Project.Revision != value)
                {
                    _document.Project.Revision = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Comments.
        /// </summary>
        public string? Comments
        {
            get => _document.Project.Comments;
            set
            {
                if (_document.Project.Comments != value)
                {
                    _document.Project.Comments = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region File Paths (Report Section)

        /// <summary>
        /// Gets or sets the Map file path.
        /// </summary>
        public string? MapPath
        {
            get => _document.Project.MapPathHistory?.FirstOrDefault();
            set
            {
                var current = _document.Project.MapPathHistory?.FirstOrDefault();
                if (current != value && !string.IsNullOrWhiteSpace(value))
                {
                    if (_document.Project.MapPathHistory == null)
                        _document.Project.MapPathHistory = new System.Collections.Generic.List<string>();
                    
                    _document.Project.MapPathHistory.Remove(value); // Remove if exists
                    _document.Project.MapPathHistory.Insert(0, value); // Add at front
                    
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Spec file path.
        /// </summary>
        public string? SpecPath
        {
            get => _document.Project.SpecPathHistory?.FirstOrDefault();
            set
            {
                var current = _document.Project.SpecPathHistory?.FirstOrDefault();
                if (current != value && !string.IsNullOrWhiteSpace(value))
                {
                    if (_document.Project.SpecPathHistory == null)
                        _document.Project.SpecPathHistory = new System.Collections.Generic.List<string>();
                    
                    _document.Project.SpecPathHistory.Remove(value);
                    _document.Project.SpecPathHistory.Insert(0, value);
                    
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Template file path.
        /// </summary>
        public string? TemplatePath
        {
            get => _document.Project.TemplatePath;
            set
            {
                if (_document.Project.TemplatePath != value)
                {
                    _document.Project.TemplatePath = value;
                    _document.MarkDirty();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Output folder path.
        /// </summary>
        public string? OutputPath { get; set; } // Not persisted in Project, just UI state

        #endregion

        #region Statistics

        /// <summary>
        /// Gets the count of buses in New data.
        /// </summary>
        public int NewBusCount => _document.Project.NewData?.BusEntries?.Count ?? 0;

        /// <summary>
        /// Gets the count of breakers in New data.
        /// </summary>
        public int NewBreakerCount => _document.Project.NewData?.LVBreakerEntries?.Count ?? 0;

        /// <summary>
        /// Gets the count of fuses in New data.
        /// </summary>
        public int NewFuseCount => _document.Project.NewData?.FuseEntries?.Count ?? 0;

        /// <summary>
        /// Gets the count of short circuit entries in New data.
        /// </summary>
        public int NewShortCircuitCount => _document.Project.NewData?.ShortCircuitEntries?.Count ?? 0;

        /// <summary>
        /// Gets the count of arc flash entries in New data.
        /// </summary>
        public int NewArcFlashCount => _document.Project.NewData?.ArcFlashEntries?.Count ?? 0;

        /// <summary>
        /// Gets the count of buses in Old data.
        /// </summary>
        public int OldBusCount => _document.Project.OldData?.BusEntries?.Count ?? 0;

        /// <summary>
        /// Gets the count of breakers in Old data.
        /// </summary>
        public int OldBreakerCount => _document.Project.OldData?.LVBreakerEntries?.Count ?? 0;

        /// <summary>
        /// Gets the count of fuses in Old data.
        /// </summary>
        public int OldFuseCount => _document.Project.OldData?.FuseEntries?.Count ?? 0;

        /// <summary>
        /// Gets the count of short circuit entries in Old data.
        /// </summary>
        public int OldShortCircuitCount => _document.Project.OldData?.ShortCircuitEntries?.Count ?? 0;

        /// <summary>
        /// Gets the count of arc flash entries in Old data.
        /// </summary>
        public int OldArcFlashCount => _document.Project.OldData?.ArcFlashEntries?.Count ?? 0;

        #endregion

        #region Commands

        public ICommand AddFileCommand { get; }
        public ICommand RemoveFileCommand { get; }
        public ICommand BrowseMapCommand { get; }
        public ICommand BrowseSpecCommand { get; }
        public ICommand BrowseTemplateCommand { get; }
        public ICommand BrowseOutputCommand { get; }

        private void ExecuteAddFile()
        {
            // TODO Task 21: Implement file import (CSV/Excel to DataSet)
            Log.Information("AddFile command - To be implemented in Task 21");
        }

        private bool CanExecuteRemoveFile()
        {
            // TODO Task 21: Check if files are selected
            return false;
        }

        private void ExecuteRemoveFile()
        {
            // TODO Task 21: Remove selected files
            Log.Information("RemoveFile command - To be implemented in Task 21");
        }

        private void ExecuteBrowseMap()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Map File",
                Filter = "Map Files (*.ezmap)|*.ezmap|All Files (*.*)|*.*",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                MapPath = dialog.FileName;
                Log.Information("Map file selected: {Path}", dialog.FileName);
            }
        }

        private void ExecuteBrowseSpec()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Spec File",
                Filter = "Spec Files (*.ezspec)|*.ezspec|JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                SpecPath = dialog.FileName;
                Log.Information("Spec file selected: {Path}", dialog.FileName);
            }
        }

        private void ExecuteBrowseTemplate()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Template File",
                Filter = "Word Documents (*.docx;*.dotx)|*.docx;*.dotx|All Files (*.*)|*.*",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                TemplatePath = dialog.FileName;
                Log.Information("Template file selected: {Path}", dialog.FileName);
            }
        }

        private void ExecuteBrowseOutput()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Output Folder",
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OutputPath = dialog.SelectedPath;
                RaisePropertyChanged(nameof(OutputPath));
                Log.Information("Output folder selected: {Path}", dialog.SelectedPath);
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Refreshes statistics display.
        /// </summary>
        /// <remarks>
        /// This should be called manually after data import operations.
        /// Since Project class doesn't implement INotifyPropertyChanged,
        /// we can't automatically detect changes.
        /// </remarks>
        public void RefreshStatistics()
        {
            RaisePropertyChanged(nameof(NewBusCount));
            RaisePropertyChanged(nameof(NewBreakerCount));
            RaisePropertyChanged(nameof(NewFuseCount));
            RaisePropertyChanged(nameof(NewShortCircuitCount));
            RaisePropertyChanged(nameof(NewArcFlashCount));
            RaisePropertyChanged(nameof(OldBusCount));
            RaisePropertyChanged(nameof(OldBreakerCount));
            RaisePropertyChanged(nameof(OldFuseCount));
            RaisePropertyChanged(nameof(OldShortCircuitCount));
            RaisePropertyChanged(nameof(OldArcFlashCount));
        }

        #endregion

        #region Cleanup

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
