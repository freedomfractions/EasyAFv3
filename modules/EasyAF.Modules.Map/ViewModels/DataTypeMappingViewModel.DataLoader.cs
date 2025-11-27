using System;
using System.Collections.Generic;
using System.Linq;
using EasyAF.Modules.Map.Models;
using Serilog;
using MapPropertyInfo = EasyAF.Modules.Map.Models.PropertyInfo;

namespace EasyAF.Modules.Map.ViewModels
{
    /// <summary>
    /// Partial class containing data loading and refresh logic.
    /// </summary>
    public partial class DataTypeMappingViewModel
    {
        #region Public Refresh Methods

        public void RefreshAvailableTables()
        {
            LoadAvailableTables();
        }

        public void RefreshTargetProperties()
        {
            var visibleProperties = _propertyDiscovery.GetPropertiesForType(_dataType);
            var visiblePropertyNames = new HashSet<string>(visibleProperties.Select(p => p.PropertyName));

            if (_document.MappingsByDataType.TryGetValue(_dataType, out var existingMappings))
            {
                var mappingsToRemove = existingMappings
                    .Where(m => !visiblePropertyNames.Contains(m.PropertyName))
                    .ToList();

                foreach (var mapping in mappingsToRemove)
                {
                    _document.RemoveMapping(_dataType, mapping.PropertyName);
                    Log.Information("Removed mapping to hidden property: {DataType}.{Property}", _dataType, mapping.PropertyName);
                }

                if (mappingsToRemove.Any())
                {
                    foreach (var mapping in mappingsToRemove)
                    {
                        var sourceCol = SourceColumns.FirstOrDefault(c => c.ColumnName == mapping.ColumnHeader);
                        if (sourceCol != null)
                        {
                            sourceCol.IsMapped = false;
                            sourceCol.MappedTo = null;
                        }
                    }
                }
            }

            LoadTargetProperties();
            SourceColumnsView.Refresh();
            TargetPropertiesView.Refresh();
            RaisePropertyChanged(nameof(MappedCount));
            RaisePropertyChanged(nameof(AvailableCount));
            UpdateTabStatus();

            Log.Information("Refreshed properties for {DataType}", _dataType);
        }

        public void RestoreTableSelection()
        {
            if (!_document.TableReferencesByDataType.TryGetValue(_dataType, out var savedTableRef))
            {
                Log.Debug("No saved table reference for {DataType}", _dataType);
                return;
            }

            if (string.IsNullOrEmpty(savedTableRef))
            {
                Log.Debug("Empty table reference for {DataType}", _dataType);
                return;
            }

            Log.Debug("Attempting to restore table selection for {DataType}: '{SavedRef}'", _dataType, savedTableRef);
            
            var matchingItem = ComboBoxItems.OfType<TableItem>()
                .FirstOrDefault(item => item.TableReference.DisplayName == savedTableRef);
            
            if (matchingItem != null)
            {
                SelectedComboBoxItem = matchingItem;
                Log.Debug("Restored table selection for {DataType}: '{TableRef}'", _dataType, savedTableRef);
                return;
            }

            Log.Warning("Could not restore table selection for {DataType}: '{TableRef}'", _dataType, savedTableRef);
        }

        #endregion

        #region Private Data Loading

        private void LoadTargetProperties()
        {
            var properties = _propertyDiscovery.GetPropertiesForType(_dataType);
            
            TargetProperties.Clear();
            foreach (var prop in properties)
            {
                var isMapped = _document.MappingsByDataType.TryGetValue(_dataType, out var mappings) &&
                               mappings.Any(m => m.PropertyName == prop.PropertyName);

                prop.IsMapped = isMapped;
                if (isMapped)
                {
                    var mapping = mappings!.First(m => m.PropertyName == prop.PropertyName);
                    prop.MappedColumn = mapping.ColumnHeader;
                    prop.Confidence = mapping.Confidence;
                }

                TargetProperties.Add(prop);
            }

            Log.Debug("Loaded {Count} properties for {DataType}", properties.Count, _dataType);
        }

        private void LoadAvailableTables()
        {
            var previouslySelectedDisplayName = SelectedTable?.DisplayName;

            AvailableTables.Clear();
            ComboBoxItems.Clear();

            var tablesByFile = new Dictionary<string, List<TableReference>>();
            
            foreach (var file in _document.ReferencedFiles)
            {
                try
                {
                    var columns = _columnExtraction.ExtractColumns(file.FilePath);
                    
                    if (!tablesByFile.ContainsKey(file.FilePath))
                    {
                        tablesByFile[file.FilePath] = new List<TableReference>();
                    }
                    
                    foreach (var tableName in columns.Keys)
                    {
                        tablesByFile[file.FilePath].Add(new TableReference 
                        { 
                            TableName = tableName, 
                            FilePath = file.FilePath 
                        });
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to extract columns from {FilePath}", file.FilePath);
                }
            }

            if (tablesByFile.Count == 0)
            {
                ComboBoxItems.Add(new FileHeaderItem 
                { 
                    FileName = "? No sample files loaded - Add files in Summary tab" 
                });
                
                SelectedTable = null;
                SelectedComboBoxItem = null;
                SourceColumns.Clear();
                
                Log.Information("No sample files loaded - showing placeholder");
                return;
            }

            foreach (var fileGroup in tablesByFile.OrderBy(kvp => System.IO.Path.GetFileName(kvp.Key)))
            {
                bool isMultiTable = fileGroup.Value.Count > 1;
                string fileName = System.IO.Path.GetFileName(fileGroup.Key);
                
                if (isMultiTable)
                {
                    ComboBoxItems.Add(new FileHeaderItem { FileName = fileName });
                }
                
                foreach (var tableRef in fileGroup.Value)
                {
                    tableRef.IsMultiTableFile = isMultiTable;
                    AvailableTables.Add(tableRef);
                    ComboBoxItems.Add(new TableItem(tableRef));
                }
            }

            TableItem? itemToSelect = null;

            if (!string.IsNullOrEmpty(previouslySelectedDisplayName))
            {
                itemToSelect = ComboBoxItems.OfType<TableItem>()
                    .FirstOrDefault(i => string.Equals(i.TableReference.DisplayName, previouslySelectedDisplayName, StringComparison.Ordinal));

                if (itemToSelect != null)
                {
                    Log.Debug("Preserved previous table selection: {DisplayName}", previouslySelectedDisplayName);
                }
            }

            if (itemToSelect == null && _document.TableReferencesByDataType.TryGetValue(_dataType, out var savedRef) && !string.IsNullOrEmpty(savedRef))
            {
                itemToSelect = ComboBoxItems.OfType<TableItem>()
                    .FirstOrDefault(i => string.Equals(i.TableReference.DisplayName, savedRef, StringComparison.Ordinal));

                if (itemToSelect != null)
                {
                    Log.Debug("Restored saved table selection for {DataType}: {DisplayName}", _dataType, savedRef);
                }
            }

            if (itemToSelect != null)
            {
                SelectedComboBoxItem = itemToSelect;
            }

            Log.Information("Loaded {Count} available tables from {FileCount} files", AvailableTables.Count, tablesByFile.Count);
        }

        private void OnTableChanged(TableReference? tableRef)
        {
            if (tableRef == null || string.IsNullOrEmpty(tableRef.TableName)) return;

            try
            {
                var columns = _columnExtraction.ExtractColumns(tableRef.FilePath);
                if (columns.TryGetValue(tableRef.TableName, out var columnList))
                {
                    LoadSourceColumns(columnList);
                    Log.Information("Loaded {Count} columns from table '{Table}'", columnList.Count, tableRef.TableName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error loading table {Table}", tableRef.TableName);
            }
        }

        private void LoadSourceColumns(List<ColumnInfo> columns)
        {
            SourceColumns.Clear();
            
            foreach (var column in columns)
            {
                column.IsMapped = _document.MappingsByDataType.TryGetValue(_dataType, out var mappings) &&
                                  mappings.Any(m => m.ColumnHeader == column.ColumnName);

                if (column.IsMapped)
                {
                    var mapping = mappings!.First(m => m.ColumnHeader == column.ColumnName);
                    column.MappedTo = $"{_dataType}.{mapping.PropertyName}";
                }

                SourceColumns.Add(column);
            }
        }

        #endregion
    }
}
