using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EasyAF.Shell.Converters;

/// <summary>
/// Returns Visible when the bound numeric value is greater than zero; otherwise Collapsed.
/// </summary>
public class NonZeroToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return Visibility.Collapsed;
        try
        {
            var num = System.Convert.ToInt32(value, CultureInfo.InvariantCulture);
            return num > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch { return Visibility.Collapsed; }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}