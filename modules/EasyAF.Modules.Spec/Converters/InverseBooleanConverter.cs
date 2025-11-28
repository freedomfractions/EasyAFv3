using System;
using System.Globalization;
using System.Windows.Data;

namespace EasyAF.Modules.Spec.Converters
{
    /// <summary>
    /// Converts a boolean value to its inverse.
    /// True ? False, False ? True.
    /// </summary>
    /// <remarks>
    /// Used for radio button binding where one button should be checked when a property is false.
    /// </remarks>
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }
    }
}
