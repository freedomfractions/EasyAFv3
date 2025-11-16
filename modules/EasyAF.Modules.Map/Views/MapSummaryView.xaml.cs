using System.Windows.Controls;
using System.Windows;
using System.Linq;
using EasyAF.Modules.Map.ViewModels;

namespace EasyAF.Modules.Map.Views
{
    /// <summary>
    /// Interaction logic for MapSummaryView.xaml
    /// </summary>
    /// <remarks>
    /// This view displays:
    /// - Map metadata (name, version, description)
    /// - Referenced sample files with add/remove functionality
    /// - Data type mapping status overview with progress indicators
    /// 
    /// MVVM Pattern: Code-behind contains only event wire-up for drag-and-drop.
    /// All business logic is in MapSummaryViewModel.
    /// </remarks>
    public partial class MapSummaryView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the MapSummaryView.
        /// </summary>
        public MapSummaryView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles drag-over event to provide visual feedback.
        /// </summary>
        private void FilesDataGrid_DragOver(object sender, DragEventArgs e)
        {
            // Check if the dragged data contains files
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        /// <summary>
        /// Handles drop event by forwarding files to ViewModel.
        /// </summary>
        private void FilesDataGrid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0 && DataContext is MapSummaryViewModel viewModel)
                {
                    // Filter to only supported file types
                    var supportedExtensions = new[] { ".csv", ".xlsx", ".xls" };
                    var validFiles = files.Where(f =>
                    {
                        var ext = System.IO.Path.GetExtension(f).ToLowerInvariant();
                        return supportedExtensions.Contains(ext);
                    }).ToArray();

                    // Add files via ViewModel
                    viewModel.AddFilesFromDrop(validFiles);
                }
            }
            e.Handled = true;
        }
    }
}
