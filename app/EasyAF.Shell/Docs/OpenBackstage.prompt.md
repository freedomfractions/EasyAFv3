I'll create a more detailed and AI-agent-friendly prompt specification for the Open backstage implementation.

```markdown
AF.Shell\Docs\OpenBackstage.prompt.md
# Open Backstage Implementation Specification

## Overview
This document provides step-by-step instructions for implementing the Open backstage tab in EasyAF Shell. Each step is designed to be completed independently and reviewed before proceeding.

## Architecture Constraints
- **MVVM Pattern**: No logic in code-behind except InitializeComponent() and event wire-up
- **Theme Resources**: Use only DynamicResource bindings from Theme/Light.xaml and Theme/Dark.xaml
- **Icons**: Use Segoe MDL2 Assets font for all icons (as TextBlock glyphs)
- **Styles**: Reuse existing styles from Styles/CommonControls.xaml
- **.NET 8 WPF** with Fluent.Ribbon

## File Structure Plan
```
app/EasyAF.Shell/
├── Views/
│   └── Backstage/
│       ├── OpenBackstageView.xaml
│       └── OpenBackstageView.xaml.cs
├── ViewModels/
│   └── Backstage/
│       └── OpenBackstageViewModel.cs
├── Models/
│   └── Backstage/
│       ├── RecentItem.cs
│       └── QuickAccessItem.cs
└── Services/
    └── IBackstageService.cs (interface for backstage control)
```

## Implementation Steps

### Step 1: Basic Layout Structure
**Goal**: Create the two-column layout with header and separator.

**Files to create/modify**:
1. Create `Views/Backstage/OpenBackstageView.xaml`
2. Create `Views/Backstage/OpenBackstageView.xaml.cs`
3. Modify `MainWindow.xaml` (replace Open tab content)

**XAML Structure**:
```
<UserControl x:Class="EasyAF.Shell.Views.Backstage.OpenBackstageView">
    <Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/> <!-- Header -->
            <RowDefinition Height="*"/>    <!-- Content -->
        </Grid.RowDefinitions>
        
        <!-- Header -->
        <TextBlock Grid.Row="0" Text="Open existing document" 
                   FontSize="18" FontWeight="SemiBold"
                   Foreground="{DynamicResource TextPrimaryBrush}"
                   Margin="0,0,0,20"/>
        
        <!-- Two Column Layout -->
        <Grid Grid.Row="1">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="200"/>  <!-- Left nav -->
                <ColumnDefinition Width="Auto"/> <!-- Separator -->
                <ColumnDefinition Width="*"/>    <!-- Right content -->
            </Grid.ColumnDefinitions>
            
            <!-- Left Column Placeholder -->
            <StackPanel Grid.Column="0" />
            
            <!-- Vertical Separator -->
            <Border Grid.Column="1" Width="1" 
                    Margin="16,0"
                    Background="{DynamicResource ControlBorderBrush}"/>
            
            <!-- Right Column Placeholder -->
            <Grid Grid.Column="2" />
        </Grid>
    </Grid>
</UserControl>
```

**Acceptance Criteria**:
- Two-column layout renders correctly
- Header text displays with proper styling
- Vertical separator visible between columns
- All colors from theme resources

### Step 2: Left Navigation Column
**Goal**: Implement navigation buttons with icons and separators.

**Add to OpenBackstageView.xaml**:
```
<!-- In Left Column StackPanel -->
<RadioButton x:Name="RecentButton" 
             Style="{StaticResource NavigationRadioButtonStyle}"
             IsChecked="True">
    <StackPanel Orientation="Horizontal">
        <TextBlock Text="&#xE81C;" FontFamily="Segoe MDL2 Assets" 
                   Margin="0,0,8,0" VerticalAlignment="Center"/>
        <TextBlock Text="Recent Files" VerticalAlignment="Center"/>
    </StackPanel>
</RadioButton>

<Separator Height="1" Margin="0,8" 
           Background="{DynamicResource ControlBorderBrush}"/>

<RadioButton x:Name="QuickAccessButton"
             Style="{StaticResource NavigationRadioButtonStyle}">
    <StackPanel Orientation="Horizontal">
        <TextBlock Text="&#xE8B7;" FontFamily="Segoe MDL2 Assets" 
                   Margin="0,0,8,0" VerticalAlignment="Center"/>
        <TextBlock Text="Quick Access" VerticalAlignment="Center"/>
    </StackPanel>
</RadioButton>

<Separator Height="1" Margin="0,8" 
           Background="{DynamicResource ControlBorderBrush}"/>

<Button x:Name="BrowseButton" 
        Style="{StaticResource BaseButtonStyle}"
        Padding="8,6">
    <StackPanel Orientation="Horizontal">
        <TextBlock Text="&#xE8E5;" FontFamily="Segoe MDL2 Assets" 
                   Margin="0,0,8,0" VerticalAlignment="Center"/>
        <TextBlock Text="Browse..." VerticalAlignment="Center"/>
    </StackPanel>
</Button>
```

**Icon Codes**:
- Recent Files: `&#xE81C;` (History/Clock)
- Quick Access: `&#xE8B7;` (Folder)
- Browse: `&#xE8E5;` (Open Folder)

**Create NavigationRadioButtonStyle** in CommonControls.xaml if not exists.

**Acceptance Criteria**:
- Three buttons display with correct icons
- Separators visible between sections
- Radio buttons mutually exclusive
- Browse button distinct from navigation options

### Step 3: Right Column Search Bar
**Goal**: Add search bar with icon at top of right column.

**Add to right column Grid**:
```
<Grid.RowDefinitions>
    <RowDefinition Height="Auto"/> <!-- Search -->
    <RowDefinition Height="*"/>    <!-- Content -->
</Grid.RowDefinitions>

<!-- Search Bar -->
<Border Grid.Row="0" 
        Background="{DynamicResource ControlBackgroundBrush}"
        BorderBrush="{DynamicResource ControlBorderBrush}"
        BorderThickness="1"
        CornerRadius="4"
        Margin="0,0,0,12">
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>
        
        <TextBlock Grid.Column="0" 
                   Text="&#xE721;" 
                   FontFamily="Segoe MDL2 Assets"
                   Foreground="{DynamicResource TextSecondaryBrush}"
                   Margin="8,0" 
                   VerticalAlignment="Center"/>
        
        <TextBox Grid.Column="1" 
                 x:Name="SearchBox"
                 BorderThickness="0"
                 Background="Transparent"
                 VerticalAlignment="Center"
                 Margin="0,4">
            <TextBox.Style>
                <Style TargetType="TextBox" BasedOn="{StaticResource {x:Type TextBox}}">
                    <Style.Triggers>
                        <Trigger Property="Text" Value="">
                            <Setter Property="Tag" Value="Search"/>
                        </Trigger>
                    </Style.Triggers>
                </Style>
            </TextBox.Style>
        </TextBox>
    </Grid>
</Border>

<!-- Content Host -->
<ScrollViewer Grid.Row="1" 
              VerticalScrollBarVisibility="Auto">
    <ContentControl x:Name="ContentHost"/>
</ScrollViewer>
```

**Acceptance Criteria**:
- Search bar displays with magnifying glass icon
- Placeholder text "Search" visible when empty
- ScrollViewer ready for content
- Theme-consistent styling

### Step 4: Recent Files Tab Control
**Goal**: Add tab control for Files/Folders under search when Recent is selected.

**Create DataTemplate for Recent mode**:
```
<UserControl.Resources>
    <DataTemplate x:Key="RecentContentTemplate">
        <TabControl>
            <TabItem Header="Files">
                <ListView x:Name="FilesListView">
                    <ListView.View>
                        <GridView>
                            <GridViewColumn Width="40"/> <!-- Icon -->
                            <GridViewColumn Header="Name" Width="300"/>
                            <GridViewColumn Width="30"/> <!-- Star -->
                            <GridViewColumn Header="Date modified" Width="120"/>
                        </GridView>
                    </ListView.View>
                </ListView>
            </TabItem>
            
            <TabItem Header="Folders">
                <ListView x:Name="FoldersListView">
                    <ListView.View>
                        <GridView>
                            <GridViewColumn Width="40"/> <!-- Icon -->
                            <GridViewColumn Header="Name" Width="350"/>
                            <GridViewColumn Header="Date modified" Width="120"/>
                        </GridView>
                    </ListView.View>
                </ListView>
            </TabItem>
        </TabControl>
    </DataTemplate>
</UserControl.Resources>
```

**Acceptance Criteria**:
- Tab control appears when Recent selected
- Files tab shows 4 columns (icon, name, star, date)
- Folders tab shows 3 columns (icon, name, date)
- Headers properly styled

### Step 5: Files Row Template
**Goal**: Implement the two-line row template for files.

**Create ItemTemplate for Files ListView**:
```
<ListView.ItemTemplate>
    <DataTemplate>
        <Grid Height="48">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="40"/>  <!-- Icon -->
                <ColumnDefinition Width="300"/> <!-- Name/Path -->
                <ColumnDefinition Width="30"/>  <!-- Star -->
                <ColumnDefinition Width="120"/> <!-- Date -->
            </Grid.ColumnDefinitions>
            
            <!-- File Icon -->
            <TextBlock Grid.Column="0" 
                       Text="&#xE8A5;" 
                       FontFamily="Segoe MDL2 Assets"
                       FontSize="20"
                       VerticalAlignment="Center"
                       HorizontalAlignment="Center"
                       Foreground="{DynamicResource TextPrimaryBrush}"/>
            
            <!-- Name and Path -->
            <StackPanel Grid.Column="1" VerticalAlignment="Center">
                <TextBlock Text="{Binding FileName}" 
                           FontWeight="SemiBold"
                           TextTrimming="CharacterEllipsis"
                           Foreground="{DynamicResource TextPrimaryBrush}"/>
                <TextBlock Text="{Binding DirectoryPath}" 
                           FontSize="11"
                           TextTrimming="CharacterEllipsis"
                           Foreground="{DynamicResource TextSecondaryBrush}"/>
            </StackPanel>
            
            <!-- Favorite Star -->
            <ToggleButton Grid.Column="2" 
                          IsChecked="{Binding IsFavorite}"
                          Style="{StaticResource StarToggleButtonStyle}">
                <TextBlock Text="{Binding IsFavorite, 
                           Converter={StaticResource BoolToStarGlyphConverter}}"
                           FontFamily="Segoe MDL2 Assets"
                           FontSize="16"/>
            </ToggleButton>
            
            <!-- Date Modified -->
            <TextBlock Grid.Column="3" 
                       Text="{Binding DateModified, StringFormat='{}{0:MMM d, yyyy}'}"
                       VerticalAlignment="Center"
                       Foreground="{DynamicResource TextSecondaryBrush}"/>
        </Grid>
    </DataTemplate>
</ListView.ItemTemplate>
```

**Acceptance Criteria**:
- File icon displays
- Two-line text (name + path) with different sizes
- Star toggles between filled/unfilled
- Date formatted correctly
- All vertically centered

### Step 6: Context Menus
**Goal**: Add right-click context menus.

**Files Context Menu**:
```
<ListView.ContextMenu>
    <ContextMenu>
        <MenuItem Header="Open" Command="{Binding OpenCommand}"/>
        <MenuItem Header="Open File Location" Command="{Binding OpenLocationCommand}"/>
        <Separator/>
        <MenuItem Header="Copy Path to Clipboard" Command="{Binding CopyPathCommand}"/>
        <MenuItem Header="Rename" Command="{Binding RenameCommand}"/>
        <MenuItem Header="Delete" Command="{Binding DeleteCommand}"/>
        <Separator/>
        <MenuItem Header="Remove from List" Command="{Binding RemoveFromListCommand}"/>
        <Separator/>
        <MenuItem Header="Add to Favorites" 
                  Command="{Binding AddToFavoritesCommand}"
                  Visibility="{Binding IsFavorite, 
                               Converter={StaticResource InverseBoolToVisibilityConverter}}"/>
        <MenuItem Header="Remove from Favorites" 
                  Command="{Binding RemoveFromFavoritesCommand}"
                  Visibility="{Binding IsFavorite, 
                               Converter={StaticResource BoolToVisibilityConverter}}"/>
    </ContextMenu>
</ListView.ContextMenu>
```

**Acceptance Criteria**:
- Context menu appears on right-click
- All menu items present
- Add/Remove Favorites mutually exclusive based on state

### Step 7: Grouping Implementation
**Goal**: Group items by date categories.

**Add CollectionViewSource**:
```
<UserControl.Resources>
    <CollectionViewSource x:Key="GroupedFiles" 
                          Source="{Binding RecentFiles}">
        <CollectionViewSource.GroupDescriptions>
            <PropertyGroupDescription PropertyName="DateCategory"/>
        </CollectionViewSource.GroupDescriptions>
    </CollectionViewSource>
</UserControl.Resources>

<!-- Update ListView binding -->
<ListView ItemsSource="{Binding Source={StaticResource GroupedFiles}}">
    <ListView.GroupStyle>
        <GroupStyle>
            <GroupStyle.HeaderTemplate>
                <DataTemplate>
                    <TextBlock Text="{Binding Name}" 
                               FontWeight="SemiBold"
                               Foreground="{DynamicResource TextPrimaryBrush}"
                               Margin="0,8,0,4"/>
                </DataTemplate>
            </GroupStyle.HeaderTemplate>
        </GroupStyle>
    </ListView.GroupStyle>
</ListView>
```

**Date Categories Logic** (in ViewModel):
- Favorites (IsFavorite = true)
- Today
- Yesterday  
- This Week
- Last Week
- Older

**Acceptance Criteria**:
- Items grouped under category headers
- Groups appear in correct order
- Headers styled consistently

### Step 8: ViewModel Implementation
**Goal**: Create view model with commands and properties.

**Create OpenBackstageViewModel.cs**:
```
public class OpenBackstageViewModel : BindableBase
{
    private BackstageMode _mode = BackstageMode.Recent;
    private RecentTab _recentTab = RecentTab.Files;
    private string _searchText = string.Empty;
    
    public BackstageMode Mode 
    { 
        get => _mode; 
        set => SetProperty(ref _mode, value); 
    }
    
    public RecentTab RecentTab 
    { 
        get => _recentTab; 
        set => SetProperty(ref _recentTab, value); 
    }
    
    public string SearchText 
    { 
        get => _searchText; 
        set 
        { 
            SetProperty(ref _searchText, value);
            FilterItems();
        }
    }
    
    public ObservableCollection<RecentFileItem> RecentFiles { get; }
    public ObservableCollection<RecentFolderItem> RecentFolders { get; }
    
    // Commands
    public ICommand SelectRecentCommand { get; }
    public ICommand SelectQuickAccessCommand { get; }
    public ICommand BrowseCommand { get; }
    public ICommand OpenCommand { get; }
    public ICommand OpenLocationCommand { get; }
    // ... other commands
}
```

**Acceptance Criteria**:
- View model properties bindable
- Commands execute without errors
- Search filters items in real-time

### Step 9: Quick Access Mode
**Goal**: Implement Quick Access with navigation strip.

**Quick Access Template**:
```
<DataTemplate x:Key="QuickAccessContentTemplate">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/> <!-- Navigation -->
            <RowDefinition Height="Auto"/> <!-- Sort -->
            <RowDefinition Height="*"/>    <!-- Content -->
        </Grid.RowDefinitions>
        
        <!-- Navigation Strip -->
        <Border Grid.Row="0" 
                Background="{DynamicResource SecondaryBackgroundBrush}"
                Padding="4" Margin="0,0,0,8">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="Auto"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>
                
                <Button Grid.Column="0" 
                        Command="{Binding NavigateUpCommand}"
                        Style="{StaticResource BaseButtonStyle}">
                    <TextBlock Text="&#xE74A;" 
                               FontFamily="Segoe MDL2 Assets"/>
                </Button>
                
                <TextBlock Grid.Column="1" 
                           Text="{Binding CurrentPath}"
                           VerticalAlignment="Center"
                           Margin="8,0"
                           TextTrimming="CharacterEllipsis"/>
            </Grid>
        </Border>
        
        <!-- Sort Options -->
        <ComboBox Grid.Row="1" 
                  SelectedItem="{Binding SortOption}"
                  Margin="0,0,0,8">
            <ComboBoxItem>Type</ComboBoxItem>
            <ComboBoxItem>Name</ComboBoxItem>
            <ComboBoxItem>Date modified</ComboBoxItem>
        </ComboBox>
        
        <!-- Combined Files/Folders List -->
        <ListView Grid.Row="2" 
                  ItemsSource="{Binding QuickAccessItems}"/>
    </Grid>
</DataTemplate>
```

**Acceptance Criteria**:
- Navigation strip shows current path
- Up button navigates to parent folder
- Sort dropdown changes item order
- Shows both files and folders

### Step 10: Browse Integration
**Goal**: Wire Browse button to open dialog and close backstage.

**In OpenBackstageViewModel**:
```
private void ExecuteBrowse()
{
    // Call existing FileCommands.OpenCommand
    _fileCommands.OpenCommand.Execute(null);
    
    // Close backstage
    _backstageService.CloseBackstage();
}
```

**IBackstageService Interface**:
```
public interface IBackstageService
{
    bool IsBackstageOpen { get; set; }
    void CloseBackstage();
}
```

**Acceptance Criteria**:
- Browse opens file dialog
- Backstage closes after file selected
- Backstage closes if dialog cancelled

### Step 11: Sample Data Generation
**Goal**: Create realistic sample data for testing.

**Sample Data Helper**:
```
private void GenerateSampleData()
{
    var now = DateTime.Now;
    
    RecentFiles.Add(new RecentFileItem
    {
        FileName = "Project_Q4_2024.ezproj",
        DirectoryPath = @"C:\Users\Documents\EasyAF\Projects",
        DateModified = now.AddHours(-2),
        IsFavorite = true
    });
    
    RecentFiles.Add(new RecentFileItem
    {
        FileName = "DataMapping_Template.ezmap",
        DirectoryPath = @"C:\Users\Documents\EasyAF\Templates",
        DateModified = now.AddDays(-1),
        IsFavorite = false
    });
    
    // Add more samples covering all date categories
}
```

**Acceptance Criteria**:
- Sample data covers all date categories
- Mix of favorites and non-favorites
- Realistic file names and paths

### Step 12: Final Integration
**Goal**: Replace MainWindow.xaml backstage content.

**In MainWindow.xaml**:
```
<fluent:BackstageTabItem Header="Open">
    <views:OpenBackstageView 
        DataContext="{Binding OpenBackstageViewModel}"/>
</fluent:BackstageTabItem>
```

**In MainWindowViewModel**:
```
public OpenBackstageViewModel OpenBackstageViewModel { get; }

public MainWindowViewModel()
{
    OpenBackstageViewModel = new OpenBackstageViewModel(
        _fileCommands, 
        _recentFilesService,
        new BackstageService(this));
}
```

**Acceptance Criteria**:
- Backstage opens with new UI
- All navigation works
- Search filters correctly
- Browse closes backstage

## Testing Checklist

### Visual Testing
- [ ] Layout matches Office-style backstage
- [ ] All theme colors applied correctly
- [ ] Icons display properly
- [ ] Responsive to window resizing

### Functional Testing
- [ ] Recent/Quick Access switching works
- [ ] Search filters items
- [ ] Star toggle updates favorites
- [ ] Context menus appear and execute
- [ ] Browse opens dialog and closes backstage
- [ ] Grouping displays correctly

### Edge Cases
- [ ] Empty recent files list
- [ ] Very long file names truncate properly
- [ ] Search with no results shows empty state
- [ ] Quick Access navigation handles root folders

## Rollback Plan
To revert all changes:
1. Delete `Views/Backstage/` folder
2. Delete `ViewModels/Backstage/` folder  
3. Delete `Models/Backstage/` folder
4. Restore original `MainWindow.xaml` Open backstage content
5. Remove `OpenBackstageViewModel` property from `MainWindowViewModel`

## Notes for AI Agents
- Complete each step fully before moving to the next
- Use only existing theme resources - no hard-coded colors
- Maintain MVVM pattern strictly
- Test each step independently
- Add cross-module edit comments when modifying existing files
- Each step should compile without errors
```

This prompt specification:

1. **Breaks down into 12 manageable steps** - each can be implemented and reviewed independently
2. **Layout-first approach** - Steps 1-7 focus on visual structure before logic
3. **Clear acceptance criteria** - Each step has specific checkpoints
4. **Detailed code examples** - Shows exact XAML/C# structure expected
5. **Progressive complexity** - Starts simple (layout) and builds up (grouping, commands)
6. **Testing checklist** - Comprehensive validation points
7. **Rollback plan** - Easy to revert if needed
8. **AI-friendly** - Clear file paths, specific icon codes, and concrete examples

Each step is designed to be achievable in a single AI agent interaction, allowing for granular feedback and iteration before proceeding to the next step.