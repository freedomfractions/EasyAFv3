# Phase 2: Tree View UI - XAML Changes Needed

## Status
? Backend code complete (DataTypeNodeViewModel, tree building logic)
?? XAML changes pending (manual edit required to avoid file corruption)

## Files Already Modified:
1. `DataTypeNodeViewModel.cs` - Created ?
2. `ProjectSummaryViewModel.cs` - Added tree building logic ?

## XAML Changes Required:

### Step 1: Add xmlns for ViewModels

At the top of `ProjectSummaryView.xaml`, add:

```xaml
<UserControl ...
             xmlns:local="clr-namespace:EasyAF.Modules.Project.ViewModels"
             ...>
```

### Step 2: Replace New Data Grid (lines 260-288)

Replace this section:
```xaml
<!-- Data Grid -->
<Grid Grid.Row="2">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="Auto"/>
    </Grid.ColumnDefinitions>
    <Grid.RowDefinitions>
        <RowDefinition Height="24"/>
        <RowDefinition Height="24"/>
        <RowDefinition Height="24"/>
        <RowDefinition Height="24"/>
        <RowDefinition Height="24"/>
    </Grid.RowDefinitions>
    
    <TextBlock Grid.Row="0" Grid.Column="0" Text="Buses" .../>
    <TextBlock Grid.Row="0" Grid.Column="1" Text="{Binding NewBusCount}" .../>
    ...
</Grid>
```

With this TreeView:
```xaml
<!-- Tree View -->
<TreeView Grid.Row="2"
          ItemsSource="{Binding NewDataTreeNodes}"
          BorderThickness="0"
          Background="Transparent">
    <TreeView.ItemContainerStyle>
        <Style TargetType="TreeViewItem">
            <Setter Property="IsExpanded" Value="{Binding IsExpanded, Mode=TwoWay}"/>
            <Setter Property="IsSelected" Value="{Binding IsSelected, Mode=TwoWay}"/>
            <Setter Property="FontSize" Value="13"/>
            <Setter Property="Padding" Value="2"/>
        </Style>
    </TreeView.ItemContainerStyle>
    <TreeView.Resources>
        <HierarchicalDataTemplate DataType="{x:Type local:DataTypeNodeViewModel}" 
                                  ItemsSource="{Binding Children}">
            <StackPanel Orientation="Horizontal">
                <TextBlock Text="{Binding Icon}" 
                           Margin="0,0,6,0" 
                           FontSize="12"
                           VerticalAlignment="Center"/>
                <TextBlock Text="{Binding DisplayName}" 
                           Foreground="{DynamicResource TextPrimaryBrush}"
                           VerticalAlignment="Center"/>
                <TextBlock Text="{Binding WarningIndicator}" 
                           Margin="6,0,0,0" 
                           FontSize="12"
                           VerticalAlignment="Center"
                           ToolTip="Scenario counts are inconsistent"/>
            </StackPanel>
        </HierarchicalDataTemplate>
    </TreeView.Resources>
</TreeView>
```

### Step 3: Replace Old Data Grid (similar section around lines 320-348)

Apply the same TreeView replacement, but use `ItemsSource="{Binding OldDataTreeNodes}"`.

## Testing After XAML Changes:

1. Build the project
2. Run the app
3. Import data with scenarios (Arc Flash/Short Circuit CSV)
4. Verify tree structure appears:
   ```
   ?? Arc Flash (160)
     ?? Main-Max (40)
     ?? Main-Min (40)
     ?? Emergency (40)
     ?? Generator (40)
   ? Buses (14)
   ? Breakers (9)
   ```

5. Verify expand/collapse works
6. Verify ?? appears if scenario counts are uneven

## Next Phase After This Works:

- Phase 3: Context menus & actions (Clear, Replace, Delete scenario)
- Phase 4: Drag-drop import
- Phase 5: Scenario selection for Composite projects
