using System.Windows;
using EasyAF.Modules.Spec.ViewModels.Dialogs;

namespace EasyAF.Modules.Spec.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for PropertyPathPickerDialog.
    /// </summary>
    public partial class PropertyPathPickerDialog : Window
    {
        public PropertyPathPickerDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the view model (set externally before showing dialog).
        /// </summary>
        public PropertyPathPickerViewModel ViewModel
        {
            get => (PropertyPathPickerViewModel)DataContext;
            set
            {
                DataContext = value;
                
                // Wire up dialog result from ViewModel
                value.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(PropertyPathPickerViewModel.DialogResult))
                    {
                        DialogResult = value.DialogResult;
                    }
                };
            }
        }
    }
}
