using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EasyAF.Shell.Converters;

/// <summary>
/// Converts IsPinned boolean to star foreground color (accent vs secondary).
/// </summary>
public class PinnedToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isPinned && isPinned)
        {
            // Return accent brush for pinned stars
            return System.Windows.Application.Current.TryFindResource("AccentBrush") as Brush 
                   ?? Brushes.DodgerBlue;
        }
        // Return secondary text brush for unpinned stars
        return System.Windows.Application.Current.TryFindResource("TextSecondaryBrush") as Brush 
               ?? Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
