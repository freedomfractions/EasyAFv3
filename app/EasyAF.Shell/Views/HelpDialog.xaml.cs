using System.Windows;
using System.ComponentModel;

namespace EasyAF.Shell.Views;

public partial class HelpDialog : Window
{
    public HelpDialog()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is ViewModels.HelpDialogViewModel vm)
        {
            vm.PropertyChanged += OnVmPropertyChanged;
        }
        if (e.OldValue is ViewModels.HelpDialogViewModel old)
        {
            old.PropertyChanged -= OnVmPropertyChanged;
        }
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is ViewModels.HelpDialogViewModel vm && e.PropertyName == nameof(ViewModels.HelpDialogViewModel.DialogResult))
        {
            DialogResult = vm.DialogResult;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is ViewModels.HelpDialogViewModel vm)
        {
            vm.PropertyChanged -= OnVmPropertyChanged;
        }
        base.OnClosed(e);
    }
}
