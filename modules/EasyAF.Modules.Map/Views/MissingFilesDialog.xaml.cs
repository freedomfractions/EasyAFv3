using System.ComponentModel;
using System.Windows;

namespace EasyAF.Modules.Map.Views
{
    /// <summary>
    /// Interaction logic for MissingFilesDialog.xaml
    /// </summary>
    public partial class MissingFilesDialog : Window
    {
        public MissingFilesDialog()
        {
            InitializeComponent();

            // Wire up DialogResult when DataContext is set
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is ViewModels.MissingFilesDialogViewModel vm)
            {
                vm.PropertyChanged += OnViewModelPropertyChanged;
            }

            if (e.OldValue is ViewModels.MissingFilesDialogViewModel oldVm)
            {
                oldVm.PropertyChanged -= OnViewModelPropertyChanged;
            }
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModels.MissingFilesDialogViewModel.DialogResult) &&
                sender is ViewModels.MissingFilesDialogViewModel vm)
            {
                DialogResult = vm.DialogResult;
            }
        }

        protected override void OnClosed(System.EventArgs e)
        {
            // Clean up event handlers
            if (DataContext is ViewModels.MissingFilesDialogViewModel vm)
            {
                vm.PropertyChanged -= OnViewModelPropertyChanged;
            }
            base.OnClosed(e);
        }
    }
}
