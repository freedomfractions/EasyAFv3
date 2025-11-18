using System;
using System.Globalization;
using CsvHelper;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace EasyAF.Import
{
    using EasyAF.Data.Models;

    /// <summary>
    /// Imports data from a CSV file into a DataSet using a mapping configuration.
    /// Supports multi-section CSVs with repeated (and unrelated) header rows.
    /// A section header is detected heuristically: a row whose trimmed values
    /// contain at least 2 column headers from the mapping configuration.
    /// Unknown sections (no mapped Id headers) are skipped with a single log entry.
    /// </summary>
    public class CsvImporter : IImporter
    {
        private readonly ILogger _logger;

        public CsvImporter(ILogger? logger = null)
        {
            _logger = logger ?? new FileLogger("import.log", LogMode.Standard);
        }

        public void Import(string filePath, MappingConfig mappingConfig, DataSet targetDataSet)
        { throw new System.NotSupportedException("Use overload with ImportOptions"); }

        public void Import(string filePath, IMappingConfig mappingConfig, DataSet targetDataSet, ImportOptions? options)
        {
            options ??= new ImportOptions();
            var strict = options.StrictMissingRequiredHeaders;
            var missingRequired = new List<string>();
            _logger.Verbose(nameof(Import), $"--- Import started for file: {filePath} ---");
            targetDataSet.SoftwareVersion = targetDataSet.SoftwareVersion ?? mappingConfig.SoftwareVersion;
            if (!string.IsNullOrWhiteSpace(targetDataSet.SoftwareVersion) && !string.IsNullOrWhiteSpace(mappingConfig.SoftwareVersion) && !string.Equals(targetDataSet.SoftwareVersion, mappingConfig.SoftwareVersion, System.StringComparison.OrdinalIgnoreCase))
                _logger.Error(nameof(Import), $"VersionMismatch: Dataset SoftwareVersion '{targetDataSet.SoftwareVersion}' differs from Mapping SoftwareVersion '{mappingConfig.SoftwareVersion}'");

            // Ensure dataset dictionaries are initialized
            targetDataSet.ArcFlashEntries ??= new Dictionary<(string, string), ArcFlash>();
            targetDataSet.ShortCircuitEntries ??= new Dictionary<(string, string, string), ShortCircuit>();
            targetDataSet.LVCBEntries ??= new Dictionary<string, LVCB>();
            targetDataSet.FuseEntries ??= new Dictionary<string, Fuse>();
            targetDataSet.CableEntries ??= new Dictionary<string, Cable>();

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            // We will manually iterate records; CsvHelper still parses quoted fields.

            // Precompute mapping groups
            var groupsByType = mappingConfig.ImportMap
                .GroupBy(m => m.TargetType)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Fast lookup of all known column headers (trimmed)
            var knownHeaders = new HashSet<string>(mappingConfig.ImportMap.Select(m => m.ColumnHeader.Trim()), StringComparer.OrdinalIgnoreCase);

            // Track every header token we classify as a header cell (union across multiple tables/sections)
            var observedHeaderTokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Helper local functions
            int physicalRow = 0; // physical row counter (including headers) for logging

            bool IsPotentialHeaderRow(string[] fields, int rowNumber)
            {
                int matches = fields.Count(f => knownHeaders.Contains(f.Trim()));
                if (matches >= 2) return true;
                if (rowNumber == 1 && matches == 1) return true; // first non-blank row heuristic
                return false;
            }

            // Active section state
            string[] currentHeader = Array.Empty<string>();
            HashSet<string> currentHeaderSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, int> currentHeaderIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> activeTargetTypes = new HashSet<string>();
            bool inKnownSection = false;

            var missingHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            while (csv.Read())
            {
                physicalRow++;
                // Extract raw row fields (CsvHelper current record)
                var record = csv.Parser.Record ?? Array.Empty<string>();
                // Detect blank row
                bool isBlank = record.All(f => string.IsNullOrWhiteSpace(f));
                if (isBlank)
                {
                    continue; // skip blank rows – next non-blank may be header
                }

                // Determine if this row should be treated as a header row (heuristic)
                if (IsPotentialHeaderRow(record, physicalRow))
                {
                    currentHeader = record.Select(f => f.Trim()).ToArray();
                    foreach (var h in currentHeader)
                        if (!string.IsNullOrWhiteSpace(h)) observedHeaderTokens.Add(h);
                    currentHeaderSet = new HashSet<string>(currentHeader, StringComparer.OrdinalIgnoreCase);
                    currentHeaderIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < record.Length; i++)
                    {
                        var trimmed = record[i].Trim();
                        if (!currentHeaderIndex.ContainsKey(trimmed)) currentHeaderIndex[trimmed] = i;
                    }
                    activeTargetTypes.Clear();
                    foreach (var kvp in groupsByType)
                    {
                        // Skip nested / child pseudo-types (e.g., LVCB.TripUnit) for section activation
                        if (kvp.Key.Contains('.')) continue;
                        var idEntry = kvp.Value.FirstOrDefault(e => e.PropertyName == "Id");
                        if idEntry == null) continue;
                        if (currentHeaderSet.Contains(idEntry.ColumnHeader.Trim())) activeTargetTypes.Add(kvp.Key);
                    }
                    inKnownSection = activeTargetTypes.Count > 0;
                    _logger.Verbose(nameof(Import), (inKnownSection ? "Activated mapped" : "Scanned header (no active mapped type)") +
                        $" section at file row {physicalRow}: headers = [{string.Join(", ", currentHeader)}]" +
                        (inKnownSection ? $"; target types: {string.Join(", ", activeTargetTypes)}" : string.Empty));
                    missingHeaders.Clear();
                    continue; // proceed to next row after header established
                }

                if (!inKnownSection)
                {
                    // Data row outside an active mapped section – ignore.
                    continue;
                }

                // Process data row for each active target
                foreach (var targetType in activeTargetTypes.ToList())
                {
                    if (!groupsByType.TryGetValue(targetType, out var mapEntries)) continue;
                    var idEntry = mapEntries.FirstOrDefault(e => e.PropertyName == "Id");
                    if (idEntry == null) continue;
                    var idHeader = idEntry.ColumnHeader.Trim();
                    if (!currentHeaderIndex.TryGetValue(idHeader, out int idCol)) continue;
                    var idValue = SafeGet(csv, idCol);
                    if (string.IsNullOrWhiteSpace(idValue)) continue; // no id means skip entity for this row

                    try
                    {
                        switch (targetType)
                        {
                            case "ArcFlash":
                                var af = new ArcFlash();
                                PopulateObjectByIndex(af, mapEntries, csv, currentHeaderIndex, currentHeaderSet, missingHeaders, missingRequired, strict);
                                if (!string.IsNullOrWhiteSpace(af.Id) && !string.IsNullOrWhiteSpace(af.Scenario))
                                {
                                    var key = (af.Id, af.Scenario);
                                    if (!targetDataSet.ArcFlashEntries.ContainsKey(key)) targetDataSet.ArcFlashEntries[key] = af; else _logger.Error(nameof(Import), $"Duplicate ArcFlash key {key} at row {physicalRow} (skipped)");
                                }
                                break;
                            case "ShortCircuit":
                                var sc = new ShortCircuit();
                                PopulateObjectByIndex(sc, mapEntries, csv, currentHeaderIndex, currentHeaderSet, missingHeaders, missingRequired, strict);
                                var bus = sc.GetType().GetProperty("Bus")?.GetValue(sc) as string;
                                var scen = sc.GetType().GetProperty("Scenario")?.GetValue(sc) as string;
                                if (!string.IsNullOrWhiteSpace(sc.Id) && !string.IsNullOrWhiteSpace(bus) && !string.IsNullOrWhiteSpace(scen))
                                {
                                    var key = (sc.Id!, bus!, scen!);
                                    if (!targetDataSet.ShortCircuitEntries.ContainsKey(key)) targetDataSet.ShortCircuitEntries[key] = sc; else _logger.Error(nameof(Import), $"Duplicate ShortCircuit key {key} at row {physicalRow} (skipped)");
                                }
                                break;
                            case "LVCB":
                                var lvcb = new LVCB();
                                PopulateObjectByIndex(lvcb, mapEntries, csv, currentHeaderIndex, currentHeaderSet, missingHeaders, missingRequired, strict);
                                // Trip unit properties are now flattened directly on LVCB (no nested object)
                                // Mappings with TargetType="LVCB" and PropertyName="TripUnitXxx" will populate automatically
                                if (!string.IsNullOrWhiteSpace(lvcb.Id))
                                {
                                    if (!targetDataSet.LVCBEntries.ContainsKey(lvcb.Id)) targetDataSet.LVCBEntries[lvcb.Id] = lvcb; else _logger.Error(nameof(Import), $"Duplicate LVCB key {lvcb.Id} at row {physicalRow} (skipped)");
                                }
                                break;
                            case "Fuse":
                                var fuse = new Fuse();
                                PopulateObjectByIndex(fuse, mapEntries, csv, currentHeaderIndex, currentHeaderSet, missingHeaders, missingRequired, strict);
                                if (!string.IsNullOrWhiteSpace(fuse.Id))
                                {
                                    if (!targetDataSet.FuseEntries.ContainsKey(fuse.Id)) targetDataSet.FuseEntries[fuse.Id] = fuse; else _logger.Error(nameof(Import), $"Duplicate Fuse key {fuse.Id} at row {physicalRow} (skipped)");
                                }
                                break;
                            case "Cable":
                                var cable = new Cable();
                                PopulateObjectByIndex(cable, mapEntries, csv, currentHeaderIndex, currentHeaderSet, missingHeaders, missingRequired, strict);
                                if (!string.IsNullOrWhiteSpace(cable.Id))
                                {
                                    if (!targetDataSet.CableEntries.ContainsKey(cable.Id)) targetDataSet.CableEntries[cable.Id] = cable; else _logger.Error(nameof(Import), $"Duplicate Cable key {cable.Id} at row {physicalRow} (skipped)");
                                }
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(nameof(Import), $"Exception processing {targetType} at row {physicalRow}: {ex.Message}");
                    }
                }
            }

            if (missingHeaders.Count > 0)
                _logger.Error(nameof(Import), "Missing headers encountered:", string.Join(", ", missingHeaders));

            // Global strict validation (Option B): ensure all required+Error headers were ever observed in ANY header row.
            if (strict)
            {
                var requiredErrorHeaders = mappingConfig.ImportMap
                    .Where(e => e.Required && e.Severity == MappingSeverity.Error)
                    .Select(e => e.ColumnHeader.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                var globalMissing = requiredErrorHeaders.Where(h => !observedHeaderTokens.Contains(h)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                if (globalMissing.Count > 0)
                    throw new InvalidDataException("Strict mode: required headers missing: " + string.Join(", ", globalMissing));
            }
        }

        private static string? SafeGet(CsvReader csv, int index)
        {
            try { return csv.GetField(index); } catch { return null; }
        }

        private void PopulateObjectByIndex(object target, List<MappingEntry> mapEntries, CsvReader csv, Dictionary<string, int> headerIndex, HashSet<string> headerSet, HashSet<string> missingHeaders, List<string> missingRequired, bool strict)
        {
            foreach (var entry in mapEntries)
            {
                var col = entry.ColumnHeader.Trim();
                if (!headerSet.Contains(col) || !headerIndex.TryGetValue(col, out int idx))
                {
                    missingHeaders.Add(col);
                    if (entry.Required && entry.Severity == MappingSeverity.Error)
                    {
                        if (strict) missingRequired.Add(col);
                        _logger.Error(nameof(PopulateObjectByIndex), $"Required header missing: {col} for {entry.TargetType}.{entry.PropertyName}");
                    }
                    else if (!string.IsNullOrWhiteSpace(entry.DefaultValue))
                    {
                        SetProperty(target, entry.PropertyName, entry.DefaultValue);
                    }
                    continue;
                }
                var value = SafeGet(csv, idx);
                SetProperty(target, entry.PropertyName, value);
            }
        }

        private void SetProperty(object obj, string propertyName, string? value)
        {
            var prop = obj.GetType().GetProperty(propertyName);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(obj, value);
            }
        }
    }
}
