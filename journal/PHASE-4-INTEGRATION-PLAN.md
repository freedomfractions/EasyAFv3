# Phase 4 Integration Plan - DiffGrid + ProjectView

**Date:** 2025-01-20  
**Status:** Planning Complete - Ready for Implementation  
**Based on:** EasyAFv2 DiffGrid + Stopgap ProjectView analysis

---

## ?? **Integration Strategy**

### **DiffGrid Control - Copy As-Is**
**Decision:** Import the entire DiffGrid control without modification

**Rationale:**
- Already implements state preservation correctly
- Uses logical positions (DPI-safe)
- Extensive test coverage proves stability
- Modular design (partial classes for each feature)

**Import Process:**
1. Copy entire `ModularUI.Controls.DiffGrid` folder
2. Add to solution as `EasyAF.Controls.DiffGrid` project
3. Reference from Project Module
4. Test in isolation before integration

---

## ?? **ProjectView Metadata - Exact Field Mapping**

### **Project Class Properties** (from lib/EasyAF.Data/Models/Project.cs)

**Existing Properties:**
- `Name` ? **Site Name** (ProjectView calls it "Site Name")
- `Client` ? **Client**
- `Location` ? **Address Line1** (first line only)
- `Date` ? **Study Date**

**Missing Properties (need to add to Project class):**
- `LBProjectNumber` (string, max 10 chars)
- `StudyEngineer` (string)
- `AddressLine2` (string)
- `AddressLine3` (string)
- `City` (string)
- `State` (string, max 2 chars, uppercase)
- `Zip` (string, max 5 digits)
- `RevisionMonth` (DateTime? or string "MM/YYYY")
- `Comments` (string, multiline)

### **Metadata GroupBox Layout** (8 rows, 4 columns)

```xaml
<GroupBox Header="Project" Margin="0,0,0,8">
    <Grid Margin="8">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto"/>      <!-- Label column 1 -->
            <ColumnDefinition Width="Auto"/>      <!-- Value column 1 -->
            <ColumnDefinition Width="Auto"/>      <!-- Label column 2 -->
            <ColumnDefinition Width="*"/>         <!-- Value column 2 -->
        </Grid.ColumnDefinitions>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>  <!-- Row 0: LB Project # | Site Name -->
            <RowDefinition Height="Auto"/>  <!-- Row 1: Client | Study Engineer -->
            <RowDefinition Height="Auto"/>  <!-- Row 2: Address Line 1 (span 3 cols) -->
            <RowDefinition Height="Auto"/>  <!-- Row 3: Address Line 2 (span 3 cols) -->
            <RowDefinition Height="Auto"/>  <!-- Row 4: Address Line 3 (span 3 cols) -->
            <RowDefinition Height="Auto"/>  <!-- Row 5: City | State | Zip -->
            <RowDefinition Height="Auto"/>  <!-- Row 6: Study Date | Revision Month -->
            <RowDefinition Height="Auto"/>  <!-- Row 7: Comments (multiline, span 3 cols) -->
        </Grid.RowDefinitions>

        <!-- Row 0 -->
        <TextBlock Grid.Row="0" Grid.Column="0" Text="L&amp;B Project Number:" 
                   VerticalAlignment="Center" Margin="0,0,8,0"/>
        <TextBox Grid.Row="0" Grid.Column="1" Width="140" MaxLength="10"
                 Text="{Binding Project.LBProjectNumber, UpdateSourceTrigger=PropertyChanged}"/>
        <TextBlock Grid.Row="0" Grid.Column="2" Text="Site Name:" 
                   VerticalAlignment="Center" Margin="16,0,8,0"/>
        <TextBox Grid.Row="0" Grid.Column="3" 
                 Text="{Binding Project.Name, UpdateSourceTrigger=PropertyChanged}"/>

        <!-- Row 1 -->
        <TextBlock Grid.Row="1" Grid.Column="0" Text="Client:" 
                   VerticalAlignment="Center" Margin="0,6,8,0"/>
        <TextBox Grid.Row="1" Grid.Column="1" Margin="0,6,0,0"
                 Text="{Binding Project.Client, UpdateSourceTrigger=PropertyChanged}"/>
        <TextBlock Grid.Row="1" Grid.Column="2" Text="Study Engineer:" 
                   VerticalAlignment="Center" Margin="16,6,8,0"/>
        <TextBox Grid.Row="1" Grid.Column="3" Margin="0,6,0,0"
                 Text="{Binding Project.StudyEngineer, UpdateSourceTrigger=PropertyChanged}"/>

        <!-- Row 2: Address Line 1 (spans columns 1-3) -->
        <TextBlock Grid.Row="2" Grid.Column="0" Text="Address Line 1:" 
                   VerticalAlignment="Center" Margin="0,6,8,0"/>
        <TextBox Grid.Row="2" Grid.Column="1" Grid.ColumnSpan="3" Margin="0,6,0,0"
                 Text="{Binding Project.Location, UpdateSourceTrigger=PropertyChanged}"/>

        <!-- Row 3: Address Line 2 -->
        <TextBlock Grid.Row="3" Grid.Column="0" Text="Address Line 2:" 
                   VerticalAlignment="Center" Margin="0,6,8,0"/>
        <TextBox Grid.Row="3" Grid.Column="1" Grid.ColumnSpan="3" Margin="0,6,0,0"
                 Text="{Binding Project.AddressLine2, UpdateSourceTrigger=PropertyChanged}"/>

        <!-- Row 4: Address Line 3 -->
        <TextBlock Grid.Row="4" Grid.Column="0" Text="Address Line 3:" 
                   VerticalAlignment="Center" Margin="0,6,8,0"/>
        <TextBox Grid.Row="4" Grid.Column="1" Grid.ColumnSpan="3" Margin="0,6,0,0"
                 Text="{Binding Project.AddressLine3, UpdateSourceTrigger=PropertyChanged}"/>

        <!-- Row 5: City | State | Zip -->
        <TextBlock Grid.Row="5" Grid.Column="0" Text="City:" 
                   VerticalAlignment="Center" Margin="0,6,8,0"/>
        <TextBox Grid.Row="5" Grid.Column="1" Margin="0,6,0,0"
                 Text="{Binding Project.City, UpdateSourceTrigger=PropertyChanged}"/>
        <TextBlock Grid.Row="5" Grid.Column="2" Text="State:" 
                   VerticalAlignment="Center" Margin="16,6,8,0"/>
        <StackPanel Grid.Row="5" Grid.Column="3" Orientation="Horizontal" Margin="0,6,0,0">
            <TextBox Width="40" MaxLength="2" CharacterCasing="Upper"
                     Text="{Binding Project.State, UpdateSourceTrigger=PropertyChanged}"/>
            <TextBlock Text="Zip:" VerticalAlignment="Center" Margin="12,0,8,0"/>
            <TextBox Width="80" MaxLength="5"
                     Text="{Binding Project.Zip, UpdateSourceTrigger=PropertyChanged}"/>
        </StackPanel>

        <!-- Row 6: Study Date | Revision Month -->
        <TextBlock Grid.Row="6" Grid.Column="0" Text="Study Date:" 
                   VerticalAlignment="Center" Margin="0,6,8,0"/>
        <DatePicker Grid.Row="6" Grid.Column="1" Margin="0,6,0,0"
                    SelectedDate="{Binding Project.Date}"/>
        <TextBlock Grid.Row="6" Grid.Column="2" Text="Revision Month:" 
                   VerticalAlignment="Center" Margin="16,6,8,0"/>
        <!-- DECISION: Use DatePicker for now, format as "MM/YYYY" in ViewModel -->
        <DatePicker Grid.Row="6" Grid.Column="3" Margin="0,6,0,0"
                    SelectedDate="{Binding Project.RevisionMonth}"/>

        <!-- Row 7: Comments (multiline) -->
        <TextBlock Grid.Row="7" Grid.Column="0" Text="Comments:" 
                   VerticalAlignment="Top" Margin="0,6,8,0"/>
        <TextBox Grid.Row="7" Grid.Column="1" Grid.ColumnSpan="3" 
                 Margin="0,6,0,0" AcceptsReturn="True" TextWrapping="Wrap" 
                 Height="80" VerticalScrollBarVisibility="Auto"
                 Text="{Binding Project.Comments, UpdateSourceTrigger=PropertyChanged}"/>
    </Grid>
</GroupBox>
```

---

## ??? **Summary Tab Layout**

### **Two-Section Design**

**Top Section:** Metadata GroupBox (as shown above)

**Bottom Section:** File Management + Statistics (Map Editor pattern)

```xaml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>  <!-- Metadata -->
        <RowDefinition Height="*"/>     <!-- File Management + Stats -->
    </Grid.RowDefinitions>

    <!-- Metadata GroupBox (from above) -->
    <GroupBox Grid.Row="0" Header="Project">
        <!-- ... metadata grid ... -->
    </GroupBox>

    <!-- File Management + Statistics -->
    <Grid Grid.Row="1" Margin="0,8,0,0">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="*"/>      <!-- File list -->
            <ColumnDefinition Width="Auto"/>   <!-- Statistics panel -->
        </Grid.ColumnDefinitions>

        <!-- Left: Referenced Files (like Map Editor) -->
        <GroupBox Grid.Column="0" Header="Referenced Files" Margin="0,0,8,0">
            <!-- Drag-drop zone -->
            <!-- File list with Remove buttons -->
        </GroupBox>

        <!-- Right: Statistics Panel -->
        <GroupBox Grid.Column="1" Header="Statistics" Width="300">
            <StackPanel Margin="8">
                <!-- New Data GroupBox (drop zone) -->
                <GroupBox Header="New Data (Drop files here)" Margin="0,0,0,8">
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="Auto"/>
                        </Grid.ColumnDefinitions>
                        <!-- Equipment counts (Buses, Breakers, etc.) -->
                        <TextBlock Grid.Row="0" Grid.Column="0" Text="Buses"/>
                        <TextBlock Grid.Row="0" Grid.Column="1" Text="{Binding NewBusCount}"/>
                        <!-- ... more rows ... -->
                    </Grid>
                </GroupBox>

                <!-- Old Data GroupBox (drop zone) -->
                <GroupBox Header="Old Data (Drop files here)">
                    <!-- Same as New Data -->
                </GroupBox>
            </StackPanel>
        </GroupBox>
    </Grid>
</Grid>
```

---

## ?? **Data Tab Structure**

### **One Tab Per Equipment Type (with data)**

**Tab Header:** `DisplayName` from `[EasyPowerClass]` attribute  
**Content:** DiffGrid showing Old vs New comparison

### **ViewModel per Tab** (state preservation)

```csharp
public class DataTypeTabViewModel : BindableBase
{
    private readonly string _dataType;
    private readonly Project _project;
    
    // State properties (persisted when tab is inactive)
    public ObservableCollection<DiffRow> Rows { get; }
    public DiffRow? SelectedRow { get; set; }
    public int FirstVisibleRowIndex { get; set; }
    public double ViewportRowOffset { get; set; }
    public double HorizontalOffset { get; set; }
    
    // DiffGrid will bind to these and preserve state automatically
    // (DiffGrid.State.cs handles the actual restoration)
}
```

### **View per Tab**

```xaml
<UserControl x:Class="EasyAF.Modules.Project.Views.DataTypeTabView">
    <diffgrid:DiffGrid ItemsSource="{Binding Rows}"
                       SelectedItem="{Binding SelectedRow, Mode=TwoWay}"
                       <!-- State preservation via DiffGrid's built-in mechanism -->
                       />
</UserControl>
```

---

## ?? **Project Class Modifications Required**

**Add to `lib/EasyAF.Data/Models/Project.cs`:**

```csharp
public class Project : BindableBase
{
    // Existing properties...
    
    // NEW PROPERTIES for metadata
    private string? _lbProjectNumber;
    private string? _studyEngineer;
    private string? _addressLine2;
    private string? _addressLine3;
    private string? _city;
    private string? _state;
    private string? _zip;
    private DateTime? _revisionMonth;
    private string? _comments;

    [Description("L&B Project Number")]
    public string? LBProjectNumber
    {
        get => _lbProjectNumber;
        set => SetProperty(ref _lbProjectNumber, value);
    }

    [Description("Study Engineer")]
    public string? StudyEngineer
    {
        get => _studyEngineer;
        set => SetProperty(ref _studyEngineer, value);
    }

    [Description("Address Line 2")]
    public string? AddressLine2
    {
        get => _addressLine2;
        set => SetProperty(ref _addressLine2, value);
    }

    [Description("Address Line 3")]
    public string? AddressLine3
    {
        get => _addressLine3;
        set => SetProperty(ref _addressLine3, value);
    }

    [Description("City")]
    public string? City
    {
        get => _city;
        set => SetProperty(ref _city, value);
    }

    [Description("State")]
    public string? State
    {
        get => _state;
        set => SetProperty(ref _state, value);
    }

    [Description("Zip Code")]
    public string? Zip
    {
        get => _zip;
        set => SetProperty(ref _zip, value);
    }

    [Description("Revision Month")]
    public DateTime? RevisionMonth
    {
        get => _revisionMonth;
        set => SetProperty(ref _revisionMonth, value);
    }

    [Description("Comments")]
    public string? Comments
    {
        get => _comments;
        set => SetProperty(ref _comments, value);
    }
}
```

---

## ? **Implementation Checklist**

### **Phase 4A: Foundation**
- [ ] Import DiffGrid control to solution
- [ ] Add new properties to Project class
- [ ] Test Project serialization (ensure new fields persist)

### **Phase 4B: Module Structure (Task 18)**
- [ ] Create `EasyAF.Modules.Project` folder structure
- [ ] Implement `ProjectModule : IDocumentModule`
- [ ] Register with shell

### **Phase 4C: Document Model (Task 19)**
- [ ] Create `ProjectDocument` wrapper
- [ ] Implement `IDocument` interface
- [ ] Wire to existing `Project` class

### **Phase 4D: Summary Tab (Task 20)**
- [ ] Create `ProjectSummaryViewModel`
- [ ] Create `ProjectSummaryView` with metadata GroupBox
- [ ] Add file management UI (drag-drop)
- [ ] Add statistics panels (New/Old data counts)

### **Phase 4E: Data Tabs (Task 21)**
- [ ] Create `DataTypeTabViewModel`
- [ ] Create `DataTypeTabView` with DiffGrid
- [ ] Implement dynamic tab generation
- [ ] Test state preservation (tab switching)
- [ ] Test DPI changes (monitor move/resize)

### **Phase 4F: Import Workflow (Task 22 - Later)**
- [ ] Defer until tabs are working

---

## ?? **Success Criteria**

1. ? Metadata fields exactly match stopgap layout
2. ? State preservation works (tab switching preserves scroll/selection)
3. ? DPI changes don't break state (monitor move/resize)
4. ? DiffGrid shows Old vs New data
5. ? Statistics update when data loaded
6. ? Theme compliant (all DynamicResource bindings)
7. ? MVVM strict (zero code-behind)

---

**Ready to Proceed:** YES ?  
**Next Action:** Start Task 18 - Create Project Module Structure

