using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EasyAF.Import
{
    /// <summary>
    /// Defines a contract for accessing mapping configuration data in a read-only manner.
    /// </summary>
    /// <remarks>
    /// This interface allows for both mutable (<see cref="MappingConfig"/>) and immutable 
    /// (<see cref="ImmutableMappingConfig"/>) implementations, enabling safe sharing across threads.
    /// </remarks>
    public interface IMappingConfig
    {
        /// <summary>
        /// Gets the software version this mapping configuration is compatible with.
        /// </summary>
        string SoftwareVersion { get; }
        
        /// <summary>
        /// Gets the optional version identifier for this mapping configuration itself.
        /// </summary>
        string? MapVersion { get; }
        
        /// <summary>
        /// Gets the collection of mapping entries defining how source columns map to target properties.
        /// </summary>
        IReadOnlyList<MappingEntry> ImportMap { get; }
    }

    /// <summary>
    /// Defines the severity level for a mapping entry validation or processing issue.
    /// </summary>
    public enum MappingSeverity 
    { 
        /// <summary>Informational only; does not affect import.</summary>
        Info, 
        
        /// <summary>Warning condition; import continues but logs a warning.</summary>
        Warning, 
        
        /// <summary>Error condition; import fails if this mapping cannot be satisfied.</summary>
        Error 
    }

    /// <summary>
    /// Represents a mapping configuration that defines how data from an import file 
    /// (CSV, Excel, etc.) is mapped to properties in EasyAF data models.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Purpose:</strong> MappingConfig is the core data structure for the EasyAF import system.
    /// It defines the relationship between source file columns (by header name) and destination 
    /// model properties (by TargetType.PropertyName), enabling flexible data import from various formats.
    /// </para>
    /// <para>
    /// <strong>File Format:</strong> Mapping configurations are stored as JSON files with a .ezmap extension.
    /// The JSON structure must use PascalCase property names to ensure consistency across the application.
    /// </para>
    /// <para>
    /// <strong>Validation:</strong> Before use, call <see cref="Validate"/> to check for:
    /// - Duplicate mappings (same TargetType.PropertyName)
    /// - Missing required fields (blank TargetType/PropertyName/ColumnHeader)
    /// - Conflicting required mappings
    /// </para>
    /// <para>
    /// <strong>Immutability:</strong> After validation, use <see cref="ToImmutable"/> to create a 
    /// thread-safe, immutable snapshot suitable for concurrent access during import operations.
    /// </para>
    /// <para>
    /// <strong>PascalCase Enforcement:</strong> The Load() method enforces PascalCase naming to maintain
    /// consistency with C# conventions and prevent confusion from case-sensitive JSON deserialization.
    /// </para>
    /// <para>
    /// <strong>Thread Safety:</strong> MappingConfig is NOT thread-safe. For concurrent access, 
    /// create an <see cref="ImmutableMappingConfig"/> via <see cref="ToImmutable"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para><strong>Example 1: Loading and validating a mapping configuration</strong></para>
    /// <code>
    /// // Load from .ezmap file
    /// var config = MappingConfig.Load("mappings/bus-import.ezmap");
    /// 
    /// // Validate before use
    /// var validation = config.Validate();
    /// if (validation.HasErrors)
    /// {
    ///     foreach (var error in validation.Errors)
    ///         Console.WriteLine($"ERROR: {error}");
    ///     return;
    /// }
    /// 
    /// // Create immutable version for import
    /// var immutableConfig = config.ToImmutable();
    /// </code>
    /// 
    /// <para><strong>Example 2: Creating a mapping configuration programmatically</strong></para>
    /// <code>
    /// var config = new MappingConfig
    /// {
    ///     SoftwareVersion = "3.0.0",
    ///     MapVersion = "1.0",
    ///     ImportMap = new List&lt;MappingEntry&gt;
    ///     {
    ///         new MappingEntry
    ///         {
    ///             TargetType = "Bus",
    ///             PropertyName = "Id",
    ///             ColumnHeader = "Bus ID",
    ///             Required = true,
    ///             Severity = MappingSeverity.Error
    ///         },
    ///         new MappingEntry
    ///         {
    ///             TargetType = "Bus",
    ///             PropertyName = "Voltage",
    ///             ColumnHeader = "Nominal kV",
    ///             Required = false,
    ///             DefaultValue = "0.48",
    ///             Aliases = new[] { "Voltage", "kV", "Nom_Voltage" }
    ///         }
    ///     }
    /// };
    /// </code>
    /// 
    /// <para><strong>Example 3: Sample .ezmap file structure</strong></para>
    /// <code>
    /// {
    ///   "SoftwareVersion": "3.0.0",
    ///   "MapVersion": "1.0",
    ///   "ImportMap": [
    ///     {
    ///       "TargetType": "Bus",
    ///       "PropertyName": "Id",
    ///       "ColumnHeader": "Bus ID",
    ///       "Required": true,
    ///       "Severity": "Error",
    ///       "Aliases": ["BusId", "Bus_ID"]
    ///     },
    ///     {
    ///       "TargetType": "ArcFlash",
    ///       "PropertyName": "Scenario",
    ///       "ColumnHeader": "Fault Scenario",
    ///       "Required": false,
    ///       "Severity": "Warning",
    ///       "DefaultValue": "Main-Min"
    ///     }
    ///   ]
    /// }
    /// </code>
    /// </example>
    public class MappingConfig : IMappingConfig
    {
        /// <summary>
        /// Gets or sets the software version this mapping is compatible with (e.g., "3.0.0").
        /// </summary>
        /// <remarks>
        /// Used to ensure mapping files are compatible with the current EasyAF version.
        /// Validation may reject mappings with incompatible version numbers.
        /// </remarks>
        public string SoftwareVersion { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets an optional version identifier for this mapping configuration itself.
        /// </summary>
        /// <remarks>
        /// Useful for tracking changes to mapping configurations over time.
        /// Not currently used in validation but available for custom workflows.
        /// </remarks>
        public string? MapVersion { get; set; }
        
        /// <summary>
        /// Gets or sets the collection of mapping entries that define the column-to-property mappings.
        /// </summary>
        /// <remarks>
        /// Each entry maps a source file column (by ColumnHeader) to a destination property
        /// (by TargetType.PropertyName). Multiple entries can target the same model type.
        /// </remarks>
        [JsonProperty("ImportMap")] 
        public List<MappingEntry> ImportMap { get; set; } = new();
        
        // Explicit interface implementation for read-only access
        IReadOnlyList<MappingEntry> IMappingConfig.ImportMap => ImportMap;

        private static readonly Regex PascalCaseRegex = new("^[A-Z][A-Za-z0-9]*$", RegexOptions.Compiled);

        /// <summary>
        /// Loads a MappingConfig from a JSON file (.ezmap) with strict PascalCase enforcement.
        /// </summary>
        /// <param name="path">The file path to the .ezmap JSON file.</param>
        /// <returns>A validated MappingConfig instance.</returns>
        /// <exception cref="InvalidDataException">
        /// Thrown when:
        /// - The file contains invalid JSON
        /// - Property names are not in PascalCase (e.g., "columnHeader" instead of "ColumnHeader")
        /// - The JSON structure doesn't match the expected schema
        /// </exception>
        /// <remarks>
        /// <para>
        /// <strong>PascalCase Enforcement:</strong> All JSON property names must start with an uppercase letter
        /// and contain only alphanumeric characters (e.g., "ImportMap", "TargetType", "ColumnHeader").
        /// Properties starting with "$" (like "$schema") are allowed and ignored.
        /// </para>
        /// <para>
        /// <strong>Normalization:</strong> After loading, the config is automatically normalized
        /// (whitespace trimmed from key fields). Call <see cref="Validate"/> separately to check for errors.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// try
        /// {
        ///     var config = MappingConfig.Load("mappings/arcflash-import.ezmap");
        ///     
        ///     var validation = config.Validate();
        ///     if (validation.HasErrors)
        ///     {
        ///         // Handle validation errors
        ///     }
        /// }
        /// catch (InvalidDataException ex)
        /// {
        ///     Console.WriteLine($"Failed to load mapping: {ex.Message}");
        /// }
        /// </code>
        /// </example>
        public static MappingConfig Load(string path)
        {
            var json = File.ReadAllText(path);
            // Pre-parse for PascalCase enforcement
            var invalid = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            try
            {
                var token = JToken.Parse(json);
                Walk(token, invalid);
            }
            catch (JsonException jx)
            {
                throw new InvalidDataException($"Mapping JSON parse error: {jx.Message}", jx);
            }
            if (invalid.Count > 0)
            {
                throw new InvalidDataException("Invalid (non-PascalCase) mapping JSON field names: " + string.Join(", ", invalid.OrderBy(s => s)));
            }

            var cfg = JsonConvert.DeserializeObject<MappingConfig>(json) ?? new MappingConfig();
            cfg.Normalize();
            return cfg;
        }

        /// <summary>
        /// Creates an immutable, thread-safe snapshot of this mapping configuration.
        /// </summary>
        /// <returns>An <see cref="ImmutableMappingConfig"/> instance.</returns>
        /// <exception cref="InvalidDataException">
        /// Thrown if the configuration has validation errors (<see cref="Validate"/> returns errors).
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method performs a deep clone of all mapping entries to prevent external mutation.
        /// The resulting immutable config is safe to share across threads during import operations.
        /// </para>
        /// <para>
        /// <strong>Validation:</strong> This method calls <see cref="Validate"/> internally.
        /// If validation fails, an exception is thrown with all error messages concatenated.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var config = MappingConfig.Load("mapping.ezmap");
        /// 
        /// // Create immutable version for concurrent import operations
        /// var immutableConfig = config.ToImmutable();
        /// 
        /// // Now safe to use across multiple threads
        /// Parallel.ForEach(files, file =>
        /// {
        ///     var importer = new CsvImporter(immutableConfig);
        ///     importer.Import(file);
        /// });
        /// </code>
        /// </example>
        public ImmutableMappingConfig ToImmutable()
        {
            var validation = Validate();
            if (validation.HasErrors)
                throw new InvalidDataException("Cannot create immutable mapping: validation errors: " + string.Join("; ", validation.Errors));
            // Clone entries to prevent later external mutation of list items
            var cloned = ImportMap.Select(e => new MappingEntry
            {
                TargetType = e.TargetType,
                PropertyName = e.PropertyName,
                ColumnHeader = e.ColumnHeader,
                Required = e.Required,
                Aliases = e.Aliases == null ? null : e.Aliases.ToArray(),
                Severity = e.Severity,
                DefaultValue = e.DefaultValue
            }).ToList();
            return new ImmutableMappingConfig(SoftwareVersion, MapVersion, cloned);
        }

        /// <summary>
        /// Recursively walks the JSON token tree to find non-PascalCase property names.
        /// </summary>
        /// <param name="token">The JSON token to examine.</param>
        /// <param name="invalid">Collection to accumulate invalid property names.</param>
        private static void Walk(JToken token, HashSet<string> invalid)
        {
            if (token.Type == JTokenType.Object)
            {
                foreach (var prop in ((JObject)token).Properties())
                {
                    var name = prop.Name;
                    // Allow schema properties starting with $
                    if (!name.StartsWith("$") && !PascalCaseRegex.IsMatch(name)) 
                        invalid.Add(name);
                    Walk(prop.Value, invalid);
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                foreach (var child in (JArray)token) 
                    Walk(child, invalid);
            }
        }

        /// <summary>
        /// Normalizes the mapping configuration by trimming whitespace from all key fields.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Modifies the configuration in-place, trimming:
        /// - TargetType
        /// - PropertyName  
        /// - ColumnHeader
        /// </para>
        /// <para>
        /// This is automatically called by <see cref="Load"/> but can be called manually
        /// if you construct a MappingConfig programmatically.
        /// </para>
        /// </remarks>
        public void Normalize()
        {
            foreach (var e in ImportMap)
            {
                e.TargetType = e.TargetType?.Trim() ?? string.Empty;
                e.PropertyName = e.PropertyName?.Trim() ?? string.Empty;
                e.ColumnHeader = e.ColumnHeader?.Trim() ?? string.Empty;
            }
        }

        /// <summary>
        /// Validates the mapping configuration, checking for common errors and issues.
        /// </summary>
        /// <returns>
        /// A <see cref="MappingValidationResult"/> containing any warnings or errors found.
        /// </returns>
        /// <remarks>
        /// <para>
        /// <strong>Validation Rules:</strong>
        /// </para>
        /// <list type="bullet">
        /// <item><description><strong>Duplicates (Warning):</strong> Multiple entries mapping to the same TargetType.PropertyName (first is used)</description></item>
        /// <item><description><strong>Blank Fields (Error):</strong> Any entry with blank TargetType, PropertyName, or ColumnHeader</description></item>
        /// <item><description><strong>Required Duplicates (Error):</strong> A required mapping that has duplicates (ambiguous which to use)</description></item>
        /// </list>
        /// <para>
        /// <strong>Thread Safety:</strong> This method is safe to call concurrently as it doesn't modify state.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var config = MappingConfig.Load("mapping.ezmap");
        /// var result = config.Validate();
        /// 
        /// // Check for errors
        /// if (result.HasErrors)
        /// {
        ///     Console.WriteLine("Validation failed:");
        ///     foreach (var error in result.Errors)
        ///         Console.WriteLine($"  ERROR: {error}");
        ///     return;
        /// }
        /// 
        /// // Log warnings but continue
        /// if (result.Warnings.Any())
        /// {
        ///     Console.WriteLine("Validation warnings:");
        ///     foreach (var warning in result.Warnings)
        ///         Console.WriteLine($"  WARN: {warning}");
        /// }
        /// 
        /// // Safe to use
        /// var immutableConfig = config.ToImmutable();
        /// </code>
        /// </example>
        public MappingValidationResult Validate()
        {
            var result = new MappingValidationResult();
            
            // Check for duplicate mappings (same TargetType.PropertyName)
            var keyGroups = ImportMap
                .GroupBy(e => (e.TargetType.Trim().ToLowerInvariant(), e.PropertyName.Trim().ToLowerInvariant()))
                .ToList();
                
            foreach (var g in keyGroups.Where(g => g.Count() > 1))
                result.Warnings.Add($"Duplicate mapping entries for {g.Key.Item1}.{g.Key.Item2} (using first occurrence).");

            // Check for blank required fields
            foreach (var m in ImportMap.Where(m => string.IsNullOrWhiteSpace(m.TargetType) 
                || string.IsNullOrWhiteSpace(m.PropertyName) 
                || string.IsNullOrWhiteSpace(m.ColumnHeader)))
            {
                result.Errors.Add("Entry has blank TargetType/PropertyName/ColumnHeader – invalid.");
            }

            // Check for required mappings that are duplicated (ambiguous)
            foreach (var req in ImportMap.Where(e => e.Required))
            {
                var key = (req.TargetType.Trim().ToLowerInvariant(), req.PropertyName.Trim().ToLowerInvariant());
                if (keyGroups.First(g => g.Key == key).Count() > 1)
                    result.Errors.Add($"Required mapping duplicated: {req.TargetType}.{req.PropertyName}");
            }
            
            return result;
        }
    }

    /// <summary>
    /// Represents an immutable, thread-safe mapping configuration.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is created via <see cref="MappingConfig.ToImmutable"/> and provides
    /// a read-only, thread-safe view of a mapping configuration suitable for concurrent access.
    /// </para>
    /// <para>
    /// <strong>Thread Safety:</strong> All properties are read-only and the internal collections
    /// are immutable snapshots, making this class safe to share across threads.
    /// </para>
    /// <para>
    /// <strong>Use Case:</strong> Use this when you need to import multiple files in parallel
    /// using the same mapping configuration without risk of concurrent modification.
    /// </para>
    /// </remarks>
    public sealed class ImmutableMappingConfig : IMappingConfig
    {
        /// <summary>
        /// Gets the software version this mapping is compatible with.
        /// </summary>
        public string SoftwareVersion { get; }
        
        /// <summary>
        /// Gets the optional version identifier for this mapping configuration.
        /// </summary>
        public string? MapVersion { get; }
        
        /// <summary>
        /// Gets the read-only collection of mapping entries.
        /// </summary>
        public IReadOnlyList<MappingEntry> ImportMap { get; }
        
        /// <summary>
        /// Internal constructor called by <see cref="MappingConfig.ToImmutable"/>.
        /// </summary>
        internal ImmutableMappingConfig(string softwareVersion, string? mapVersion, IReadOnlyList<MappingEntry> entries)
        { 
            SoftwareVersion = softwareVersion; 
            MapVersion = mapVersion; 
            ImportMap = entries; 
        }
    }

    /// <summary>
    /// Represents a single mapping entry that defines how a source column maps to a destination property.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each MappingEntry establishes a relationship between:
    /// - A source column (identified by <see cref="ColumnHeader"/> or <see cref="Aliases"/>)
    /// - A destination property (identified by <see cref="TargetType"/>.<see cref="PropertyName"/>)
    /// </para>
    /// <para>
    /// <strong>Target Types:</strong> Valid values for TargetType include:
    /// - "Bus" - Maps to <see cref="EasyAF.Data.Models.Bus"/> properties
    /// - "ArcFlash" - Maps to <see cref="EasyAF.Data.Models.ArcFlash"/> properties
    /// - "ShortCircuit" - Maps to <see cref="EasyAF.Data.Models.ShortCircuit"/> properties
    /// - "LVCB" - Maps to <see cref="EasyAF.Data.Models.LVCB"/> properties
    /// - "Fuse" - Maps to <see cref="EasyAF.Data.Models.Fuse"/> properties
    /// - "Cable" - Maps to <see cref="EasyAF.Data.Models.Cable"/> properties
    /// - "LVCB.TripUnit" - Maps to nested <see cref="EasyAF.Data.Models.TripUnit"/> properties
    /// </para>
    /// <para>
    /// <strong>Required vs Optional:</strong>
    /// - Required=true with Severity=Error: Import fails if column is missing
    /// - Required=false with DefaultValue: Uses default if column is missing
    /// - Required=false without DefaultValue: Property is left blank if column is missing
    /// </para>
    /// </remarks>
    /// <example>
    /// <para><strong>Example 1: Required mapping with error severity</strong></para>
    /// <code>
    /// new MappingEntry
    /// {
    ///     TargetType = "Bus",
    ///     PropertyName = "Id",
    ///     ColumnHeader = "Bus ID",
    ///     Required = true,
    ///     Severity = MappingSeverity.Error
    /// }
    /// </code>
    /// 
    /// <para><strong>Example 2: Optional mapping with default value and aliases</strong></para>
    /// <code>
    /// new MappingEntry
    /// {
    ///     TargetType = "Bus",
    ///     PropertyName = "Voltage",
    ///     ColumnHeader = "Nominal Voltage",
    ///     Required = false,
    ///     DefaultValue = "0.48",
    ///     Aliases = new[] { "Voltage", "kV", "Nom_kV" },
    ///     Severity = MappingSeverity.Warning
    /// }
    /// </code>
    /// 
    /// <para><strong>Example 3: Nested property mapping (LVCB Trip Unit)</strong></para>
    /// <code>
    /// new MappingEntry
    /// {
    ///     TargetType = "LVCB.TripUnit",
    ///     PropertyName = "LongTimePickup",
    ///     ColumnHeader = "LT Pickup (A)",
    ///     Required = false
    /// }
    /// </code>
    /// </example>
    public class MappingEntry
    {
        /// <summary>
        /// Gets or sets the target model type name (e.g., "Bus", "ArcFlash", "ShortCircuit", "LVCB.TripUnit").
        /// </summary>
        /// <remarks>
        /// This should match a model class name in the EasyAF.Data.Models namespace.
        /// Use dot notation for nested properties (e.g., "LVCB.TripUnit").
        /// Case-insensitive matching is used during import.
        /// </remarks>
        public string TargetType { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the property name on the target type (e.g., "Id", "Scenario", "Voltage").
        /// </summary>
        /// <remarks>
        /// Must match a public property on the target model class.
        /// Case-insensitive matching is used during import.
        /// </remarks>
        public string PropertyName { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the primary column header to match in the source file.
        /// </summary>
        /// <remarks>
        /// This is the expected header name in the CSV/Excel file.
        /// If not found, <see cref="Aliases"/> are checked (if provided).
        /// Matching is case-insensitive and whitespace-trimmed.
        /// </remarks>
        public string ColumnHeader { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets whether this mapping is required for a successful import.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If true and <see cref="Severity"/> is Error:
        /// - Missing column causes import to fail
        /// - Blank values are flagged as errors
        /// </para>
        /// <para>
        /// If false:
        /// - Missing column uses <see cref="DefaultValue"/> (if provided)
        /// - Import continues with warnings/info messages
        /// </para>
        /// </remarks>
        public bool Required { get; set; } = false;
        
        /// <summary>
        /// Gets or sets alternative column header names to search for if <see cref="ColumnHeader"/> is not found.
        /// </summary>
        /// <remarks>
        /// NOTE: Alias support is defined but not yet fully implemented in all importers.
        /// Future enhancement planned for Phase 3.
        /// </remarks>
        public string[]? Aliases { get; set; }
        
        /// <summary>
        /// Gets or sets the severity level for validation issues with this mapping.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item><description><see cref="MappingSeverity.Info"/>: Log informational message only</description></item>
        /// <item><description><see cref="MappingSeverity.Warning"/>: Log warning, continue import</description></item>
        /// <item><description><see cref="MappingSeverity.Error"/>: Log error, fail import</description></item>
        /// </list>
        /// </remarks>
        public MappingSeverity Severity { get; set; } = MappingSeverity.Info;
        
        /// <summary>
        /// Gets or sets an optional default value to use if the column is missing from the source file.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only used when:
        /// - The column specified by <see cref="ColumnHeader"/> and <see cref="Aliases"/> is not found
        /// - <see cref="Severity"/> is not Error
        /// </para>
        /// <para>
        /// The default value is used as-is (string). Type conversion happens during model population.
        /// </para>
        /// </remarks>
        public string? DefaultValue { get; set; }
    }

    /// <summary>
    /// Represents the result of validating a <see cref="MappingConfig"/>, containing any warnings or errors found.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use <see cref="HasErrors"/> to quickly check if validation failed.
    /// Warnings indicate potential issues but don't prevent usage.
    /// </para>
    /// </remarks>
    public class MappingValidationResult
    {
        /// <summary>
        /// Gets the list of warning messages found during validation.
        /// </summary>
        /// <remarks>
        /// Warnings indicate potential issues (e.g., duplicate mappings) that don't prevent usage
        /// but may cause unexpected behavior. Review warnings before proceeding with import.
        /// </remarks>
        public List<string> Warnings { get; } = new();
        
        /// <summary>
        /// Gets the list of error messages found during validation.
        /// </summary>
        /// <remarks>
        /// Errors indicate critical issues that must be fixed before the mapping can be used.
        /// Common errors include blank required fields and ambiguous required mappings.
        /// </remarks>
        public List<string> Errors { get; } = new();
        
        /// <summary>
        /// Gets a value indicating whether any validation errors were found.
        /// </summary>
        /// <remarks>
        /// Convenience property equivalent to checking <c>Errors.Count > 0</c>.
        /// If true, the mapping configuration should not be used until errors are corrected.
        /// </remarks>
        public bool HasErrors => Errors.Count > 0;
    }
}
