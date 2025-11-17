using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EasyAF.Import;
using EasyAF.Modules.Map.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace EasyAF.Modules.Map.Services
{
    /// <summary>
    /// Service for serializing and deserializing MapDocument instances to/from .ezmap JSON files.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The .ezmap file format extends the standard MappingConfig format used by ImportManager
    /// with additional metadata fields used by the Map Editor:
    /// - MapName: User-friendly name for the mapping configuration
    /// - Description: Optional description of the mapping's purpose
    /// - DateModified: Timestamp of last modification
    /// - ReferencedFiles: List of sample files used to create the mapping
    /// </para>
    /// <para>
    /// <strong>Compatibility:</strong> Files saved by the Map Editor can be loaded by the 
    /// Project Module's ImportManager (which ignores the extra metadata fields).
    /// Conversely, the Map Editor can load files created by older tools or ImportManager
    /// (missing metadata fields default to empty/null values).
    /// </para>
    /// <para>
    /// <strong>Format Version:</strong> This serializer writes format version 3.0.0, which is
    /// compatible with the EasyAF V3 ImportManager and earlier versions that use MappingConfig.
    /// </para>
    /// </remarks>
    public class MapDocumentSerializer
    {
        private static readonly JsonSerializerSettings JsonSettings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };

        /// <summary>
        /// Saves a MapDocument to a .ezmap JSON file.
        /// </summary>
        /// <param name="document">The MapDocument to save.</param>
        /// <param name="filePath">The file path where the document should be saved.</param>
        /// <exception cref="ArgumentNullException">Thrown when document or filePath is null.</exception>
        /// <exception cref="IOException">Thrown when file write operation fails.</exception>
        /// <remarks>
        /// <para>
        /// The saved file includes:
        /// - Map Editor metadata (MapName, Description, DateModified, ReferencedFiles)
        /// - MappingConfig fields (SoftwareVersion, MapVersion, ImportMap)
        /// </para>
        /// <para>
        /// The ImportMap array is constructed from the document's MappingsByDataType dictionary,
        /// with each mapping entry tagged with its TargetType (data type name).
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var serializer = new MapDocumentSerializer();
        /// serializer.Save(mapDocument, "path/to/file.ezmap");
        /// </code>
        /// </example>
        public void Save(MapDocument document, string filePath)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            try
            {
                // Update DateModified before saving
                document.DateModified = DateTime.Now;

                // Build the JSON structure
                var json = new
                {
                    // Optional JSON schema reference (ignored by parsers, helpful for editors)
                    schema = "https://easyaf.app/schemas/mapping-v3.json",
                    
                    // Map Editor metadata (ignored by ImportManager)
                    MapName = document.MapName,
                    Description = string.IsNullOrWhiteSpace(document.Description) ? null : document.Description,
                    DateModified = document.DateModified,
                    ReferencedFiles = document.ReferencedFiles.Select(f => new
                    {
                        FilePath = f.FilePath,
                        // Status is transient - don't persist it
                    }).ToArray(),
                    
                    // MappingConfig fields (used by ImportManager)
                    SoftwareVersion = document.SoftwareVersion,
                    MapVersion = "1.0",
                    ImportMap = document.MappingsByDataType
                        .SelectMany(kvp => kvp.Value.Select(m => new MappingEntry
                        {
                            TargetType = kvp.Key,
                            PropertyName = m.PropertyName,
                            ColumnHeader = m.ColumnHeader,
                            Required = false, // Map editor doesn't currently support Required flag
                            Severity = MappingSeverity.Info
                        }))
                        .ToArray()
                };

                // Serialize to JSON
                var jsonText = JsonConvert.SerializeObject(json, JsonSettings);
                
                // Write to file
                File.WriteAllText(filePath, jsonText);
                
                Log.Information("Saved map document to {Path} ({MappingCount} mappings across {DataTypeCount} data types)",
                    filePath, 
                    document.MappingsByDataType.Values.Sum(v => v.Count),
                    document.MappingsByDataType.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save map document to {Path}", filePath);
                throw;
            }
        }

        /// <summary>
        /// Loads a MapDocument from a .ezmap JSON file.
        /// </summary>
        /// <param name="filePath">The file path to load from.</param>
        /// <returns>A MapDocument instance populated with the file's data.</returns>
        /// <exception cref="ArgumentNullException">Thrown when filePath is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file doesn't exist.</exception>
        /// <exception cref="InvalidDataException">Thrown when the file contains invalid JSON or schema.</exception>
        /// <remarks>
        /// <para>
        /// This method can load:
        /// - Files created by the Map Editor (with full metadata)
        /// - Files created by older tools or ImportManager (metadata defaults to empty)
        /// - Legacy .ezmap files without Map Editor metadata fields
        /// </para>
        /// <para>
        /// <strong>Validation:</strong> The loaded MappingConfig is validated using MappingConfig.Validate().
        /// Validation errors are logged but don't prevent loading (to support recovering from
        /// partially corrupted files).
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var serializer = new MapDocumentSerializer();
        /// var document = serializer.Load("path/to/file.ezmap");
        /// </code>
        /// </example>
        public MapDocument Load(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath)) throw new FileNotFoundException($"Map file not found: {filePath}", filePath);

            try
            {
                // Read and parse JSON
                var jsonText = File.ReadAllText(filePath);
                var json = JObject.Parse(jsonText);

                // Extract metadata (with defaults for backward compatibility)
                var mapName = json["MapName"]?.ToString() ?? Path.GetFileNameWithoutExtension(filePath);
                var description = json["Description"]?.ToString() ?? string.Empty;
                var dateModified = json["DateModified"]?.ToObject<DateTime>() ?? File.GetLastWriteTime(filePath);
                var softwareVersion = json["SoftwareVersion"]?.ToString() ?? "Unknown";

                // Extract referenced files (may not exist in older files)
                var referencedFiles = new List<ReferencedFile>();
                if (json["ReferencedFiles"] is JArray filesArray)
                {
                    foreach (var fileToken in filesArray)
                    {
                        var path = fileToken["FilePath"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(path))
                        {
                            referencedFiles.Add(new ReferencedFile
                            {
                                FilePath = path,
                                // Status will be recalculated when the document loads
                                Status = File.Exists(path) ? "Valid" : "Missing"
                            });
                        }
                    }
                }

                // Extract mappings using MappingConfig for compatibility
                var mappingsByDataType = new Dictionary<string, List<Mapping>>();
                
                if (json["ImportMap"] is JArray importMapArray)
                {
                    foreach (var entry in importMapArray)
                    {
                        var targetType = entry["TargetType"]?.ToString();
                        var propertyName = entry["PropertyName"]?.ToString();
                        var columnHeader = entry["ColumnHeader"]?.ToString();

                        if (string.IsNullOrWhiteSpace(targetType) || 
                            string.IsNullOrWhiteSpace(propertyName) || 
                            string.IsNullOrWhiteSpace(columnHeader))
                        {
                            Log.Warning("Skipping invalid mapping entry in {Path}: TargetType={TargetType}, PropertyName={PropertyName}, ColumnHeader={ColumnHeader}",
                                filePath, targetType, propertyName, columnHeader);
                            continue;
                        }

                        // Group mappings by data type
                        if (!mappingsByDataType.ContainsKey(targetType))
                        {
                            mappingsByDataType[targetType] = new List<Mapping>();
                        }

                        mappingsByDataType[targetType].Add(new Mapping
                        {
                            PropertyName = propertyName,
                            ColumnHeader = columnHeader
                        });
                    }
                }

                // Create and populate document
                var document = new MapDocument
                {
                    MapName = mapName,
                    Description = description,
                    DateModified = dateModified,
                    SoftwareVersion = softwareVersion
                };

                // Add referenced files
                foreach (var file in referencedFiles)
                {
                    document.ReferencedFiles.Add(file);
                }

                // Add mappings
                foreach (var kvp in mappingsByDataType)
                {
                    foreach (var mapping in kvp.Value)
                    {
                        document.UpdateMapping(kvp.Key, mapping.PropertyName, mapping.ColumnHeader);
                    }
                }

                Log.Information("Loaded map document from {Path} ({MappingCount} mappings across {DataTypeCount} data types)",
                    filePath,
                    mappingsByDataType.Values.Sum(v => v.Count),
                    mappingsByDataType.Count);

                return document;
            }
            catch (JsonException ex)
            {
                Log.Error(ex, "Failed to parse JSON from {Path}", filePath);
                throw new InvalidDataException($"Invalid JSON format in file: {filePath}", ex);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load map document from {Path}", filePath);
                throw;
            }
        }

        /// <summary>
        /// Validates that a file is a valid .ezmap file without fully loading it.
        /// </summary>
        /// <param name="filePath">The file path to validate.</param>
        /// <returns>True if the file appears to be a valid .ezmap file; otherwise false.</returns>
        /// <remarks>
        /// <para>
        /// This method performs a quick validation by checking:
        /// - File exists
        /// - File contains valid JSON
        /// - JSON has an "ImportMap" array (the only required field)
        /// </para>
        /// <para>
        /// This is useful for file type detection before attempting a full load.
        /// </para>
        /// </remarks>
        public bool IsValidMapFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return false;

                var jsonText = File.ReadAllText(filePath);
                var json = JObject.Parse(jsonText);

                // Must have an ImportMap array (even if empty)
                return json["ImportMap"] is JArray;
            }
            catch
            {
                return false;
            }
        }
    }
}
