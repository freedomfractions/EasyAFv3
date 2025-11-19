using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;
using EasyAF.Modules.Map.Models;

namespace EasyAF.Modules.Map.Converters;

/// <summary>
/// Converts MappingStatus enum to appropriate theme brush.
/// Looks up the brush from Application.Current.Resources.
/// </summary>
public class MappingStatusToBrushConverter : IValueConverter
{
    /// <summary>
    /// Converts MappingStatus to theme brush.
    /// </summary>
    /// <param name="value">MappingStatus enum value</param>
    /// <returns>SolidColorBrush from theme resources</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not MappingStatus status)
            return Application.Current.TryFindResource("TextSecondaryBrush") ?? Brushes.Gray;
        
        var resourceKey = status switch
        {
            MappingStatus.Unmapped => "ErrorBrush",      // Red for unmapped
            MappingStatus.Partial => "WarningBrush",     // Orange/Yellow for partial
            MappingStatus.Complete => "SuccessBrush",    // Green for complete
            _ => "TextSecondaryBrush"
        };

        return Application.Current.TryFindResource(resourceKey) ?? Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
