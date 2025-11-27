using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EasyAF.Modules.Map.Converters
{
    /// <summary>
    /// Converts a count (integer) to Visibility.
    /// Count > 0 ? Collapsed, Count == 0 ? Visible
    /// </summary>
    /// <remarks>
    /// Used to hide elements when a collection has items.
    /// For example, hiding a "No files" message when files exist.
    /// </remarks>
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                // Hide when count > 0, show when count == 0
                return count > 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            
            // If not an int, try to convert
            if (value != null && int.TryParse(value.ToString(), out int parsedCount))
            {
                return parsedCount > 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            
            return Visibility.Visible; // Default to visible if value is invalid
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("CountToVisibilityConverter does not support two-way binding.");
        }
    }
}
