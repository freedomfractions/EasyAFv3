using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace EasyAF.Modules.Project.Views
{
    /// <summary>
    /// Interaction logic for ProjectSummaryView.xaml
    /// </summary>
    /// <remarks>
    /// MVVM Compliance: This code-behind contains only InitializeComponent() and scroll forwarding logic.
    /// All business logic is in ProjectSummaryViewModel.
    /// </remarks>
    public partial class ProjectSummaryView : UserControl
    {
        public ProjectSummaryView()
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

            // Only handle if the event originated from a DependencyObject and hasn't been handled yet
            if (e.Handled || !(e.OriginalSource is DependencyObject source))
                return;

            // Skip if source is not a Visual (e.g., Run elements in text)
            if (source is not Visual and not Visual3D)
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

                // Only try to get parent if current object is a Visual or Visual3D
                if (parentObject is Visual or Visual3D)
                {
                    parentObject = VisualTreeHelper.GetParent(parentObject);
                }
                else
                {
                    // Not in visual tree, exit early
                    return null;
                }
            }

            return null;
        }
    }
}
