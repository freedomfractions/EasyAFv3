using System.Linq;
using System.Windows.Controls;
using EasyAF.Modules.Map.Models;
using EasyAF.Modules.Map.ViewModels;

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
    /// - Table selection dropdown with cross-tab exclusivity
    /// - Real-time count display
    /// 
    /// MVVM Exception: ComboBox_DropDownOpened event handler updates table availability.
    /// This is done in code-behind because it requires direct manipulation of ComboBoxItems
    /// collection at the moment the dropdown opens (timing is critical to avoid race conditions).
    /// </remarks>
    public partial class DataTypeMappingView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the DataTypeMappingView.
        /// </summary>
        public DataTypeMappingView()
        {
            InitializeComponent();
            Serilog.Log.Information("DataTypeMappingView constructor called - new instance created");
        }

        /// <summary>
        /// Handles the ComboBox Loaded event to attach the DropDownOpened handler.
        /// </summary>
        /// <remarks>
        /// CROSS-MODULE EDIT: 2025-01-18 Cross-Tab Table Exclusivity (Event Attachment Fix)
        /// Modified for: Attach DropDownOpened handler to each dynamically created ComboBox instance
        /// Related modules: Map (MapDocumentView DataTemplate system)
        /// Rollback instructions: Remove this method and use direct DropDownOpened binding
        /// 
        /// This fixes the issue where only the first data type tab worked correctly.
        /// The problem was that DataTemplates create new view instances, but event handlers
        /// in XAML don't automatically attach to runtime-created instances.
        /// 
        /// Solution: Use Loaded event to programmatically attach DropDownOpened handler
        /// to each ComboBox instance as it's created by the DataTemplate.
        /// </remarks>
        private void ComboBox_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Serilog.Log.Warning("ComboBox_Loaded event FIRED! Sender type: {Type}", sender?.GetType().Name);
            
            if (sender is ComboBox comboBox)
            {
                var dataType = (DataContext as DataTypeMappingViewModel)?.DataType ?? "unknown";
                
                Serilog.Log.Warning("ComboBox_Loaded: Attaching handler for {DataType} tab", dataType);
                
                // Detach first to avoid duplicate subscriptions (in case Loaded fires multiple times)
                comboBox.DropDownOpened -= ComboBox_DropDownOpened;
                
                // Attach the handler to this specific ComboBox instance
                comboBox.DropDownOpened += ComboBox_DropDownOpened;
                
                Serilog.Log.Warning("ComboBox_Loaded: Handler attached successfully for {DataType}", dataType);
            }
            else
            {
                Serilog.Log.Error("ComboBox_Loaded: Sender is NOT a ComboBox! Type: {Type}", sender?.GetType().Name);
            }
        }

        /// <summary>
        /// Handles the ComboBox DropDownOpened event to refresh table availability.
        /// </summary>
        /// <remarks>
        /// CROSS-MODULE EDIT: 2025-01-18 Cross-Tab Table Exclusivity (Event-Driven)
        /// Modified for: Implement dynamic table exclusivity on dropdown open
        /// Related modules: Map (MapDocumentViewModel, DataTypeMappingViewModel, TableItem)
        /// Rollback instructions: Remove this event handler and the DropDownOpened binding
        /// 
        /// This approach solves the race condition problem by:
        /// 1. Only running when user actually opens the dropdown (not on every selection change)
        /// 2. Querying current state of all tabs at that moment (always accurate)
        /// 3. No persistence needed - pure dynamic query
        /// 4. No string comparison issues - compares DisplayName to DisplayName
        /// 
        /// Algorithm:
        /// 1. Get the ViewModel (DataTypeMappingViewModel)
        /// 2. Ask parent (MapDocumentViewModel) for all selected tables
        /// 3. For each TableItem in ComboBoxItems:
        ///    - If table is selected on a DIFFERENT tab, mark as unavailable
        ///    - Otherwise, clear the unavailable flag
        /// </remarks>
        private void ComboBox_DropDownOpened(object sender, System.EventArgs e)
        {
            // Get the ViewModel from DataContext
            if (DataContext is not DataTypeMappingViewModel viewModel)
                return;

            // Get all currently selected tables from other tabs
            var selectedTablesByDataType = viewModel.GetSelectedTablesFromOtherTabs();

            Serilog.Log.Debug("DropDown opened for {DataType}. Other tabs using tables: {Count}", 
                viewModel.DataType, selectedTablesByDataType.Count);
            
            foreach (var kvp in selectedTablesByDataType)
            {
                Serilog.Log.Debug("  {Tab} is using: '{Table}'", kvp.Key, kvp.Value);
            }

            // Update availability for each table in the dropdown
            foreach (var item in viewModel.ComboBoxItems)
            {
                if (item is TableItem tableItem)
                {
                    var displayName = tableItem.TableReference.DisplayName;
                    
                    // Check if this table is selected on another tab
                    var usedByTab = selectedTablesByDataType
                        .Where(kvp => kvp.Key != viewModel.DataType) // Exclude current tab
                        .Where(kvp => kvp.Value == displayName) // Match by DisplayName
                        .Select(kvp => kvp.Key)
                        .FirstOrDefault();

                    if (usedByTab != null)
                    {
                        // Table is used by another tab - mark as unavailable
                        var friendlyName = viewModel.GetFriendlyDataTypeName(usedByTab);
                        tableItem.UsedByDataType = friendlyName;
                        Serilog.Log.Information("Table '{Table}' marked as UNAVAILABLE on {CurrentTab} (used by {OtherTab})", 
                            displayName, viewModel.DataType, usedByTab);
                    }
                    else
                    {
                        // Table is available - clear the flag
                        tableItem.UsedByDataType = null;
                    }
                }
            }
        }
    }
}
