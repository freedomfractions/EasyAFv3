using System.Windows;
using System.ComponentModel;

namespace EasyAF.Shell.Views;

public partial class AboutDialog : Window
{
    public AboutDialog()
    {
        InitializeComponent();
        
        // Wire up DialogResult when DataContext is set
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is ViewModels.AboutDialogViewModel vm)
        {
            vm.PropertyChanged += OnViewModelPropertyChanged;
        }
        
        if (e.OldValue is ViewModels.AboutDialogViewModel oldVm)
        {
            oldVm.PropertyChanged -= OnViewModelPropertyChanged;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModels.AboutDialogViewModel.DialogResult) &&
            sender is ViewModels.AboutDialogViewModel vm)
        {
            DialogResult = vm.DialogResult;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        // Clean up event handlers
        if (DataContext is ViewModels.AboutDialogViewModel vm)
        {
            vm.PropertyChanged -= OnViewModelPropertyChanged;
        }
        base.OnClosed(e);
    }
}
