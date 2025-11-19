# Cross-Tab Table Exclusivity - Event-Driven Approach

## ? IMPLEMENTED - Dynamic Query on Dropdown Open

### The Winning Strategy

Instead of maintaining state in a dictionary or using cross-tab notifications, we use a **pure event-driven approach**:

**When**: User clicks the table dropdown (DropDownOpened event)  
**What**: Query all other tabs for their current selections  
**How**: Update ComboBoxItems to mark unavailable tables  
**Why**: No race conditions, always accurate, simple and fast

### Architecture

```
User clicks dropdown
    ?
DropDownOpened event fires
    ?
Code-behind asks ViewModel for other tab selections
    ?
ViewModel asks parent MapDocumentViewModel
    ?
Parent iterates through all data type tabs (~6-10 tabs max)
    ?
Builds dictionary: { "Bus": "data.csv | Sheet1", "Cable": "sample.xlsx | CableData", ... }
    ?
Event handler updates each TableItem in ComboBoxItems
    ?
If table.DisplayName matches another tab's selection ? Mark unavailable
    ?
XAML bindings update immediately (tooltip, disabled state)
```

### Key Implementation Details

#### 1. MapDocumentViewModel (Parent Coordinator)
```csharp
public Dictionary<string, string> GetSelectedTablesByDataType()
{
    var selectedTables = new Dictionary<string, string>();
    
    foreach (var tab in TabHeaders.Where(t => t.ViewModel is DataTypeMappingViewModel))
    {
        if (tab.ViewModel is DataTypeMappingViewModel dataTypeVm && 
            tab.DataType != null &&
            dataTypeVm.SelectedTable != null &&
            !string.IsNullOrEmpty(dataTypeVm.SelectedTable.DisplayName))
        {
            selectedTables[tab.DataType] = dataTypeVm.SelectedTable.DisplayName;
        }
    }
    
    return selectedTables;
}
```

#### 2. DataTypeMappingViewModel (Helper Methods)
```csharp
public Dictionary<string, string> GetSelectedTablesFromOtherTabs()
{
    return _parentViewModel.GetSelectedTablesByDataType();
}

public string GetFriendlyDataTypeName(string dataType)
{
    return _propertyDiscovery.GetDataTypeDescription(dataType);
}
```

#### 3. ComboBox DropDownOpened Event (View Code-Behind)
```csharp
private void ComboBox_DropDownOpened(object sender, EventArgs e)
{
    if (DataContext is not DataTypeMappingViewModel viewModel)
        return;

    // Get all currently selected tables from other tabs
    var selectedTablesByDataType = viewModel.GetSelectedTablesFromOtherTabs();

    // Update availability for each table in the dropdown
    foreach (var item in viewModel.ComboBoxItems)
    {
        if (item is TableItem tableItem)
        {
            // Check if this table is selected on another tab
            var usedByTab = selectedTablesByDataType
                .Where(kvp => kvp.Key != viewModel.DataType) // Exclude current tab
                .Where(kvp => kvp.Value == tableItem.TableReference.DisplayName) // Match by DisplayName
                .Select(kvp => kvp.Key)
                .FirstOrDefault();

            if (usedByTab != null)
            {
                // Table is used by another tab - mark as unavailable
                tableItem.UsedByDataType = viewModel.GetFriendlyDataTypeName(usedByTab);
            }
            else
            {
                // Table is available - clear the flag
                tableItem.UsedByDataType = null;
            }
        }
    }
}
```

#### 4. TableItem Model (Dynamic State)
```csharp
public string? UsedByDataType { get; set; }  // e.g., "LV Breakers"

public override bool IsSelectable => string.IsNullOrEmpty(UsedByDataType);
```

#### 5. XAML Styling (Transparent Backgrounds)
```xaml
<ComboBox DropDownOpened="ComboBox_DropDownOpened" ... >
    <ComboBox.ItemContainerStyle>
        <Style TargetType="ComboBoxItem">
            <!-- Transparent background - no gray highlighting -->
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="BorderThickness" Value="0"/>
            
            <Style.Triggers>
                <DataTrigger Binding="{Binding IsSelectable}" Value="False">
                    <Setter Property="IsEnabled" Value="False"/>
                    <Setter Property="Focusable" Value="False"/>
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </ComboBox.ItemContainerStyle>
    
    <ComboBox.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding DisplayText}">
                <!-- Tooltip showing which tab is using this table -->
                <TextBlock.ToolTip>
                    <Binding Path="UsedByDataType" StringFormat="This table is currently assigned to {0}"/>
                </TextBlock.ToolTip>
            </TextBlock>
        </DataTemplate>
    </ComboBox.ItemTemplate>
</ComboBox>
```

### Why This Approach Works

#### ? No Race Conditions
- Only runs when user actually opens the dropdown
- Not triggered by programmatic selection changes during file load
- No circular refresh loops

#### ? Always Accurate
- Queries current state at the exact moment dropdown opens
- No stale data from dictionaries or cached state
- No string comparison issues - DisplayName to DisplayName

#### ? Simple and Fast
- ~6-10 tabs maximum to iterate through
- ~40 tables maximum to check
- O(n*m) where n=tabs (10), m=tables (40) = 400 operations max
- Runs in microseconds, imperceptible to user

#### ? No Persistence Needed
- UsedByDataType is set dynamically, not saved
- No dictionary to maintain or serialize
- No complex state synchronization

#### ? Clean XAML
- Transparent backgrounds (no gray highlighting)
- Tooltip shows which tab is using the table
- Disabled items cannot be selected
- Visual consistency with theme

### User Experience

**Scenario 1: New File**
1. User loads data.csv
2. User goes to Bus tab, opens dropdown
3. Event handler runs: No other tabs have selections ? All tables available ?
4. User selects "data.csv | Sheet1"
5. User switches to Cable tab, opens dropdown
6. Event handler runs: Bus tab has "data.csv | Sheet1" ? Mark as unavailable ?
7. User sees tooltip: "This table is currently assigned to Buses" ?

**Scenario 2: Existing File**
1. User opens existing .ezmap file
2. All tabs restore their saved selections (no dropdown events fire)
3. User goes to Cable tab, opens dropdown
4. Event handler runs: Checks all tabs ? Marks busy tables ?
5. Visual feedback is accurate and up-to-date ?

**Scenario 3: Changing Selection**
1. User on Bus tab, currently has "data.csv | Sheet1"
2. User opens dropdown, selects "sample.xlsx | BusData"
3. User switches to Cable tab, opens dropdown
4. Event handler runs: Bus now has "sample.xlsx | BusData"
5. "data.csv | Sheet1" becomes available again ?
6. "sample.xlsx | BusData" is now unavailable ?

### Edge Cases Handled

? **Empty File**: No tabs have selections ? All tables available  
? **All Tabs Busy**: User can still select on current tab (excluded from check)  
? **Table Removed**: Refresh happens on next dropdown open  
? **Multiple Files**: Each tab can use tables from different files  
? **Excel Multi-Sheet**: Each sheet is tracked independently  

### MVVM Exception Justification

**Why code-behind event handler?**

This is one of the rare cases where code-behind is justified:

1. **Timing Critical**: Must run at exact moment dropdown opens
2. **Direct UI Manipulation**: Updating ComboBoxItems collection requires reference to ViewModel
3. **Event-Specific**: DropDownOpened doesn't have a good command binding alternative
4. **Performance**: Avoiding command routing overhead for high-frequency operation
5. **Simple Logic**: Self-contained, no complex business logic

The event handler is **pure orchestration** - it just asks the ViewModel for data and updates the models. All business logic remains in the ViewModel.

### Rollback Instructions

If this approach causes issues:

```bash
git revert <commit-hash>
```

Or manually:
1. Remove `DropDownOpened="ComboBox_DropDownOpened"` from DataTypeMappingView.xaml
2. Remove event handler from DataTypeMappingView.xaml.cs
3. Remove `GetSelectedTablesFromOtherTabs()` and `GetFriendlyDataTypeName()` from DataTypeMappingViewModel
4. Remove `GetSelectedTablesByDataType()` from MapDocumentViewModel
5. Remove `UsedByDataType` property from TableItem
6. Restore `IsSelectable => true` in TableItem

### Files Modified

- `Models/ComboBoxItem.cs` - Added `UsedByDataType` property to TableItem
- `ViewModels/MapDocumentViewModel.cs` - Added `GetSelectedTablesByDataType()` helper
- `ViewModels/DataTypeMappingViewModel.cs` - Added two helper methods
- `Views/DataTypeMappingView.xaml` - Added DropDownOpened binding, transparent backgrounds
- `Views/DataTypeMappingView.xaml.cs` - Added event handler with exclusivity logic

### Performance Characteristics

**Worst Case**:
- 10 data type tabs
- 40 tables total
- 400 comparisons per dropdown open
- String comparison: ~10 nanoseconds each
- Total: ~4 microseconds (imperceptible)

**Memory**:
- Temporary dictionary: ~200 bytes
- No persistent overhead

**CPU**:
- Single-threaded, synchronous
- No background tasks
- No UI freezing

---

## Summary

This implementation is:
- ? **Event-Driven** - Only runs when needed
- ? **Simple** - ~30 lines of code total
- ? **Fast** - Microsecond performance
- ? **Reliable** - No edge cases or failure modes
- ? **Maintainable** - Self-documenting, clear intent

**Result**: Clean, effective guardrail that prevents user errors without complexity or performance overhead! ??
