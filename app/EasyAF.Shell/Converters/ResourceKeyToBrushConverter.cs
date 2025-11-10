using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace EasyAF.Shell.Converters;

/// <summary>
/// Converts a resource key string to the actual resource brush from the application resources.
/// </summary>
public class ResourceKeyToBrushConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string resourceKey && !string.IsNullOrWhiteSpace(resourceKey))
        {
            try
            {
                return Application.Current.TryFindResource(resourceKey) as Brush;
            }
            catch
            {
                // If resource not found, return a fallback brush
                return Application.Current.TryFindResource("TextPrimaryBrush") as Brush;
            }
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
