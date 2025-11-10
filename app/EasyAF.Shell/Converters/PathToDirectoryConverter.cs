using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace EasyAF.Shell.Converters;

/// <summary>
/// Converts a full file path to just the directory path for display.
/// </summary>
public class PathToDirectoryConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string path && !string.IsNullOrWhiteSpace(path))
        {
            try
            {
                return Path.GetDirectoryName(path) ?? path;
            }
            catch
            {
                return string.Empty;
            }
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
