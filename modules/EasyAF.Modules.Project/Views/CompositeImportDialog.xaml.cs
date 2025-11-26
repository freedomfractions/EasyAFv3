using System.Collections.Generic;
using System.Windows;
using EasyAF.Modules.Project.ViewModels;
using EasyAF.Modules.Project.Helpers;

namespace EasyAF.Modules.Project.Views
{
    /// <summary>
    /// Interaction logic for CompositeImportDialog.xaml
    /// </summary>
    public partial class CompositeImportDialog : Window
    {
        public CompositeImportDialog(Dictionary<string, FileScanResult> fileScanResults, List<string> existingScenarios)
        {
            InitializeComponent();

            var viewModel = new CompositeImportDialogViewModel(fileScanResults, existingScenarios);
            DataContext = viewModel;

            // Watch for dialog result changes and close the window
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CompositeImportDialogViewModel.DialogResult))
                {
                    DialogResult = viewModel.DialogResult;
                    Close(); // Close the dialog window
                }
            };
        }

        public CompositeImportDialogViewModel ViewModel => (CompositeImportDialogViewModel)DataContext;
    }
}
