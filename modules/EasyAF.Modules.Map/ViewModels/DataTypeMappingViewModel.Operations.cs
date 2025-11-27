using System.Linq;
using EasyAF.Modules.Map.Models;
using Serilog;
using MapPropertyInfo = EasyAF.Modules.Map.Models.PropertyInfo;

namespace EasyAF.Modules.Map.ViewModels
{
    /// <summary>
    /// Partial class containing manual mapping operations (Map, Unmap, Clear).
    /// </summary>
    public partial class DataTypeMappingViewModel
    {
        #region Map/Unmap Operations

        private bool CanExecuteMapSelected()
        {
            return HasTableSelected && 
                   SelectedSourceColumn != null && 
                   SelectedTargetProperty != null;
        }

        private void ExecuteMapSelected()
        {
            if (SelectedSourceColumn == null || SelectedTargetProperty == null) return;

            // Check 1: Is the selected PROPERTY already mapped to a DIFFERENT COLUMN?
            if (SelectedTargetProperty.IsMapped && SelectedTargetProperty.MappedColumn != SelectedSourceColumn.ColumnName)
            {
                var confirmed = _dialogService.Confirm(
                    $"Property '{SelectedTargetProperty.PropertyName}' is already mapped to column '{SelectedTargetProperty.MappedColumn}'.\n\n" +
                    $"Do you want to replace the existing mapping with '{SelectedSourceColumn.ColumnName}'?",
                    "Replace Existing Mapping?");
                
                if (!confirmed)
                {
                    Log.Debug("User canceled replacing property mapping for {Property}", SelectedTargetProperty.PropertyName);
                    return;
                }
                
                var oldColumn = SourceColumns.FirstOrDefault(c => c.ColumnName == SelectedTargetProperty.MappedColumn);
                if (oldColumn != null)
                {
                    oldColumn.IsMapped = false;
                    oldColumn.MappedTo = null;
                }
                
                Log.Information("Replacing property mapping for {Property}: {OldColumn} ? {NewColumn}",
                    SelectedTargetProperty.PropertyName, SelectedTargetProperty.MappedColumn, SelectedSourceColumn.ColumnName);
            }

            // Check 2: Is the selected COLUMN already mapped to a DIFFERENT PROPERTY?
            if (SelectedSourceColumn.IsMapped && SelectedSourceColumn.MappedTo != $"{_dataType}.{SelectedTargetProperty.PropertyName}")
            {
                var currentPropertyName = SelectedSourceColumn.MappedTo?.Split('.').LastOrDefault() ?? "Unknown";
                
                var confirmed = _dialogService.Confirm(
                    $"Column '{SelectedSourceColumn.ColumnName}' is already mapped to property '{currentPropertyName}'.\n\n" +
                    $"Do you want to replace the existing mapping with '{SelectedTargetProperty.PropertyName}'?",
                    "Replace Existing Mapping?");
                
                if (!confirmed)
                {
                    Log.Debug("User canceled replacing column mapping for {Column}", SelectedSourceColumn.ColumnName);
                    return;
                }
                
                var oldProperty = TargetProperties.FirstOrDefault(p => p.PropertyName == currentPropertyName);
                if (oldProperty != null)
                {
                    oldProperty.IsMapped = false;
                    oldProperty.MappedColumn = null;
                    _document.RemoveMapping(_dataType, oldProperty.PropertyName);
                }
                
                Log.Information("Replacing column mapping for {Column}: {OldProperty} ? {NewProperty}",
                    SelectedSourceColumn.ColumnName, currentPropertyName, SelectedTargetProperty.PropertyName);
            }

            try
            {
                _document.UpdateMapping(_dataType, SelectedTargetProperty.PropertyName, SelectedSourceColumn.ColumnName);

                // Clear confidence when user manually maps (manual edit = 100% trust, no badge needed)
                if (_document.MappingsByDataType.TryGetValue(_dataType, out var entries))
                {
                    var entry = entries.FirstOrDefault(e => e.PropertyName == SelectedTargetProperty.PropertyName);
                    if (entry != null)
                    {
                        entry.Confidence = null;
                    }
                }

                SelectedTargetProperty.IsMapped = true;
                SelectedTargetProperty.MappedColumn = SelectedSourceColumn.ColumnName;
                SelectedTargetProperty.Confidence = null;
                SelectedSourceColumn.IsMapped = true;
                SelectedSourceColumn.MappedTo = $"{_dataType}.{SelectedTargetProperty.PropertyName}";

                SourceColumnsView.Refresh();
                TargetPropertiesView.Refresh();
                RaisePropertyChanged(nameof(MappedCount));
                UpdateCommandStates();
                UpdateTabStatus();

                Log.Information("Mapped {Column} -> {DataType}.{Property}",
                    SelectedSourceColumn.ColumnName, _dataType, SelectedTargetProperty.PropertyName);
            }
            catch (System.Exception ex)
            {
                Log.Error(ex, "Failed to create mapping");
            }
        }

        private bool CanExecuteUnmapSelected()
        {
            return SelectedTargetProperty?.IsMapped == true;
        }

        private void ExecuteUnmapSelected()
        {
            if (SelectedTargetProperty == null) return;

            try
            {
                var mappedColumn = SelectedTargetProperty.MappedColumn;
                if (mappedColumn != null)
                {
                    var sourceCol = SourceColumns.FirstOrDefault(c => c.ColumnName == mappedColumn);
                    if (sourceCol != null)
                    {
                        sourceCol.IsMapped = false;
                        sourceCol.MappedTo = null;
                    }
                }

                _document.RemoveMapping(_dataType, SelectedTargetProperty.PropertyName);
                SelectedTargetProperty.IsMapped = false;
                SelectedTargetProperty.MappedColumn = null;
                SelectedTargetProperty.Confidence = null;  // Clear confidence badge when unmapping

                SourceColumnsView.Refresh();
                TargetPropertiesView.Refresh();
                RaisePropertyChanged(nameof(MappedCount));
                UpdateTabStatus();

                Log.Information("Unmapped {DataType}.{Property} (confidence badge cleared)", _dataType, SelectedTargetProperty.PropertyName);
            }
            catch (System.Exception ex)
            {
                Log.Error(ex, "Failed to remove mapping");
            }
        }

        #endregion

        #region Clear/Reset Operations

        private void ExecuteClearMappings()
        {
            if (_document.MappingsByDataType.TryGetValue(_dataType, out var mappings) && mappings.Count > 0)
            {
                var confirmed = _dialogService.Confirm(
                    $"This will remove all mappings for {DataTypeDisplayName}.\n\n" +
                    $"Are you sure?",
                    "Clear All Mappings?");

                if (confirmed)
                {
                    _document.ClearMappings(_dataType);
                    foreach (var prop in TargetProperties)
                    {
                        prop.IsMapped = false;
                        prop.MappedColumn = null;
                        prop.Confidence = null;
                    }
                    foreach (var col in SourceColumns)
                    {
                        col.IsMapped = false;
                        col.MappedTo = null;
                    }
                    SourceColumnsView.Refresh();
                    TargetPropertiesView.Refresh();
                    RaisePropertyChanged(nameof(MappedCount));
                    UpdateTabStatus();
                    Log.Information("Cleared all mappings for {DataType}", _dataType);
                }
            }
        }

        private void ExecuteManageFields()
        {
            Log.Information("Manage Fields requested for {DataType}", _dataType);
        }

        private bool CanExecuteResetTable()
        {
            return HasTableSelected;
        }

        private void ExecuteResetTable()
        {
            SelectedTable = null!;
            SelectedComboBoxItem = null;
            SourceColumns.Clear();
            Log.Information("Reset table selection for {DataType}", _dataType);
        }

        #endregion

        #region Mapping Helper

        /// <summary>
        /// Creates a mapping between the specified property and column, updating the document and UI.
        /// </summary>
        private void CreateMapping(MapPropertyInfo property, ColumnInfo column, double? confidence = null)
        {
            _document.UpdateMapping(_dataType, property.PropertyName, column.ColumnName);

            if (confidence.HasValue && _document.MappingsByDataType.TryGetValue(_dataType, out var entries))
            {
                var entry = entries.FirstOrDefault(e => e.PropertyName == property.PropertyName);
                if (entry != null)
                {
                    entry.Confidence = confidence.Value;
                }
            }

            property.IsMapped = true;
            property.MappedColumn = column.ColumnName;
            property.Confidence = confidence;
            column.IsMapped = true;
            column.MappedTo = $"{_dataType}.{property.PropertyName}";

            SourceColumnsView.Refresh();
            TargetPropertiesView.Refresh();
            RaisePropertyChanged(nameof(MappedCount));
            UpdateCommandStates();
            UpdateTabStatus();
        }

        #endregion
    }
}
