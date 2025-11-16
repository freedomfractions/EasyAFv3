# Mapping Module Implementation Specification

## Overview
This document provides step-by-step instructions for implementing the Mapping Module in EasyAF Shell. The module enables users to create and edit .ezmap files that define associations between data file columns (from CSV/Excel exports) and EasyAF data type properties.

## Context & Purpose
The Mapping Module solves a critical problem: third-party software (EasyPower) frequently changes column names between versions, adding/removing spaces or renaming fields entirely. Instead of hardcoding mappings, we provide a visual tool to create flexible, reusable mapping configurations that can adapt to these changes.

## Architecture Overview
- **Module Pattern**: Self-contained module with ribbon integration
- **MVVM Strict**: No logic in code-behind except InitializeComponent()
- **Core Libraries**: Leverage existing EasyAF.Import.MappingConfig for persistence
- **Document Model**: Multi-tab document interface with summary and data type tabs
- **.NET 8 WPF** with Fluent.Ribbon
- **Theme Integration**: Full DynamicResource binding to Light/Dark themes

## Critical Dependencies
These libraries MUST be referenced and working:
- **EasyAF.Data** (data type definitions - Bus, LVCB, ArcFlash, etc.)
- **EasyAF.Import** (MappingConfig serialization and ImportManager)
- **EasyAF.Engine** (future integration for validation)

## File Structure Plan
```
modules/EasyAF.Modules.Map/
??? MapModule.cs (module registration - ALREADY EXISTS)
??? Models/
?   ??? MapDocument.cs
?   ??? PropertyInfo.cs
?   ??? ColumnInfo.cs
??? ViewModels/
?   ??? MapDocumentViewModel.cs
?   ??? MapSummaryViewModel.cs
?   ??? DataTypeMappingViewModel.cs
??? Views/
?   ??? MapDocumentView.xaml
?   ??? MapDocumentView.xaml.cs
?   ??? MapSummaryView.xaml
?   ??? MapSummaryView.xaml.cs
?   ??? DataTypeMappingView.xaml
?   ??? DataTypeMappingView.xaml.cs
??? Services/
?   ??? IPropertyDiscoveryService.cs
?   ??? PropertyDiscoveryService.cs
?   ??? IAutoMappingService.cs
?   ??? AutoMappingService.cs
?   ??? ColumnExtractionService.cs
??? Converters/
    ??? MappingStatusConverter.cs
```

## Implementation Phases

---

## PHASE 1: Core Data Model & Service Infrastructure

### Task 13: Map Document Data Model
**Goal**: Create the MapDocument class that represents a .ezmap file in memory.

**Create `Models/MapDocument.cs`**:
```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using EasyAF.Core.Contracts;
using EasyAF.Import;

namespace EasyAF.Modules.Map.Models
{
    /// <summary>
    /// Represents a mapping document (.ezmap) that defines associations between 
    /// source file columns and target data type properties.
    /// </summary>
    public class MapDocument : IDocument, INotifyPropertyChanged
    {
        private string? _filePath;
        private bool _isDirty;
        private string _mapName = "Untitled Map";
        private string _softwareVersion = "3.0.0";
        private string _description = string.Empty;
        private DateTime _dateModified = DateTime.Now;

        public event PropertyChangedEventHandler? PropertyChanged;

        // IDocument implementation
        public string? FilePath 
        { 
            get => _filePath;
            set { _filePath = value; OnPropertyChanged(nameof(FilePath)); }
        }

        public bool IsDirty 
        { 
            get => _isDirty;
            set { _isDirty = value; OnPropertyChanged(nameof(IsDirty)); }
        }

        public string DisplayName => string.IsNullOrEmpty(_mapName) 
            ? "Untitled Map" 
            : _mapName;

        // Map metadata
        public string MapName
        {
            get => _mapName;
            set { _mapName = value; MarkDirty(); OnPropertyChanged(nameof(MapName)); OnPropertyChanged(nameof(DisplayName)); }
        }

        public string SoftwareVersion
        {
            get => _softwareVersion;
            set { _softwareVersion = value; MarkDirty(); OnPropertyChanged(nameof(SoftwareVersion)); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; MarkDirty(); OnPropertyChanged(nameof(Description)); }
        }

        public DateTime DateModified
        {
            get => _dateModified;
            private set { _dateModified = value; OnPropertyChanged(nameof(DateModified)); }
        }

        // Mapping data
        public Dictionary<string, List<MappingEntry>> MappingsByDataType { get; } = new();
        public List<ReferencedFile> ReferencedFiles { get; } = new();
        
        public void AddMapping(string dataType, MappingEntry entry)
        {
            if (!MappingsByDataType.ContainsKey(dataType))
                MappingsByDataType[dataType] = new List<MappingEntry>();
            
            MappingsByDataType[dataType].Add(entry);
            MarkDirty();
        }

        public void RemoveMapping(string dataType, string propertyName)
        {
            if (MappingsByDataType.TryGetValue(dataType, out var entries))
            {
                entries.RemoveAll(e => e.PropertyName == propertyName);
                MarkDirty();
            }
        }

        public void ClearMappings(string dataType)
        {
            if (MappingsByDataType.ContainsKey(dataType))
            {
                MappingsByDataType[dataType].Clear();
                MarkDirty();
            }
        }

        public MappingConfig ToMappingConfig()
        {
            var config = new MappingConfig
            {
                SoftwareVersion = SoftwareVersion,
                MapVersion = "1.0",
                ImportMap = new List<MappingEntry>()
            };

            foreach (var kvp in MappingsByDataType)
            {
                config.ImportMap.AddRange(kvp.Value);
            }

            return config;
        }

        public static MapDocument FromMappingConfig(MappingConfig config, string? filePath = null)
        {
            var doc = new MapDocument
            {
                SoftwareVersion = config.SoftwareVersion,
                FilePath = filePath,
                IsDirty = false
            };

            foreach (var entry in config.ImportMap)
            {
                if (!doc.MappingsByDataType.ContainsKey(entry.TargetType))
                    doc.MappingsByDataType[entry.TargetType] = new List<MappingEntry>();
                
                doc.MappingsByDataType[entry.TargetType].Add(entry);
            }

            return doc;
        }

        private void MarkDirty()
        {
            IsDirty = true;
            DateModified = DateTime.Now;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Represents a reference to a sample file used for mapping.
    /// </summary>
    public class ReferencedFile
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Status { get; set; } = "Unknown";
        public DateTime LastAccessed { get; set; }
    }
}
```

**Create `Models/PropertyInfo.cs`**:
```csharp
namespace EasyAF.Modules.Map.Models
{
    /// <summary>
    /// Represents a property on a target data type that can be mapped.
    /// </summary>
    public class PropertyInfo
    {
        public string PropertyName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string PropertyType { get; set; } = "string";
        public string? Description { get; set; }
        public bool IsMapped { get; set; }
        public string? MappedColumn { get; set; }
    }
}
```

**Create `Models/ColumnInfo.cs`**:
```csharp
namespace EasyAF.Modules.Map.Models
{
    /// <summary>
    /// Represents a column discovered in a source file.
    /// </summary>
    public class ColumnInfo
    {
        public string ColumnName { get; set; } = string.Empty;
        public int ColumnIndex { get; set; }
        public string SourceTable { get; set; } = string.Empty;
        public bool IsMapped { get; set; }
        public string? MappedTo { get; set; }
        public int SampleValueCount { get; set; }
    }
}
```

**Update `MapModule.cs` - CreateNewDocument()**:
```csharp
public IDocument CreateNewDocument()
{
    Log.Information("Creating new mapping document");
    var document = new MapDocument();
    document.IsDirty = true; // New documents are dirty until saved
    return document;
}
```

**Update `MapModule.cs` - OpenDocument()**:
```csharp
public IDocument OpenDocument(string filePath)
{
    Log.Information("Opening mapping document from {FilePath}", filePath);
    
    if (!File.Exists(filePath))
        throw new FileNotFoundException($"Map file not found: {filePath}");

    var config = MappingConfig.Load(filePath);
    var document = MapDocument.FromMappingConfig(config, filePath);
    document.MapName = Path.GetFileNameWithoutExtension(filePath);
    
    return document;
}
```

**Update `MapModule.cs` - SaveDocument()**:
```csharp
public void SaveDocument(IDocument document, string filePath)
{
    if (document == null)
        throw new ArgumentNullException(nameof(document));

    if (document is not MapDocument mapDoc)
        throw new InvalidCastException("Document is not a MapDocument");

    Log.Information("Saving mapping document to {FilePath}", filePath);

    var config = mapDoc.ToMappingConfig();
    var json = JsonConvert.SerializeObject(config, Formatting.Indented);
    
    // Atomic write: temp file then replace
    var tempPath = filePath + ".tmp";
    File.WriteAllText(tempPath, json);
    File.Move(tempPath, filePath, overwrite: true);
    
    mapDoc.FilePath = filePath;
    mapDoc.IsDirty = false;
    
    Log.Information("Map saved successfully");
}
```

**Acceptance Criteria**:
- [ ] MapDocument implements IDocument correctly
- [ ] Can create new empty MapDocument
- [ ] Can save MapDocument to .ezmap file
- [ ] Can load .ezmap file into MapDocument
- [ ] IsDirty flag tracks changes
- [ ] PropertyChanged events fire correctly

---

### Task 14: Property Discovery Service
**Goal**: Discover properties from EasyAF.Data models via reflection.

**Create `Services/IPropertyDiscoveryService.cs`**:
```csharp
using System.Collections.Generic;
using EasyAF.Modules.Map.Models;

namespace EasyAF.Modules.Map.Services
{
    /// <summary>
    /// Service for discovering mappable properties from EasyAF data types.
    /// </summary>
    public interface IPropertyDiscoveryService
    {
        /// <summary>
        /// Gets all available data types that can be mapped.
        /// </summary>
        List<string> GetAvailableDataTypes();

        /// <summary>
        /// Gets all properties for a specific data type.
        /// </summary>
        List<PropertyInfo> GetPropertiesForType(string dataTypeName);

        /// <summary>
        /// Gets nested properties (e.g., LVCB.TripUnit).
        /// </summary>
        List<PropertyInfo> GetNestedProperties(string parentType, string nestedType);
    }
}
```

**Create `Services/PropertyDiscoveryService.cs`**:
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using EasyAF.Modules.Map.Models;
using Serilog;

namespace EasyAF.Modules.Map.Services
{
    /// <summary>
    /// Discovers mappable properties from EasyAF.Data models via reflection.
    /// </summary>
    public class PropertyDiscoveryService : IPropertyDiscoveryService
    {
        private readonly Dictionary<string, Type> _typeCache = new();
        private readonly Dictionary<string, List<PropertyInfo>> _propertyCache = new();

        public PropertyDiscoveryService()
        {
            // Cache available types from EasyAF.Data.Models
            DiscoverTypes();
        }

        public List<string> GetAvailableDataTypes()
        {
            return _typeCache.Keys.OrderBy(k => k).ToList();
        }

        public List<PropertyInfo> GetPropertiesForType(string dataTypeName)
        {
            // Check cache
            if (_propertyCache.TryGetValue(dataTypeName, out var cached))
                return cached;

            if (!_typeCache.TryGetValue(dataTypeName, out var type))
            {
                Log.Warning("Data type not found: {DataType}", dataTypeName);
                return new List<PropertyInfo>();
            }

            var properties = new List<PropertyInfo>();
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                properties.Add(new PropertyInfo
                {
                    PropertyName = prop.Name,
                    DataType = dataTypeName,
                    PropertyType = prop.PropertyType.Name,
                    Description = ExtractXmlDocumentation(prop)
                });
            }

            _propertyCache[dataTypeName] = properties;
            return properties;
        }

        public List<PropertyInfo> GetNestedProperties(string parentType, string nestedType)
        {
            // Handle nested types like "LVCB.TripUnit"
            var fullTypeName = $"{parentType}.{nestedType}";
            return GetPropertiesForType(nestedType);
        }

        private void DiscoverTypes()
        {
            var assembly = Assembly.Load("EasyAF.Data");
            var modelTypes = assembly.GetTypes()
                .Where(t => t.Namespace == "EasyAF.Data.Models" && t.IsClass && t.IsPublic)
                .ToList();

            foreach (var type in modelTypes)
            {
                _typeCache[type.Name] = type;
                Log.Debug("Discovered data type: {TypeName}", type.Name);
            }

            Log.Information("Discovered {Count} data types", _typeCache.Count);
        }

        private string? ExtractXmlDocumentation(PropertyInfo prop)
        {
            // Extract XML documentation comment if available
            // This is a simplified version; full implementation would load XML doc file
            try
            {
                // Look for <summary> tag in compiled XML docs
                // For now, return null - can enhance later
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
```

**Register service in `MapModule.Initialize()`**:
```csharp
public void Initialize(IUnityContainer container)
{
    Log.Information("Initializing Map module v{Version}", ModuleVersion);

    // Register services
    container.RegisterSingleton<IPropertyDiscoveryService, PropertyDiscoveryService>();
    
    Log.Information("Map module initialized successfully");
}
```

**Acceptance Criteria**:
- [ ] Service discovers all EasyAF.Data.Models types
- [ ] Can retrieve properties for Bus, LVCB, ArcFlash, etc.
- [ ] Properties include name and type information
- [ ] Service is singleton (cached)

---

### Task 15: Column Extraction Service
**Goal**: Extract column headers from CSV/Excel files.

**Create `Services/ColumnExtractionService.cs`**:
```csharp
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using ExcelDataReader;
using EasyAF.Modules.Map.Models;

namespace EasyAF.Modules.Map.Services
{
    /// <summary>
    /// Extracts column information from CSV and Excel files.
    /// </summary>
    public class ColumnExtractionService
    {
        public Dictionary<string, List<ColumnInfo>> ExtractColumns(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            
            return extension switch
            {
                ".csv" => ExtractFromCsv(filePath),
                ".xls" or ".xlsx" => ExtractFromExcel(filePath),
                _ => throw new NotSupportedException($"File type not supported: {extension}")
            };
        }

        private Dictionary<string, List<ColumnInfo>> ExtractFromCsv(string filePath)
        {
            var result = new Dictionary<string, List<ColumnInfo>>();
            var columns = new List<ColumnInfo>();

            using var reader = new StreamReader(filePath);
            var headerLine = reader.ReadLine();
            if (headerLine == null) return result;

            var headers = headerLine.Split(',');
            for (int i = 0; i < headers.Length; i++)
            {
                columns.Add(new ColumnInfo
                {
                    ColumnName = headers[i].Trim('"', ' '),
                    ColumnIndex = i,
                    SourceTable = "Sheet1"
                });
            }

            result["Sheet1"] = columns;
            return result;
        }

        private Dictionary<string, List<ColumnInfo>> ExtractFromExcel(string filePath)
        {
            var result = new Dictionary<string, List<ColumnInfo>>();

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = true
                }
            });

            foreach (DataTable table in dataSet.Tables)
            {
                var columns = new List<ColumnInfo>();
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    columns.Add(new ColumnInfo
                    {
                        ColumnName = table.Columns[i].ColumnName,
                        ColumnIndex = i,
                        SourceTable = table.TableName,
                        SampleValueCount = table.Rows.Count
                    });
                }
                result[table.TableName] = columns;
            }

            return result;
        }
    }
}
```

**Acceptance Criteria**:
- [ ] Can extract columns from CSV files
- [ ] Can extract columns from Excel files (all sheets)
- [ ] Returns table/sheet name with columns
- [ ] Handles quoted CSV headers

---

## PHASE 2: View Models & Commands

### Task 16: Map Document View Model
**Goal**: Create the main view model for the map document.

**Create `ViewModels/MapDocumentViewModel.cs`**:
```csharp
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using EasyAF.Core.Mvvm;
using EasyAF.Modules.Map.Models;
using EasyAF.Modules.Map.Services;
using Serilog;

namespace EasyAF.Modules.Map.ViewModels
{
    /// <summary>
    /// View model for the entire map document with tab management.
    /// </summary>
    public class MapDocumentViewModel : BindableBase
    {
        private readonly MapDocument _document;
        private readonly IPropertyDiscoveryService _propertyDiscovery;
        private object? _selectedTabContent;

        public MapDocumentViewModel(
            MapDocument document,
            IPropertyDiscoveryService propertyDiscovery)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _propertyDiscovery = propertyDiscovery ?? throw new ArgumentNullException(nameof(propertyDiscovery));

            // Initialize tabs
            TabHeaders = new ObservableCollection<TabHeaderInfo>();
            InitializeTabs();
        }

        public MapDocument Document => _document;
        public ObservableCollection<TabHeaderInfo> TabHeaders { get; }

        public object? SelectedTabContent
        {
            get => _selectedTabContent;
            set => SetProperty(ref _selectedTabContent, value);
        }

        private void InitializeTabs()
        {
            // Summary tab (always first)
            TabHeaders.Add(new TabHeaderInfo
            {
                Header = "Summary",
                Status = string.Empty,
                ViewModel = new MapSummaryViewModel(_document, _propertyDiscovery)
            });

            // Data type tabs (dynamic)
            var dataTypes = _propertyDiscovery.GetAvailableDataTypes();
            foreach (var dataType in dataTypes)
            {
                TabHeaders.Add(new TabHeaderInfo
                {
                    Header = dataType,
                    Status = "?", // Empty circle = unmapped
                    ViewModel = new DataTypeMappingViewModel(_document, dataType, _propertyDiscovery)
                });
            }

            // Select summary tab
            if (TabHeaders.Count > 0)
                SelectedTabContent = TabHeaders[0].ViewModel;
        }

        public void UpdateTabStatus(string dataType, MappingStatus status)
        {
            var tab = TabHeaders.FirstOrDefault(t => t.Header == dataType);
            if (tab == null) return;

            tab.Status = status switch
            {
                MappingStatus.Unmapped => "?",
                MappingStatus.Partial => "?",
                MappingStatus.Complete => "?",
                _ => "?"
            };
        }
    }

    public class TabHeaderInfo : BindableBase
    {
        private string _header = string.Empty;
        private string _status = string.Empty;

        public string Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public object? ViewModel { get; set; }
    }

    public enum MappingStatus
    {
        Unmapped,
        Partial,
        Complete
    }
}
```

**Acceptance Criteria**:
- [ ] Creates tab for summary
- [ ] Creates tab for each data type
- [ ] Tab selection works
- [ ] Status indicators update

---

### Task 17: Map Summary View Model
**Goal**: Create view model for the summary tab.

**Create `ViewModels/MapSummaryViewModel.cs`**:
```csharp
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using EasyAF.Core.Mvvm;
using EasyAF.Modules.Map.Models;
using EasyAF.Modules.Map.Services;
using Microsoft.Win32;

namespace EasyAF.Modules.Map.ViewModels
{
    /// <summary>
    /// View model for the map summary tab showing metadata and data type properties.
    /// </summary>
    public class MapSummaryViewModel : BindableBase
    {
        private readonly MapDocument _document;
        private readonly IPropertyDiscoveryService _propertyDiscovery;
        private ReferencedFile? _selectedFile;

        public MapSummaryViewModel(
            MapDocument document,
            IPropertyDiscoveryService propertyDiscovery)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _propertyDiscovery = propertyDiscovery ?? throw new ArgumentNullException(nameof(propertyDiscovery));

            // Initialize collections
            DataTypeProperties = new ObservableCollection<DataTypePropertySummary>();
            AvailableTables = new ObservableCollection<string>();

            // Commands
            AddFileCommand = new RelayCommand(ExecuteAddFile);
            RemoveFileCommand = new RelayCommand(ExecuteRemoveFile, () => SelectedFile != null);
            BrowseFileCommand = new RelayCommand(ExecuteBrowseFile);

            // Initialize data type summaries
            InitializeDataTypeSummaries();
        }

        // Metadata bindings
        public string MapName
        {
            get => _document.MapName;
            set { _document.MapName = value; OnPropertyChanged(); }
        }

        public string SoftwareVersion
        {
            get => _document.SoftwareVersion;
            set { _document.SoftwareVersion = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _document.Description;
            set { _document.Description = value; OnPropertyChanged(); }
        }

        public DateTime DateModified => _document.DateModified;

        // Collections
        public ObservableCollection<ReferencedFile> ReferencedFiles => 
            new ObservableCollection<ReferencedFile>(_document.ReferencedFiles);

        public ReferencedFile? SelectedFile
        {
            get => _selectedFile;
            set { SetProperty(ref _selectedFile, value); (RemoveFileCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        public ObservableCollection<DataTypePropertySummary> DataTypeProperties { get; }
        public ObservableCollection<string> AvailableTables { get; }

        // Commands
        public ICommand AddFileCommand { get; }
        public ICommand RemoveFileCommand { get; }
        public ICommand BrowseFileCommand { get; }

        private void InitializeDataTypeSummaries()
        {
            var dataTypes = _propertyDiscovery.GetAvailableDataTypes();
            foreach (var dataType in dataTypes)
            {
                var properties = _propertyDiscovery.GetPropertiesForType(dataType);
                var mappedCount = _document.MappingsByDataType.TryGetValue(dataType, out var mappings) 
                    ? mappings.Count 
                    : 0;

                DataTypeProperties.Add(new DataTypePropertySummary
                {
                    TypeName = dataType,
                    FieldsAvailable = properties.Count,
                    FieldsMapped = mappedCount,
                    StatusColor = mappedCount == 0 ? "Red" : 
                                  mappedCount < properties.Count ? "Orange" : "Green"
                });
            }
        }

        private void ExecuteAddFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Data Files (*.csv;*.xls;*.xlsx)|*.csv;*.xls;*.xlsx",
                Title = "Add Sample File"
            };

            if (dialog.ShowDialog() == true)
            {
                _document.ReferencedFiles.Add(new ReferencedFile
                {
                    FileName = System.IO.Path.GetFileName(dialog.FileName),
                    FilePath = dialog.FileName,
                    Status = "Available",
                    LastAccessed = DateTime.Now
                });
                OnPropertyChanged(nameof(ReferencedFiles));
            }
        }

        private void ExecuteRemoveFile()
        {
            if (SelectedFile != null)
            {
                _document.ReferencedFiles.Remove(SelectedFile);
                OnPropertyChanged(nameof(ReferencedFiles));
            }
        }

        private void ExecuteBrowseFile()
        {
            ExecuteAddFile();
        }
    }

    public class DataTypePropertySummary : BindableBase
    {
        private string _typeName = string.Empty;
        private int _fieldsAvailable;
        private int _fieldsMapped;
        private string _statusColor = "Gray";
        private string? _sourceTable;

        public string TypeName
        {
            get => _typeName;
            set => SetProperty(ref _typeName, value);
        }

        public int FieldsAvailable
        {
            get => _fieldsAvailable;
            set => SetProperty(ref _fieldsAvailable, value);
        }

        public int FieldsMapped
        {
            get => _fieldsMapped;
            set => SetProperty(ref _fieldsMapped, value);
        }

        public string StatusColor
        {
            get => _statusColor;
            set => SetProperty(ref _statusColor, value);
        }

        public string? SourceTable
        {
            get => _sourceTable;
            set => SetProperty(ref _sourceTable, value);
        }
    }
}
```

**Acceptance Criteria**:
- [ ] Displays map metadata
- [ ] Shows referenced files
- [ ] Can add/remove files
- [ ] Shows data type summaries

---

## PHASE 3: Views & UI Implementation

### Task 18: Map Document View
**Goal**: Create the main document view with tab control.

**Create `Views/MapDocumentView.xaml`**:
```xaml
<UserControl x:Class="EasyAF.Modules.Map.Views.MapDocumentView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:views="clr-namespace:EasyAF.Modules.Map.Views">
    
    <Grid Background="{DynamicResource PrimaryBackgroundBrush}">
        <TabControl ItemsSource="{Binding TabHeaders}"
                    SelectedValue="{Binding SelectedTabContent}"
                    BorderThickness="0">
            
            <!-- Tab Header Template -->
            <TabControl.ItemTemplate>
                <DataTemplate>
                    <StackPanel Orientation="Horizontal">
                        <TextBlock Text="{Binding Status}" 
                                   Margin="0,0,4,0"
                                   FontFamily="Segoe MDL2 Assets"
                                   VerticalAlignment="Center"/>
                        <TextBlock Text="{Binding Header}"
                                   VerticalAlignment="Center"/>
                    </StackPanel>
                </DataTemplate>
            </TabControl.ItemTemplate>
            
            <!-- Tab Content Template -->
            <TabControl.ContentTemplate>
                <DataTemplate>
                    <ContentControl Content="{Binding ViewModel}">
                        <ContentControl.Resources>
                            <!-- Map ViewModels to Views -->
                            <DataTemplate DataType="{x:Type vm:MapSummaryViewModel}">
                                <views:MapSummaryView/>
                            </DataTemplate>
                            <DataTemplate DataType="{x:Type vm:DataTypeMappingViewModel}">
                                <views:DataTypeMappingView/>
                            </DataTemplate>
                        </ContentControl.Resources>
                    </ContentControl>
                </DataTemplate>
            </TabControl.ContentTemplate>
            
        </TabControl>
    </Grid>
</UserControl>
```

**Create `Views/MapDocumentView.xaml.cs`**:
```csharp
using System.Windows.Controls;

namespace EasyAF.Modules.Map.Views
{
    public partial class MapDocumentView : UserControl
    {
        public MapDocumentView()
        {
            InitializeComponent();
        }
    }
}
```

**Acceptance Criteria**:
- [ ] Tab control displays
- [ ] Tab headers show status icons
- [ ] Tab content switches correctly
- [ ] Uses theme brushes

---

### Task 19: Map Summary View
**Goal**: Create the summary tab UI.

**Create `Views/MapSummaryView.xaml`**:
```xaml
<UserControl x:Class="EasyAF.Modules.Map.Views.MapSummaryView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <ScrollViewer VerticalScrollBarVisibility="Auto"
                  Background="{DynamicResource PrimaryBackgroundBrush}">
        <Grid Margin="20">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/> <!-- Metadata -->
                <RowDefinition Height="Auto"/> <!-- Files -->
                <RowDefinition Height="*"/>    <!-- Properties -->
            </Grid.RowDefinitions>
            
            <!-- Metadata Section -->
            <GroupBox Grid.Row="0" Header="Map Information" 
                      Margin="0,0,0,16"
                      BorderBrush="{DynamicResource ControlBorderBrush}">
                <Grid Margin="8">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="140"/>
                        <ColumnDefinition Width="*"/>
                    </Grid.ColumnDefinitions>
                    <Grid.RowDefinitions>
                        <RowDefinition Height="32"/>
                        <RowDefinition Height="32"/>
                        <RowDefinition Height="32"/>
                        <RowDefinition Height="80"/>
                    </Grid.RowDefinitions>
                    
                    <TextBlock Grid.Row="0" Grid.Column="0" 
                               Text="Map Name:" 
                               VerticalAlignment="Center"
                               Foreground="{DynamicResource TextPrimaryBrush}"/>
                    <TextBox Grid.Row="0" Grid.Column="1" 
                             Text="{Binding MapName, UpdateSourceTrigger=PropertyChanged}"
                             VerticalAlignment="Center"/>
                    
                    <TextBlock Grid.Row="1" Grid.Column="0" 
                               Text="Software Version:" 
                               VerticalAlignment="Center"
                               Foreground="{DynamicResource TextPrimaryBrush}"/>
                    <TextBox Grid.Row="1" Grid.Column="1" 
                             Text="{Binding SoftwareVersion}"
                             VerticalAlignment="Center"/>
                    
                    <TextBlock Grid.Row="2" Grid.Column="0" 
                               Text="Date Modified:" 
                               VerticalAlignment="Center"
                               Foreground="{DynamicResource TextPrimaryBrush}"/>
                    <TextBlock Grid.Row="2" Grid.Column="1" 
                               Text="{Binding DateModified, StringFormat='{}{0:yyyy-MM-dd HH:mm}'}"
                               VerticalAlignment="Center"
                               Foreground="{DynamicResource TextSecondaryBrush}"/>
                    
                    <TextBlock Grid.Row="3" Grid.Column="0" 
                               Text="Description:" 
                               VerticalAlignment="Top" Margin="0,4,0,0"
                               Foreground="{DynamicResource TextPrimaryBrush}"/>
                    <TextBox Grid.Row="3" Grid.Column="1" 
                             Text="{Binding Description}"
                             TextWrapping="Wrap"
                             AcceptsReturn="True"
                             VerticalScrollBarVisibility="Auto"/>
                </Grid>
            </GroupBox>
            
            <!-- Referenced Files Section -->
            <GroupBox Grid.Row="1" Header="Referenced Sample Files" 
                      Margin="0,0,0,16"
                      BorderBrush="{DynamicResource ControlBorderBrush}">
                <Grid Margin="8">
                    <Grid.RowDefinitions>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="150"/>
                    </Grid.RowDefinitions>
                    
                    <ToolBar Grid.Row="0" Background="Transparent">
                        <Button Command="{Binding AddFileCommand}" 
                                Content="Add File"/>
                        <Button Command="{Binding RemoveFileCommand}" 
                                Content="Remove"/>
                    </ToolBar>
                    
                    <DataGrid Grid.Row="1" 
                              ItemsSource="{Binding ReferencedFiles}"
                              SelectedItem="{Binding SelectedFile}"
                              AutoGenerateColumns="False"
                              IsReadOnly="True"
                              Background="{DynamicResource ControlBackgroundBrush}"
                              Foreground="{DynamicResource TextPrimaryBrush}">
                        <DataGrid.Columns>
                            <DataGridTextColumn Header="File Name" 
                                                Binding="{Binding FileName}" 
                                                Width="200"/>
                            <DataGridTextColumn Header="Path" 
                                                Binding="{Binding FilePath}" 
                                                Width="*"/>
                            <DataGridTextColumn Header="Status" 
                                                Binding="{Binding Status}" 
                                                Width="80"/>
                        </DataGrid.Columns>
                    </DataGrid>
                </Grid>
            </GroupBox>
            
            <!-- Data Type Properties Table -->
            <GroupBox Grid.Row="2" Header="Data Type Mapping Status"
                      BorderBrush="{DynamicResource ControlBorderBrush}">
                <DataGrid ItemsSource="{Binding DataTypeProperties}"
                          AutoGenerateColumns="False"
                          IsReadOnly="True"
                          Background="{DynamicResource ControlBackgroundBrush}"
                          Foreground="{DynamicResource TextPrimaryBrush}">
                    <DataGrid.Columns>
                        <DataGridTextColumn Header="Data Type" 
                                            Binding="{Binding TypeName}" 
                                            Width="150"/>
                        <DataGridTemplateColumn Header="Status" Width="60">
                            <DataGridTemplateColumn.CellTemplate>
                                <DataTemplate>
                                    <Ellipse Width="12" Height="12" 
                                             Fill="{Binding StatusColor}"/>
                                </DataTemplate>
                            </DataGridTemplateColumn.CellTemplate>
                        </DataGridTemplateColumn>
                        <DataGridTextColumn Header="Available" 
                                            Binding="{Binding FieldsAvailable}" 
                                            Width="80"/>
                        <DataGridTextColumn Header="Mapped" 
                                            Binding="{Binding FieldsMapped}" 
                                            Width="80"/>
                    </DataGrid.Columns>
                </DataGrid>
            </GroupBox>
        </Grid>
    </ScrollViewer>
</UserControl>
```

**Create `Views/MapSummaryView.xaml.cs`**:
```csharp
using System.Windows.Controls;

namespace EasyAF.Modules.Map.Views
{
    public partial class MapSummaryView : UserControl
    {
        public MapSummaryView()
        {
            InitializeComponent();
        }
    }
}
```

**Acceptance Criteria**:
- [ ] Metadata fields editable
- [ ] File list functional
- [ ] Data type table displays
- [ ] All theme bindings work

---

## PHASE 4: Advanced Features

### Task 20: Auto-Mapping Intelligence
**Goal**: Implement smart mapping suggestions.

**Create `Services/IAutoMappingService.cs`**:
```csharp
using System.Collections.Generic;
using EasyAF.Modules.Map.Models;

namespace EasyAF.Modules.Map.Services
{
    public interface IAutoMappingService
    {
        List<MappingSuggestion> GenerateSuggestions(
            List<ColumnInfo> sourceColumns,
            List<PropertyInfo> targetProperties);
    }

    public class MappingSuggestion
    {
        public string SourceColumn { get; set; } = string.Empty;
        public string TargetProperty { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public string Strategy { get; set; } = string.Empty;
    }
}
```

**Create `Services/AutoMappingService.cs`**:
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using EasyAF.Modules.Map.Models;

namespace EasyAF.Modules.Map.Services
{
    public class AutoMappingService : IAutoMappingService
    {
        public List<MappingSuggestion> GenerateSuggestions(
            List<ColumnInfo> sourceColumns,
            List<PropertyInfo> targetProperties)
        {
            var suggestions = new List<MappingSuggestion>();

            // Strategy 1: Exact match
            suggestions.AddRange(ExactMatch(sourceColumns, targetProperties));

            // Strategy 2: Normalized match (remove spaces, case-insensitive)
            suggestions.AddRange(NormalizedMatch(sourceColumns, targetProperties));

            // Strategy 3: Fuzzy match (Levenshtein distance)
            suggestions.AddRange(FuzzyMatch(sourceColumns, targetProperties));

            return suggestions
                .OrderByDescending(s => s.Confidence)
                .ToList();
        }

        private IEnumerable<MappingSuggestion> ExactMatch(
            List<ColumnInfo> sources,
            List<PropertyInfo> targets)
        {
            foreach (var source in sources)
            {
                var target = targets.FirstOrDefault(t => 
                    t.PropertyName.Equals(source.ColumnName, StringComparison.OrdinalIgnoreCase));

                if (target != null)
                {
                    yield return new MappingSuggestion
                    {
                        SourceColumn = source.ColumnName,
                        TargetProperty = target.PropertyName,
                        Confidence = 1.0,
                        Strategy = "Exact Match"
                    };
                }
            }
        }

        private IEnumerable<MappingSuggestion> NormalizedMatch(
            List<ColumnInfo> sources,
            List<PropertyInfo> targets)
        {
            foreach (var source in sources)
            {
                var normalizedSource = Normalize(source.ColumnName);

                foreach (var target in targets)
                {
                    var normalizedTarget = Normalize(target.PropertyName);

                    if (normalizedSource == normalizedTarget)
                    {
                        yield return new MappingSuggestion
                        {
                            SourceColumn = source.ColumnName,
                            TargetProperty = target.PropertyName,
                            Confidence = 0.9,
                            Strategy = "Normalized Match"
                        };
                    }
                }
            }
        }

        private IEnumerable<MappingSuggestion> FuzzyMatch(
            List<ColumnInfo> sources,
            List<PropertyInfo> targets)
        {
            foreach (var source in sources)
            {
                foreach (var target in targets)
                {
                    var similarity = CalculateSimilarity(source.ColumnName, target.PropertyName);
                    if (similarity > 0.7) // 70% threshold
                    {
                        yield return new MappingSuggestion
                        {
                            SourceColumn = source.ColumnName,
                            TargetProperty = target.PropertyName,
                            Confidence = similarity,
                            Strategy = "Fuzzy Match"
                        };
                    }
                }
            }
        }

        private string Normalize(string input)
        {
            return input.Replace(" ", "")
                        .Replace("_", "")
                        .Replace("-", "")
                        .ToLowerInvariant();
        }

        private double CalculateSimilarity(string s1, string s2)
        {
            s1 = Normalize(s1);
            s2 = Normalize(s2);

            if (s1 == s2) return 1.0;

            int distance = LevenshteinDistance(s1, s2);
            int maxLength = Math.Max(s1.Length, s2.Length);

            return 1.0 - (double)distance / maxLength;
        }

        private int LevenshteinDistance(string s1, string s2)
        {
            int[,] d = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                d[i, 0] = i;

            for (int j = 0; j <= s2.Length; j++)
                d[0, j] = j;

            for (int j = 1; j <= s2.Length; j++)
            {
                for (int i = 1; i <= s1.Length; i++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[s1.Length, s2.Length];
        }
    }
}
```

**Acceptance Criteria**:
- [ ] Exact matches return 100% confidence
- [ ] Normalized matches handle spaces/case
- [ ] Fuzzy matching works for similar names
- [ ] Results ranked by confidence

---

## Testing Plan

### Unit Testing
- [ ] MapDocument serialization/deserialization
- [ ] Property discovery for all data types
- [ ] Column extraction from CSV
- [ ] Column extraction from Excel
- [ ] Auto-mapping algorithms

### Integration Testing
- [ ] Create new map ? save ? load
- [ ] Add sample file ? extract columns
- [ ] Make mappings ? validate ? save
- [ ] Load existing .ezmap ? edit ? save

### UI Testing
- [ ] All tabs display correctly
- [ ] Theme switching works
- [ ] Status indicators update
- [ ] Commands enable/disable properly

## Implementation Notes

### Keep Files Manageable
- Each ViewModel < 300 lines
- Each View < 200 lines of XAML
- Services focused on single responsibility
- Split large services into helpers

### Cross-Module Edits
None required! The mapping module is fully self-contained and only references:
- EasyAF.Core (contracts)
- EasyAF.Data (reflection only)
- EasyAF.Import (serialization)

### Rollback Plan
1. Delete `modules/EasyAF.Modules.Map/` folder
2. Remove module from `ModuleLoader` discovery
3. No other files modified

## Future Enhancements (Not in Scope)
- Undo/Redo system (Phase 4)
- Sample data preview (Phase 4)  
- Historical pattern learning (Phase 4)
- Drag-and-drop mapping (Phase 4)
- Multi-file mapping (Phase 5)

---

## Success Criteria
? Can create new .ezmap file  
? Can load existing .ezmap file  
? Can add sample files and extract columns  
? Can view all available data types and properties  
? Can manually create column-to-property mappings  
? Auto-mapping suggests reasonable matches  
? Can save mappings to .ezmap file  
? Integrates with Shell document management  
? Full theme support (Light/Dark)  
? No code-behind logic (MVVM strict)
