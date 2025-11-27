using System;
using System.Globalization;
using System.Windows.Data;
using EasyAF.Shell.Extensions;

namespace EasyAF.Shell.Converters;

/// <summary>
/// Converts an IModule to its description text.
/// </summary>
public class ModuleToDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is EasyAF.Core.Contracts.IModule module)
        {
            return module.GetModuleDescription();
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
