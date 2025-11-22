using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EasyAF.Modules.Project.Converters
{
    /// <summary>
    /// Converts an integer delta value to a color brush.
    /// Positive = Success (green), Negative = Error (red), Zero = Secondary (gray)
    /// </summary>
    public class DeltaToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int delta)
            {
                if (delta > 0)
                    return new SolidColorBrush(Color.FromRgb(0x05, 0x96, 0x69)); // Success green
                if (delta < 0)
                    return new SolidColorBrush(Color.FromRgb(0xDC, 0x26, 0x26)); // Error red
            }
            return new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80)); // Secondary gray
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
