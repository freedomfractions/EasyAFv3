using System.Globalization;
using System.Windows.Data;
using EasyAF.Shell.Models.Backstage;

namespace EasyAF.Shell.Converters;

/// <summary>
/// Converts a QuickAccessFolder to a boolean for RadioButton IsChecked binding.
/// Compares the folder parameter with the SelectedQuickAccessFolder value.
/// </summary>
public class QuickAccessFolderToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is QuickAccessFolder selectedFolder && parameter is QuickAccessFolder currentFolder)
        {
            return selectedFolder == currentFolder;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isChecked && isChecked && parameter is QuickAccessFolder folder)
        {
            return folder;
        }
        return Binding.DoNothing;
    }
}
