using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EasyAF.Modules.Spec.ViewModels;

namespace EasyAF.Modules.Spec.Views
{
    /// <summary>
    /// Interaction logic for TableEditorView.xaml
    /// </summary>
    /// <remarks>
    /// MVVM Pattern: This code-behind contains ONLY InitializeComponent() and scroll forwarding logic.
    /// All business logic is in TableEditorViewModel.
    /// </remarks>
    public partial class TableEditorView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the TableEditorView.
        /// </summary>
        public TableEditorView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles double-click on PropertyPaths cell to open the picker dialog.
        /// </summary>
        private void PropertyPaths_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Only trigger on double-click
            if (e.ClickCount != 2) return;

            // Get the ViewModel and trigger the Edit command
            if (DataContext is TableEditorViewModel viewModel && viewModel.EditColumnCommand.CanExecute(null))
            {
                viewModel.EditColumnCommand.Execute(null);
            }
        }

        /// <summary>
        /// Handles double-click on Filters DataGrid to edit the selected filter.
        /// </summary>
        private void FiltersDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Get the ViewModel and trigger the EditFilter command
            if (DataContext is TableEditorViewModel viewModel && viewModel.EditFilterCommand.CanExecute(null))
            {
                viewModel.EditFilterCommand.Execute(null);
            }
        }

        /// <summary>
        /// Forwards mouse wheel events from DataGrids to the parent ScrollViewer.
        /// This prevents DataGrids from absorbing scroll events.
        /// </summary>
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);

            // Only handle if the event originated from a DependencyObject and hasn't been handled yet
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
    }
}
