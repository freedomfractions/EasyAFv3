using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EasyAF.Shell.Converters;

/// <summary>
/// Multi-value converter that returns Visible if ANY input boolean is true.
/// </summary>
/// <remarks>
/// Used for showing close button when file tab is hovered OR active.
/// </remarks>
public class BooleanOrToVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        foreach (var value in values)
        {
            if (value is bool boolValue && boolValue)
                return Visibility.Visible;
        }
        
        return Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
