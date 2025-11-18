using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EasyAF.Modules.Map.Converters
{
    /// <summary>
    /// Converts a boolean value to Visibility, inverting the logic.
    /// True ? Collapsed, False ? Visible.
    /// </summary>
    /// <remarks>
    /// Used to show elements when a condition is NOT met, such as showing
    /// a placeholder message when no table is selected (HasTableSelected = false).
    /// </remarks>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible; // Default to visible if value is not bool
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("InverseBoolToVisibilityConverter does not support two-way binding.");
        }
    }
}
