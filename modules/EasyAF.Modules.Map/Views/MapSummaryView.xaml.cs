using System.Windows.Controls;

namespace EasyAF.Modules.Map.Views
{
    /// <summary>
    /// Interaction logic for MapSummaryView.xaml
    /// </summary>
    /// <remarks>
    /// This view displays:
    /// - Map metadata (name, version, description)
    /// - Referenced sample files with add/remove functionality
    /// - Data type mapping status overview with progress indicators
    /// 
    /// STRICT MVVM: No logic here except InitializeComponent().
    /// All functionality is in MapSummaryViewModel.
    /// </remarks>
    public partial class MapSummaryView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the MapSummaryView.
        /// </summary>
        public MapSummaryView()
        {
            InitializeComponent();
        }
    }
}
