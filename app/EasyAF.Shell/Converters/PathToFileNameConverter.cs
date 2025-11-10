using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace EasyAF.Shell.Converters;

/// <summary>
/// Converts a full file path to just the filename for display.
/// </summary>
public class PathToFileNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string path && !string.IsNullOrWhiteSpace(path))
        {
            try
            {
                return Path.GetFileName(path);
            }
            catch
            {
                return path;
            }
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
