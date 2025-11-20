using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace EasyAF.Shell.Behaviors
{
    /// <summary>
    /// CROSS-MODULE EDIT: 2025-01-20 Task 20 - DatePicker Scroll Behavior
    /// Modified for: Smart scroll behavior for DatePicker popup calendar
    /// Related modules: Project (ProjectSummaryView.xaml - DatePicker controls)
    /// Rollback instructions: Remove this file and DatePicker.Loaded event handler
    /// 
    /// Behavior:
    /// - When calendar popup is open and mouse is OVER the calendar: scroll changes months
    /// - When calendar popup is open and mouse is OUTSIDE the calendar: close popup and allow form scroll
    /// </summary>
    public static class DatePickerScrollBehavior
    {
        public static readonly DependencyProperty EnableScrollBehaviorProperty =
            DependencyProperty.RegisterAttached(
                "EnableScrollBehavior",
                typeof(bool),
                typeof(DatePickerScrollBehavior),
                new PropertyMetadata(false, OnEnableScrollBehaviorChanged));

        public static bool GetEnableScrollBehavior(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableScrollBehaviorProperty);
        }

        public static void SetEnableScrollBehavior(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableScrollBehaviorProperty, value);
        }

        private static void OnEnableScrollBehaviorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DatePicker datePicker)
            {
                if ((bool)e.NewValue)
                {
                    datePicker.Loaded += DatePicker_Loaded;
                }
                else
                {
                    datePicker.Loaded -= DatePicker_Loaded;
                }
            }
        }

        private static void DatePicker_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is DatePicker datePicker)
            {
                // Find the popup in the DatePicker template
                var popup = FindVisualChild<Popup>(datePicker);
                if (popup != null)
                {
                    // Attach to popup opened/closed events
                    popup.Opened += (s, args) => AttachScrollHandlers(datePicker, popup);
                    popup.Closed += (s, args) => DetachScrollHandlers(datePicker);
                }
            }
        }

        private static void AttachScrollHandlers(DatePicker datePicker, Popup popup)
        {
            // Attach PreviewMouseWheel to the DatePicker to intercept scroll events
            datePicker.PreviewMouseWheel += DatePicker_PreviewMouseWheel;
            
            // Store the popup reference for later use
            datePicker.SetValue(PopupReferenceProperty, popup);
        }

        private static void DetachScrollHandlers(DatePicker datePicker)
        {
            datePicker.PreviewMouseWheel -= DatePicker_PreviewMouseWheel;
            datePicker.ClearValue(PopupReferenceProperty);
        }

        private static readonly DependencyProperty PopupReferenceProperty =
            DependencyProperty.RegisterAttached("PopupReference", typeof(Popup), typeof(DatePickerScrollBehavior));

        private static void DatePicker_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is DatePicker datePicker)
            {
                var popup = (Popup)datePicker.GetValue(PopupReferenceProperty);
                if (popup == null || !popup.IsOpen)
                    return;

                // The popup.Child should be a Calendar control directly
                var calendar = popup.Child as Calendar;
                if (calendar == null)
                {
                    // If not direct, search for it
                    calendar = FindVisualChild<Calendar>(popup.Child);
                }

                if (calendar == null)
                    return;

                // Get mouse position relative to the calendar
                var mousePos = Mouse.GetPosition(calendar);
                bool isMouseOverCalendar = mousePos.X >= 0 && mousePos.Y >= 0 &&
                                           mousePos.X <= calendar.ActualWidth &&
                                           mousePos.Y <= calendar.ActualHeight;

                if (isMouseOverCalendar)
                {
                    // Mouse is over calendar - change months instead of scrolling form
                    e.Handled = true;
                    
                    // Change displayed month
                    if (e.Delta > 0)
                    {
                        // Scroll up = previous month
                        calendar.DisplayDate = calendar.DisplayDate.AddMonths(-1);
                    }
                    else
                    {
                        // Scroll down = next month
                        calendar.DisplayDate = calendar.DisplayDate.AddMonths(1);
                    }
                }
                else
                {
                    // Mouse is outside calendar - close popup and allow form to scroll
                    e.Handled = false;
                    datePicker.IsDropDownOpen = false;
                }
            }
        }

        /// <summary>
        /// Helper method to find a visual child of a specific type in the visual tree
        /// </summary>
        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
                return null;

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
