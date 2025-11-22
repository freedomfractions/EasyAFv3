using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EasyAF.Modules.Project.Converters
{
    /// <summary>
    /// Converts boolean to Visibility with inverse logic (true = Collapsed, false = Visible).
    /// </summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility != Visibility.Visible;
            }
            return false;
        }
    }

    /// <summary>
    /// Converts boolean to FontWeight (true = Bold, false = Normal).
    /// </summary>
    public class BooleanToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
            {
                return FontWeights.SemiBold;
            }
            return FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Compares a numeric value against zero using the specified operator.
    /// Parameter: '>' for greater than, '<' for less than
    /// </summary>
    public class ComparisonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue && parameter is string op)
            {
                return op switch
                {
                    ">" => intValue > 0,
                    "<" => intValue < 0,
                    "=" => intValue == 0,
                    _ => false
                };
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
