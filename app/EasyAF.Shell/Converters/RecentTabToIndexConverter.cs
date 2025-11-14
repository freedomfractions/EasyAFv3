using System;
using System.Globalization;
using System.Windows.Data;
using EasyAF.Shell.ViewModels.Backstage;

namespace EasyAF.Shell.Converters;

/// <summary>
/// Converts RecentTab enum to TabControl SelectedIndex (0=Files, 1=Folders).
/// </summary>
public class RecentTabToIndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is RecentTab tab)
        {
            return (int)tab;
        }
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int index)
        {
            return (RecentTab)index;
        }
        return RecentTab.Files;
    }
}
