using System.Windows;
using EasyAF.Modules.Spec.ViewModels.Dialogs;

namespace EasyAF.Modules.Spec.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for PropertyPathPickerDialog.xaml
    /// </summary>
    public partial class PropertyPathPickerDialog : Window
    {
        public PropertyPathPickerDialog()
        {
            InitializeComponent();
            
            // Subscribe to ViewModel DialogResult changes
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is PropertyPathPickerViewModel viewModel)
            {
                viewModel.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(PropertyPathPickerViewModel.DialogResult))
                    {
                        DialogResult = viewModel.DialogResult;
                    }
                };
            }
        }
    }
}
