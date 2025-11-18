using System.Windows;
using EasyAF.Modules.Map.ViewModels;

namespace EasyAF.Modules.Map.Views
{
    /// <summary>
    /// Interaction logic for PropertySelectorDialog.xaml
    /// </summary>
    /// <remarks>
    /// Dialog for selecting which properties of a data type should be enabled for mapping.
    /// </remarks>
    public partial class PropertySelectorDialog : Window
    {
        /// <summary>
        /// Initializes a new instance of the PropertySelectorDialog.
        /// </summary>
        public PropertySelectorDialog()
        {
            InitializeComponent();
            
            // Wire up DialogResult from ViewModel
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Unsubscribe from old ViewModel
            if (e.OldValue is PropertySelectorViewModel oldVm)
            {
                oldVm.PropertyChanged -= OnViewModelPropertyChanged;
            }

            // Subscribe to new ViewModel
            if (e.NewValue is PropertySelectorViewModel newVm)
            {
                newVm.PropertyChanged += OnViewModelPropertyChanged;
            }
        }

        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PropertySelectorViewModel.DialogResult))
            {
                if (sender is PropertySelectorViewModel vm && vm.DialogResult.HasValue)
                {
                    DialogResult = vm.DialogResult;
                }
            }
        }
    }
}
