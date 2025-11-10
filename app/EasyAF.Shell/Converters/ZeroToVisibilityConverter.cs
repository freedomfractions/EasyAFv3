using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EasyAF.Shell.Converters
{
    /// <summary>
    /// Converts an integer (e.g. collection count) of 0 to Visible (for placeholder) otherwise Collapsed.
    /// </summary>
    public class ZeroToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Visibility.Visible;
            try
            {
                var num = System.Convert.ToInt32(value);
                return num == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
