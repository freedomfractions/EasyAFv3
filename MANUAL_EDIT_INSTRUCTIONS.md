# Manual Edit Required: Frozen GridView Headers

## File to Edit
`app/EasyAF.Shell/Views/Backstage/OpenBackstageView.xaml`

## What to Change
Find the **Browser mode ListView** (QuickAccessFolder mode) around line 575-580.

### Current Code:
```xaml
<ListView Background="Transparent"
          BorderThickness="0"
          ItemsSource="{Binding BrowserEntries}"
          behaviors:DragToQuickAccessBehavior.IsEnabled="True"
          behaviors:DragToQuickAccessBehavior.QuickAccessTargetName="QuickAccessItemsControl">
```

### Change To:
```xaml
<ListView Background="Transparent"
          BorderThickness="0"
          ItemsSource="{Binding BrowserEntries}"
          Style="{StaticResource FrozenHeaderListViewStyle}"
          behaviors:DragToQuickAccessBehavior.IsEnabled="True"
          behaviors:DragToQuickAccessBehavior.QuickAccessTargetName="QuickAccessItemsControl">
```

## What This Does
- Adds `Style="{StaticResource FrozenHeaderListViewStyle}"` attribute
- Keeps column headers (Name, Size, Modified) **frozen at top** while list scrolls
- Headers no longer scroll away when browsing Quick Access folders
- Consistent UX with Windows Explorer

## How to Apply
1. Open `OpenBackstageView.xaml` in Visual Studio
2. Press `Ctrl+F` to search
3. Search for: `ItemsSource="{Binding BrowserEntries}"`
4. You'll find the ListView for Browser mode
5. Add the Style line after BorderThickness line
6. Save the file
7. Build should succeed with no errors
8. Delete this instruction file after applying

## Verification
Run the app, go to File ? Open backstage, click a Quick Access folder.
Scroll down the file list - the column headers should stay at the top!
