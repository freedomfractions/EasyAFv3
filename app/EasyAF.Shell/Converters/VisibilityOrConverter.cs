using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EasyAF.Shell.Converters;

/// <summary>
/// Multi-value converter that returns Visible if ANY input is Visible.
/// </summary>
/// <remarks>
/// Used for showing Welcome screen when no documents OR Welcome tab is active.
/// Handles both Visibility and boolean inputs.
/// </remarks>
public class VisibilityOrConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length == 0)
            return Visibility.Collapsed;
        
        foreach (var value in values)
        {
            // Check for Visibility.Visible
            if (value is Visibility visibility && visibility == Visibility.Visible)
                return Visibility.Visible;
            
            // Check for explicit true boolean
            if (value is bool boolValue && boolValue)
                return Visibility.Visible;
        }
        
        // Default to Collapsed if no Visible or true values found
        return Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
