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
            
            // Handle PreviewMouseWheel to forward scroll events from ListView to parent ScrollViewer
            this.AddHandler(PreviewMouseWheelEvent, new MouseWheelEventHandler(OnPreviewMouseWheel), true);
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Let ListView scroll events bubble up to the parent ScrollViewer
            // This allows scrolling when mouse is over ListView items, not just the scrollbar
            if (e.Handled)
                return;

            e.Handled = true;
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = MouseWheelEvent,
                Source = sender
            };
            
            var parent = ((Control)sender).Parent as UIElement;
            parent?.RaiseEvent(eventArg);
        }
    }
}
