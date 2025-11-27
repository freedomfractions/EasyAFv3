using System.Windows.Controls;
using System.Windows;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
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
    /// MVVM Pattern: Code-behind contains only event wire-up for drag-and-drop
    /// and scroll forwarding logic to prevent DataGrids from absorbing scroll events.
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
        /// Forwards mouse wheel events from the DataGrid to the parent ScrollViewer.
        /// This prevents the DataGrid from absorbing scroll events.
        /// </summary>
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);

            // Only handle if the event originated from a DataGrid and hasn't been handled yet
            if (e.Handled || !(e.OriginalSource is DependencyObject source))
                return;

            // Find the DataGrid in the visual tree
            var dataGrid = FindParent<DataGrid>(source);
            if (dataGrid == null)
                return;

            // Find the parent ScrollViewer (the main ScrollViewer for the entire view)
            var scrollViewer = FindParent<ScrollViewer>(dataGrid);
            if (scrollViewer == null)
                return;

            // Forward the scroll to the parent ScrollViewer
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - (e.Delta / 3.0));
            e.Handled = true;
        }

        /// <summary>
        /// Finds the first parent of a specific type in the visual tree.
        /// </summary>
        private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject? parentObject = child;

            while (parentObject != null)
            {
                if (parentObject is T parent)
                    return parent;

                parentObject = VisualTreeHelper.GetParent(parentObject);
            }

            return null;
        }

        /// <summary>
        /// Handles drag-over event to provide visual feedback.
        /// </summary>
        private void FilesDataGrid_DragOver(object sender, DragEventArgs e)
        {
            // Check if the dragged data contains files
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Check if any of the files have valid extensions
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var supportedExtensions = new[] { ".csv", ".xlsx", ".xls" };
                
                var hasValidFiles = files != null && files.Any(f =>
                {
                    var ext = System.IO.Path.GetExtension(f).ToLowerInvariant();
                    return supportedExtensions.Contains(ext);
                });

                e.Effects = hasValidFiles ? DragDropEffects.Copy : DragDropEffects.None;
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
