using System.Windows;

namespace EasyAF.Modules.Project.Views
{
    /// <summary>
    /// Interaction logic for ImportDataDialog.xaml
    /// </summary>
    public partial class ImportDataDialog : Window
    {
        public ImportDataDialog()
        {
            InitializeComponent();
        }

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.ImportDataViewModel viewModel)
            {
                var success = await viewModel.ExecuteImportAsync();
                
                if (success)
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    // Error message already displayed in status
                    // User can retry or cancel
                }
            }
        }
    }
}
