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
/// Returns Collapsed if all inputs are explicitly false or if no inputs are true.
/// </remarks>
public class BooleanOrToVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length == 0)
            return Visibility.Collapsed;
        
        foreach (var value in values)
        {
            // Only check for explicit true values
            if (value is bool boolValue && boolValue)
                return Visibility.Visible;
        }
        
        // Default to Collapsed if no true values found
        return Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
