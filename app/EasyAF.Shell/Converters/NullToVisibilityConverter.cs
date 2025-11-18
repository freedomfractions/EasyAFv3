using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EasyAF.Shell.Converters
{
    /// <summary>
    /// Converts null to Collapsed and non-null to Visible.
    /// </summary>
    /// <remarks>
    /// Used to show/hide UI elements based on whether a ViewModel is available.
    /// </remarks>
    public class NullToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts null to Collapsed, non-null to Visible.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
