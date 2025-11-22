using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EasyAF.Shell.Converters;

/// <summary>
/// Converts a boolean value to Visibility, inverting the logic.
/// True ? Collapsed, False ? Visible.
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
