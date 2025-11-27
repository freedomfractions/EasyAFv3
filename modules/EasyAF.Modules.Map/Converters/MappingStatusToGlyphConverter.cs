using System;
using System.Globalization;
using System.Windows.Data;
using EasyAF.Modules.Map.Models;

namespace EasyAF.Modules.Map.Converters;

/// <summary>
/// Converts MappingStatus enum to Segoe MDL2 Assets glyph code.
/// </summary>
public class MappingStatusToGlyphConverter : IValueConverter
{
    /// <summary>
    /// Converts MappingStatus to Unicode glyph.
    /// </summary>
    /// <param name="value">MappingStatus enum value</param>
    /// <returns>Unicode glyph string for Segoe MDL2 Assets font</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not MappingStatus status)
            return "\uE711"; // CircleRing glyph as fallback
        
        return status switch
        {
            MappingStatus.Unmapped => "\uE91F",  // StatusCircleRing - hollow circle
            MappingStatus.Partial => "\uE9F5",   // ProgressRing - animated/partial circle
            MappingStatus.Complete => "\uE930", // CompletedSolid - filled circle with checkmark
            _ => "\uE711"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
