using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace EasyAF.Modules.Spec.Converters
{
    /// <summary>
    /// Multi-value converter that converts empty message formatting properties to WPF styling attributes.
    /// Used to provide live preview of formatting in the empty message TextBox.
    /// 
    /// Default formatting (when no formatting specified):
    /// - Font: Arial 11pt Regular
    /// - Colors: Black text on White background
    /// - Alignment: Middle Center
    /// </summary>
    public class EmptyMessageFontFamilyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string fontName && !string.IsNullOrWhiteSpace(fontName))
            {
                try
                {
                    return new FontFamily(fontName);
                }
                catch
                {
                    // Invalid font name, return default
                }
            }
            // Default: Arial (Word-like default for reports)
            return new FontFamily("Arial");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EmptyMessageFontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string sizeStr && double.TryParse(sizeStr, out var size) && size > 0)
            {
                return size;
            }
            // Default: 11pt (standard document size)
            return 11.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EmptyMessageFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool bold && bold)
            {
                return FontWeights.Bold;
            }
            // Default: Regular (Normal)
            return FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EmptyMessageFontStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool italic && italic)
            {
                return FontStyles.Italic;
            }
            // Default: Normal (not italic)
            return FontStyles.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EmptyMessageTextDecorationConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool underline && underline)
            {
                return TextDecorations.Underline;
            }
            // Default: No underline
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EmptyMessageBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hex && !string.IsNullOrWhiteSpace(hex))
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString("#" + hex.TrimStart('#'));
                    return new SolidColorBrush(color);
                }
                catch
                {
                    // Invalid color, fall through to default
                }
            }
            // Default: White (plain document background)
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EmptyMessageForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hex && !string.IsNullOrWhiteSpace(hex))
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString("#" + hex.TrimStart('#'));
                    return new SolidColorBrush(color);
                }
                catch
                {
                    // Invalid color, fall through to default
                }
            }
            // Default: Black (standard document text)
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EmptyMessageHorizontalAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string align && !string.IsNullOrWhiteSpace(align))
            {
                return align.ToLowerInvariant() switch
                {
                    "left" => TextAlignment.Left,
                    "center" => TextAlignment.Center,
                    "right" => TextAlignment.Right,
                    _ => TextAlignment.Center
                };
            }
            // Default: Center
            return TextAlignment.Center;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EmptyMessageVerticalAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string align && !string.IsNullOrWhiteSpace(align))
            {
                return align.ToLowerInvariant() switch
                {
                    "top" => VerticalAlignment.Top,
                    "center" => VerticalAlignment.Center,
                    "middle" => VerticalAlignment.Center,
                    "bottom" => VerticalAlignment.Bottom,
                    _ => VerticalAlignment.Center
                };
            }
            // Default: Center (Middle)
            return VerticalAlignment.Center;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
