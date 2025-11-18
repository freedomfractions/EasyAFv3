using System.Windows.Controls;

namespace EasyAF.Modules.Map.Views
{
    /// <summary>
    /// Interaction logic for DataTypeMappingView.xaml
    /// </summary>
    /// <remarks>
    /// This view provides a dual-pane interface for mapping source columns to target properties:
    /// - Left pane: Available columns from sample files (filterable)
    /// - Center: Map/Unmap buttons
    /// - Right pane: Target properties for the data type (filterable, with descriptions)
    /// 
    /// Features:
    /// - Visual feedback for mapped items (green highlight)
    /// - Search/filter on both panes
    /// - Toolbar with Auto-Map, Clear All, and Load File commands
    /// - Table selection dropdown
    /// - Real-time count display
    /// 
    /// STRICT MVVM: No logic here except InitializeComponent().
    /// All functionality is in DataTypeMappingViewModel.
    /// </remarks>
    public partial class DataTypeMappingView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the DataTypeMappingView.
        /// </summary>
        public DataTypeMappingView()
        {
            InitializeComponent();
        }
    }
}
