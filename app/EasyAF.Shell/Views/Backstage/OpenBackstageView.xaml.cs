using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EasyAF.Shell.Views.Backstage
{
    /// <summary>
    /// OpenBackstageView code-behind. Handles scroll event forwarding from ListViews and scroll position reset.
    /// </summary>
    public partial class OpenBackstageView : UserControl
    {
        public OpenBackstageView()
        {
            InitializeComponent();
            
            // CROSS-MODULE EDIT: 2025-01-15 Option B Polish (2/2)
            // Modified for: Subscribe to FocusSearchRequested event to focus search box
            // Rollback instructions: Remove Loaded event handler and FocusSearchRequested subscription
            this.Loaded += OnLoaded;
            
            // Handle keyboard navigation for Page Up/Down in ListViews
            this.AddHandler(PreviewKeyDownEvent, new KeyEventHandler(OnPreviewKeyDown), true);
        }
        
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Subscribe to ViewModel's events
            if (DataContext is ViewModels.Backstage.OpenBackstageViewModel vm)
            {
                vm.FocusSearchRequested += OnFocusSearchRequested;
                vm.ScrollToTopRequested += OnScrollToTopRequested;
            }
        }
        
        private void OnFocusSearchRequested(object? sender, EventArgs e)
        {
            // Focus the search TextBox
            SearchTextBox.Focus();
            SearchTextBox.SelectAll();
        }
        
        /// <summary>
        /// Handles scroll-to-top requests from the ViewModel.
        /// Resets the scroll position of the main content area to the top (0,0).
        /// </summary>
        /// <remarks>
        /// Tries to scroll via ListView.ScrollIntoView first (better for virtualization),
        /// then falls back to ScrollViewer.ScrollToTop if no ListView is found.
        /// </remarks>
        private void OnScrollToTopRequested(object? sender, EventArgs e)
        {
            // Try ListView first (better for virtualized collections)
            var listView = FindVisualChild<ListView>(this);
            if (listView?.Items.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"[ScrollReset] ListView found with {listView.Items.Count} items - scrolling to first item");
                listView.ScrollIntoView(listView.Items[0]);
                listView.SelectedIndex = -1; // Clear selection
                return;
            }
            
            System.Diagnostics.Debug.WriteLine("[ScrollReset] No ListView found, trying ScrollViewer...");
            
            // Fallback to ScrollViewer
            var scrollViewer = FindVisualChild<ScrollViewer>(this);
            if (scrollViewer != null)
            {
                System.Diagnostics.Debug.WriteLine($"[ScrollReset] ScrollViewer found - current offset: {scrollViewer.VerticalOffset}");
                scrollViewer.ScrollToTop();
                System.Diagnostics.Debug.WriteLine($"[ScrollReset] After reset - offset: {scrollViewer.VerticalOffset}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[ScrollReset] ERROR: No ScrollViewer found in visual tree!");
            }
        }

        /// <summary>
        /// Finds the first child of a specific type in the visual tree.
        /// </summary>
        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
                return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                
                if (child is T typedChild)
                    return typedChild;

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }

            return null;
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
