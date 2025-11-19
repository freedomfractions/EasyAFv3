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
            return "\uE711"; // QuestionMark glyph as fallback
        
        return status switch
        {
            MappingStatus.Unmapped => "\uE711",  // CircleRing (empty circle)
            MappingStatus.Partial => "\uE73C",   // ProgressRingDots (partial indicator)
            MappingStatus.Complete => "\uE73E",  // CheckMark (complete)
            _ => "\uE711"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
