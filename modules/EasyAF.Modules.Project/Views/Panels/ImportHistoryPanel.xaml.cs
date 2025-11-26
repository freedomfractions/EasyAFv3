using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EasyAF.Modules.Project.Views.Panels
{
    /// <summary>
    /// Interaction logic for ImportHistoryPanel.xaml
    /// </summary>
    /// <remarks>
    /// This panel displays the import history in a hierarchical tree structure:
    /// - Parent nodes: Import sessions (grouped by timestamp)
    /// - Child nodes: Individual files imported in that session
    /// 
    /// Displays separate trees for New Data and Old Data imports.
    /// 
    /// MVVM Pattern: Code-behind is minimal. All logic is in ProjectSummaryViewModel.
    /// </remarks>
    public partial class ImportHistoryPanel : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the ImportHistoryPanel.
        /// </summary>
        public ImportHistoryPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles mouse wheel events to allow scrolling to bubble up to parent ScrollViewer
        /// when TreeView has reached its scroll limit.
        /// </summary>
        private void TreeView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is not TreeView treeView)
                return;

            // Find the TreeView's internal ScrollViewer
            var scrollViewer = FindVisualChild<ScrollViewer>(treeView);
            if (scrollViewer == null)
                return;

            // Check if we're at the scroll limit
            bool atTop = scrollViewer.VerticalOffset == 0 && e.Delta > 0;
            bool atBottom = scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight && e.Delta < 0;

            // If at limit, don't handle the event - let it bubble to parent
            if (atTop || atBottom)
            {
                e.Handled = false;
                
                // Manually bubble the event to parent
                var parent = treeView.Parent as UIElement;
                if (parent != null)
                {
                    var newEvent = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                    {
                        RoutedEvent = UIElement.MouseWheelEvent,
                        Source = treeView
                    };
                    parent.RaiseEvent(newEvent);
                }
            }
        }

        /// <summary>
        /// Finds a visual child of a specific type in the visual tree.
        /// </summary>
        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
