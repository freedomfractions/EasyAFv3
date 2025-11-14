using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EasyAF.Shell.Views.Backstage
{
    /// <summary>
    /// OpenBackstageView code-behind. Handles scroll event forwarding from ListViews.
    /// </summary>
    public partial class OpenBackstageView : UserControl
    {
        public OpenBackstageView()
        {
            InitializeComponent();
            
            // Handle keyboard navigation for Page Up/Down in ListViews
            this.AddHandler(PreviewKeyDownEvent, new KeyEventHandler(OnPreviewKeyDown), true);
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);

            // Only handle if the event originated from a ListView and hasn't been handled yet
            if (e.Handled || !(e.OriginalSource is DependencyObject source))
                return;

            // Find the ListView in the visual tree
            var listView = FindParent<ListView>(source);
            if (listView == null)
                return;

            // Find the ScrollViewer that contains the ListView
            var scrollViewer = FindParent<ScrollViewer>(listView);
            if (scrollViewer == null)
                return;

            // Forward the scroll to the ScrollViewer
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - (e.Delta / 3.0));
            e.Handled = true;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Handle Page Up/Down to scroll the parent ScrollViewer instead of jumping to first/last item
            if (e.Key != Key.PageUp && e.Key != Key.PageDown)
                return;

            if (!(e.OriginalSource is DependencyObject source))
                return;

            // Find the ListView
            var listView = FindParent<ListView>(source);
            if (listView == null)
                return;

            // Find the parent ScrollViewer
            var scrollViewer = FindParent<ScrollViewer>(listView);
            if (scrollViewer == null)
                return;

            // Calculate page size (viewport height)
            var pageSize = scrollViewer.ViewportHeight;

            // Scroll by page size
            if (e.Key == Key.PageUp)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - pageSize);
            }
            else // PageDown
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + pageSize);
            }

            e.Handled = true;
        }

        private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject? parentObject = child;

            while (parentObject != null)
            {
                if (parentObject is T parent)
                    return parent;

                parentObject = System.Windows.Media.VisualTreeHelper.GetParent(parentObject);
            }

            return null;
        }
    }
}
