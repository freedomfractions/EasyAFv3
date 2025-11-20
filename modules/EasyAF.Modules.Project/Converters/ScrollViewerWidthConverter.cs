using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace EasyAF.Modules.Project.Converters
{
    /// <summary>
    /// Converts ScrollViewer's ViewportWidth to usable width by subtracting scrollbar width when visible.
    /// This prevents content from being clipped by the vertical scrollbar.
    /// </summary>
    public class ScrollViewerWidthConverter : IValueConverter
    {
        /// <summary>
        /// Default scrollbar width in WPF (SystemParameters.VerticalScrollBarWidth is typically 17-18px)
        /// Using a safe default of 20 to account for any theme variations
        /// </summary>
        private const double ScrollBarWidth = 20.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double viewportWidth)
            {
                // Subtract scrollbar width to prevent content from being clipped
                return viewportWidth - ScrollBarWidth;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
