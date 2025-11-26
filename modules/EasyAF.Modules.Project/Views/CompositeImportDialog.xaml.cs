using System.Collections.Generic;
using System.Windows;
using EasyAF.Modules.Project.ViewModels;

namespace EasyAF.Modules.Project.Views
{
    /// <summary>
    /// Interaction logic for CompositeImportDialog.xaml
    /// </summary>
    public partial class CompositeImportDialog : Window
    {
        public CompositeImportDialog(Dictionary<string, List<string>> fileScenarios, List<string> existingScenarios)
        {
            InitializeComponent();

            var viewModel = new CompositeImportDialogViewModel(fileScenarios, existingScenarios);
            DataContext = viewModel;

            // Watch for dialog result changes
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CompositeImportDialogViewModel.DialogResult))
                {
                    DialogResult = viewModel.DialogResult;
                }
            };
        }

        public CompositeImportDialogViewModel ViewModel => (CompositeImportDialogViewModel)DataContext;
    }
}
