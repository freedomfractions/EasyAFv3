using System.Globalization;
using System.Windows.Data;

namespace EasyAF.Shell.Converters;

/// <summary>
/// Converts a theme name to a boolean for RadioButton IsChecked binding.
/// </summary>
public class ThemeNameToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string themeName && parameter is string selectedTheme)
        {
            return themeName.Equals(selectedTheme, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
