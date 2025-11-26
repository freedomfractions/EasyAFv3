using System.Windows.Controls;

namespace EasyAF.Modules.Project.Views.Panels
{
    /// <summary>
    /// Interaction logic for ImportHistoryPanel.xaml
    /// </summary>
    /// <remarks>
    /// This panel displays the import history in a hierarchical tree structure:
    /// - Parent nodes: Import sessions (grouped by timestamp)
    /// - Child nodes: Individual files imported in that session
    /// 
    /// Displays separate trees for New Data and Old Data imports.
    /// 
    /// MVVM Pattern: Code-behind is minimal. All logic is in ProjectSummaryViewModel.
    /// </remarks>
    public partial class ImportHistoryPanel : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the ImportHistoryPanel.
        /// </summary>
        public ImportHistoryPanel()
        {
            InitializeComponent();
        }
    }
}
