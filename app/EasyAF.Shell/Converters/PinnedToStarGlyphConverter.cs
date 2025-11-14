using System;
using System.Globalization;
using System.Windows.Data;

namespace EasyAF.Shell.Converters;

/// <summary>
/// Converts IsPinned boolean to star glyph (filled vs outline).
/// </summary>
public class PinnedToStarGlyphConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isPinned)
        {
            return isPinned ? "\uE735" : "\uE734"; // Filled star : Outline star
        }
        return "\uE734"; // Outline star default
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
