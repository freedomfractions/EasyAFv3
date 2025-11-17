using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Serilog;

namespace EasyAF.Modules.Map.ViewModels
{
    /// <summary>
    /// ViewModel for the Missing Files dialog.
    /// </summary>
    /// <remarks>
    /// This dialog is shown when a map document is loaded with missing referenced files.
    /// Users can browse for relocated files, remove missing references, or continue with missing files.
    /// </remarks>
    public class MissingFilesDialogViewModel : BindableBase
    {
        private bool? _dialogResult;

        /// <summary>
        /// Initializes a new instance of the MissingFilesDialogViewModel.
        /// </summary>
        /// <param name="missingFiles">List of missing file paths.</param>
        public MissingFilesDialogViewModel(System.Collections.Generic.List<string> missingFiles)
        {
            if (missingFiles == null) throw new ArgumentNullException(nameof(missingFiles));

            // Create file entries
            foreach (var filePath in missingFiles)
            {
                MissingFiles.Add(new MissingFileEntry
                {
                    OriginalPath = filePath,
                    FileName = System.IO.Path.GetFileName(filePath),
                    Status = "Missing"
                });
            }

            // Commands
            BrowseCommand = new DelegateCommand<MissingFileEntry>(ExecuteBrowse);
            RemoveCommand = new DelegateCommand<MissingFileEntry>(ExecuteRemove);
            RemoveAllCommand = new DelegateCommand(ExecuteRemoveAll);
            ContinueCommand = new DelegateCommand(ExecuteContinue);
            CancelCommand = new DelegateCommand(ExecuteCancel);

            Log.Debug("MissingFilesDialogViewModel initialized with {Count} missing files", MissingFiles.Count);
        }

        #region Properties

        /// <summary>
        /// Gets the collection of missing file entries.
        /// </summary>
        public ObservableCollection<MissingFileEntry> MissingFiles { get; } = new();

        /// <summary>
        /// Gets the count of remaining missing files.
        /// </summary>
        public int RemainingCount => MissingFiles.Count(f => f.Status == "Missing");

        /// <summary>
        /// Gets the count of resolved files.
        /// </summary>
        public int ResolvedCount => MissingFiles.Count(f => f.Status == "Resolved");

        /// <summary>
        /// Gets the count of files marked for removal.
        /// </summary>
        public int RemovedCount => MissingFiles.Count(f => f.Status == "Removed");

        /// <summary>
        /// Gets or sets the dialog result.
        /// </summary>
        public bool? DialogResult
        {
            get => _dialogResult;
            set => SetProperty(ref _dialogResult, value);
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to browse for a relocated file.
        /// </summary>
        public ICommand BrowseCommand { get; }

        /// <summary>
        /// Command to remove a single file reference.
        /// </summary>
        public ICommand RemoveCommand { get; }

        /// <summary>
        /// Command to remove all missing file references.
        /// </summary>
        public ICommand RemoveAllCommand { get; }

        /// <summary>
        /// Command to continue with the current state.
        /// </summary>
        public ICommand ContinueCommand { get; }

        /// <summary>
        /// Command to cancel the operation.
        /// </summary>
        public ICommand CancelCommand { get; }

        #endregion

        #region Command Implementations

        private void ExecuteBrowse(MissingFileEntry? entry)
        {
            if (entry == null) return;

            try
            {
                var dialog = new OpenFileDialog
                {
                    Title = $"Locate '{entry.FileName}'",
                    Filter = "All Files (*.*)|*.*|CSV Files (*.csv)|*.csv|Excel Files (*.xlsx;*.xls)|*.xlsx;*.xls",
                    FileName = entry.FileName,
                    CheckFileExists = true,
                    Multiselect = false
                };

                // Try to set initial directory to the original file's directory
                var originalDir = System.IO.Path.GetDirectoryName(entry.OriginalPath);
                if (!string.IsNullOrWhiteSpace(originalDir) && System.IO.Directory.Exists(originalDir))
                {
                    dialog.InitialDirectory = originalDir;
                }

                if (dialog.ShowDialog() == true)
                {
                    entry.NewPath = dialog.FileName;
                    entry.Status = "Resolved";
                    Log.Information("User located missing file: {Original} -> {New}", entry.OriginalPath, entry.NewPath);
                    RefreshCounts();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error browsing for file: {Path}", entry.OriginalPath);
            }
        }

        private void ExecuteRemove(MissingFileEntry? entry)
        {
            if (entry == null) return;

            entry.Status = "Removed";
            Log.Information("User marked file for removal: {Path}", entry.OriginalPath);
            RefreshCounts();
        }

        private void ExecuteRemoveAll()
        {
            foreach (var file in MissingFiles.Where(f => f.Status == "Missing"))
            {
                file.Status = "Removed";
            }
            Log.Information("User marked all missing files for removal");
            RefreshCounts();
        }

        private void ExecuteContinue()
        {
            DialogResult = true;
            Log.Debug("User confirmed missing files dialog (Resolved: {Resolved}, Removed: {Removed}, Remaining: {Remaining})",
                ResolvedCount, RemovedCount, RemainingCount);
        }

        private void ExecuteCancel()
        {
            DialogResult = false;
            Log.Debug("User cancelled missing files dialog");
        }

        #endregion

        #region Helper Methods

        private void RefreshCounts()
        {
            RaisePropertyChanged(nameof(RemainingCount));
            RaisePropertyChanged(nameof(ResolvedCount));
            RaisePropertyChanged(nameof(RemovedCount));
        }

        #endregion
    }

    /// <summary>
    /// Represents a missing file entry in the dialog.
    /// </summary>
    public class MissingFileEntry : BindableBase
    {
        private string _status = "Missing";
        private string? _newPath;

        /// <summary>
        /// Gets or sets the original file path (that's missing).
        /// </summary>
        public string OriginalPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file name (for display).
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the status: "Missing", "Resolved", "Removed".
        /// </summary>
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        /// <summary>
        /// Gets or sets the new file path (if user located it).
        /// </summary>
        public string? NewPath
        {
            get => _newPath;
            set => SetProperty(ref _newPath, value);
        }

        /// <summary>
        /// Gets the display path (new path if resolved, otherwise original).
        /// </summary>
        public string DisplayPath => !string.IsNullOrWhiteSpace(NewPath) ? NewPath : OriginalPath;
    }
}
