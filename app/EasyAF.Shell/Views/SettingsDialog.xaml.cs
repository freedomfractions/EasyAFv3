using System.Windows;

namespace EasyAF.Shell.Views;

/// <summary>
/// Interaction logic for SettingsDialog.xaml
/// </summary>
public partial class SettingsDialog : Window
{
    public SettingsDialog()
    {
        InitializeComponent();

        // Close the dialog when the ViewModel signals DialogResult
        DataContextChanged += (_, __) => HookViewModel();
        Loaded += (_, __) => HookViewModel();
    }

    private void HookViewModel()
    {
        if (DataContext is ViewModels.SettingsDialogViewModel vm)
        {
            vm.PropertyChanged -= VmOnPropertyChanged;
            vm.PropertyChanged += VmOnPropertyChanged;
        }
    }

    private void VmOnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModels.SettingsDialogViewModel.DialogResult)
            && DataContext is ViewModels.SettingsDialogViewModel vm)
        {
            if (vm.DialogResult.HasValue)
            {
                DialogResult = vm.DialogResult;
                Close();
            }
        }
    }
}
