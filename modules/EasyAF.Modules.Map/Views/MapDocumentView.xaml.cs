using System.Windows.Controls;

namespace EasyAF.Modules.Map.Views
{
    /// <summary>
    /// Interaction logic for MapDocumentView.xaml
    /// </summary>
    /// <remarks>
    /// This is the main view for a map document, containing a TabControl that hosts:
    /// - Summary tab (MapSummaryView)
    /// - Data type tabs (DataTypeMappingView for each discovered type)
    /// 
    /// STRICT MVVM: No logic here except InitializeComponent().
    /// All functionality is in MapDocumentViewModel.
    /// </remarks>
    public partial class MapDocumentView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the MapDocumentView.
        /// </summary>
        public MapDocumentView()
        {
            InitializeComponent();
        }
    }
}
