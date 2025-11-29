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
            // Return default font
            return new FontFamily("Segoe UI");
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
            // Return default font size
            return 12.0;
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
            // Return transparent (use default control background)
            return Brushes.Transparent;
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
            // Return unset so it uses the default theme color
            return DependencyProperty.UnsetValue;
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
                    _ => TextAlignment.Left
                };
            }
            return TextAlignment.Left;
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
            return VerticalAlignment.Center;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
