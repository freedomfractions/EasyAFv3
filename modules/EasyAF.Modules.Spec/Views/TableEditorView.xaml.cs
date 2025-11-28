using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EasyAF.Modules.Spec.ViewModels;

namespace EasyAF.Modules.Spec.Views
{
    /// <summary>
    /// Interaction logic for TableEditorView.xaml
    /// </summary>
    /// <remarks>
    /// MVVM Pattern: This code-behind contains ONLY InitializeComponent().
    /// All logic is in TableEditorViewModel.
    /// </remarks>
    public partial class TableEditorView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the TableEditorView.
        /// </summary>
        public TableEditorView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles double-click on PropertyPaths cell to open the picker dialog.
        /// </summary>
        private void PropertyPaths_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Only trigger on double-click
            if (e.ClickCount != 2) return;

            // Get the ViewModel and trigger the Edit command
            if (DataContext is TableEditorViewModel viewModel && viewModel.EditColumnCommand.CanExecute(null))
            {
                viewModel.EditColumnCommand.Execute(null);
            }
        }

        /// <summary>
        /// Handles double-click on Filters DataGrid to edit the selected filter.
        /// </summary>
        private void FiltersDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Get the ViewModel and trigger the EditFilter command
            if (DataContext is TableEditorViewModel viewModel && viewModel.EditFilterCommand.CanExecute(null))
            {
                viewModel.EditFilterCommand.Execute(null);
            }
        }
    }
}
