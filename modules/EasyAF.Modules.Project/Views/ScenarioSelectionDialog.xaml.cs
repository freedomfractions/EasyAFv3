using System.Windows;

namespace EasyAF.Modules.Project.Views
{
    /// <summary>
    /// Interaction logic for ScenarioSelectionDialog.xaml
    /// </summary>
    public partial class ScenarioSelectionDialog : Window
    {
        public ScenarioSelectionDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate at least one scenario is selected
            if (DataContext is ViewModels.ScenarioSelectionViewModel viewModel)
            {
                if (viewModel.SelectedCount == 0)
                {
                    MessageBox.Show(
                        "Please select at least one scenario to import.",
                        "No Scenarios Selected",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
            }

            DialogResult = true;
            Close();
        }
    }
}
