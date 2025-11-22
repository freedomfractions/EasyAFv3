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
            targetDataSet.ArcFlashEntries ??= new Dictionary<CompositeKey, ArcFlash>();
            targetDataSet.ShortCircuitEntries ??= new Dictionary<CompositeKey, ShortCircuit>();
            targetDataSet.LVBreakerEntries ??= new Dictionary<CompositeKey, LVBreaker>();
            targetDataSet.FuseEntries ??= new Dictionary<CompositeKey, Fuse>();
            targetDataSet.CableEntries ??= new Dictionary<CompositeKey, Cable>();

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
                    
                    // SIGNATURE MATCHING: Score each datatype by how many of its expected headers are present
                    var typeScores = new Dictionary<string, (int matchCount, int totalExpected, double percentage)>();
                    
                    foreach (var kvp in groupsByType)
                    {
                        if (kvp.Key.Contains('.')) continue; // skip nested groups

                        var dataType = kvp.Key;
                        var mapEntries = kvp.Value;
                        
                        // Count how many mapped headers are present
                        int matchCount = 0;
                        int totalExpected = mapEntries.Count;
                        
                        // Get key properties for this type
                        Type? instanceType = dataType switch
                        {
                            "ArcFlash" => typeof(ArcFlash),
                            "ShortCircuit" => typeof(ShortCircuit),
                            "LVBreaker" => typeof(LVBreaker),
                            "Fuse" => typeof(Fuse),
                            "Cable" => typeof(Cable),
                            "Bus" => typeof(Bus),
                            _ => null
                        };

                        if (instanceType == null) continue;
                        
                        var keyProps = CompositeKeyHelper.GetCompositeKeyProperties(instanceType);
                        
                        // Count all matching headers
                        foreach (var entry in mapEntries)
                        {
                            if (currentHeaderSet.Contains(entry.ColumnHeader.Trim()))
                            {
                                matchCount++;
                            }
                        }
                        
                        // Calculate match percentage
                        double percentage = totalExpected > 0 ? (double)matchCount / totalExpected * 100.0 : 0.0;
                        
                        typeScores[dataType] = (matchCount, totalExpected, percentage);
                        
                        _logger.Verbose(nameof(Import), $"Signature match for '{dataType}': {matchCount}/{totalExpected} headers ({percentage:F1}%), {keyProps.Length} key properties defined");
                    }
                    
                    // Find the best match (highest match count, with percentage as tiebreaker)
                    // Require at least 30% header overlap
                    const double MIN_MATCH_THRESHOLD = 30.0;
                    
                    var bestMatches = typeScores
                        .Where(kvp => kvp.Value.percentage >= MIN_MATCH_THRESHOLD)
                        .OrderByDescending(kvp => kvp.Value.matchCount)
                        .ThenByDescending(kvp => kvp.Value.percentage)
                        .ToList();
                    
                    if (bestMatches.Any())
                    {
                        var bestMatch = bestMatches.First();
                        var dataType = bestMatch.Key;
                        var score = bestMatch.Value;
                        
                        // Verify the best match has at least one key property
                        var instanceType = dataType switch
                        {
                            "ArcFlash" => typeof(ArcFlash),
                            "ShortCircuit" => typeof(ShortCircuit),
                            "LVBreaker" => typeof(LVBreaker),
                            "Fuse" => typeof(Fuse),
                            "Cable" => typeof(Cable),
                            "Bus" => typeof(Bus),
                            _ => null
                        };
                        
                        if (instanceType != null)
                        {
                            var keyProps = CompositeKeyHelper.GetCompositeKeyProperties(instanceType);
                            var mapEntries = groupsByType[dataType];
                            bool hasKeyHeader = keyProps.Any(kp => 
                                mapEntries.Any(e => e.PropertyName == kp && 
                                currentHeaderSet.Contains(e.ColumnHeader.Trim())));
                        
                            if (hasKeyHeader)
                            {
                                activeTargetTypes.Add(dataType);
                                _logger.Info(nameof(Import), $"Best match: '{dataType}' with {score.matchCount}/{score.totalExpected} headers ({score.percentage:F1}%)");
                            }
                            else
                            {
                                var keyNames = string.Join(", ", keyProps);
                                _logger.Info(nameof(Import), $"Rejected best match '{dataType}' - missing key headers ({keyNames})");
                            }
                        }
                        
                        // Log other strong candidates that were not selected
                        foreach (var candidate in bestMatches.Skip(1).Take(2))
                        {
                            _logger.Verbose(nameof(Import), $"  Candidate: '{candidate.Key}' with {candidate.Value.matchCount}/{candidate.Value.totalExpected} headers ({candidate.Value.percentage:F1}%)");
                        }
                    }
                    else
                    {
                        _logger.Info(nameof(Import), $"No datatype matched threshold ({MIN_MATCH_THRESHOLD}%) for CSV at row {physicalRow}");
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
                    
                    // Get the type for this target
                    Type? instanceType = targetType switch
                    {
                        "ArcFlash" => typeof(ArcFlash),
                        "ShortCircuit" => typeof(ShortCircuit),
                        "LVBreaker" => typeof(LVBreaker),
                        "Fuse" => typeof(Fuse),
                        "Cable" => typeof(Cable),
                        "Bus" => typeof(Bus),
                        _ => null
                    };

                    if (instanceType == null) continue;

                    // Discover composite key properties for this type
                    var keyProps = CompositeKeyHelper.GetCompositeKeyProperties(instanceType);
                    if (keyProps.Length == 0)
                    {
                        _logger.Error(nameof(Import), $"No composite key properties found for {targetType} - skipping type");
                        continue;
                    }

                    // Check if at least one key property is mapped and has a value in this row
                    bool hasKeyValue = false;
                    foreach (var keyProp in keyProps)
                    {
                        var keyMapping = mapEntries.FirstOrDefault(e => e.PropertyName == keyProp);
                        if (keyMapping != null && currentHeaderIndex.TryGetValue(keyMapping.ColumnHeader.Trim(), out int keyCol))
                        {
                            var keyValue = SafeGet(csv, keyCol);
                            if (!string.IsNullOrWhiteSpace(keyValue))
                            {
                                hasKeyValue = true;
                                break;
                            }
                        }
                    }

                    if (!hasKeyValue) continue; // Skip this row if no key values present

                    try
                    {
                        // Dynamically create instance based on target type
                        object? instance = Activator.CreateInstance(instanceType);
                        if (instance == null) continue;

                        // Populate object from CSV using mapping
                        PopulateObjectByIndex(instance, mapEntries, csv, currentHeaderIndex, currentHeaderSet, missingHeaders, missingRequired, strict);

                        // Build composite key dynamically using reflection
                        var key = CompositeKeyHelper.BuildCompositeKey(instance, instanceType);
                        if (key == null)
                        {
                            _logger.Error(nameof(Import), $"Incomplete composite key for {targetType} at row {physicalRow} - skipped");
                            continue;
                        }

                        // Add to appropriate dictionary based on target type
                        switch (targetType)
                        {
                            case "ArcFlash":
                                if (!targetDataSet.ArcFlashEntries.ContainsKey(key))
                                    targetDataSet.ArcFlashEntries[key] = (ArcFlash)instance;
                                else
                                    _logger.Error(nameof(Import), $"Duplicate ArcFlash key {key} at row {physicalRow} (skipped)");
                                break;

                            case "ShortCircuit":
                                if (!targetDataSet.ShortCircuitEntries.ContainsKey(key))
                                    targetDataSet.ShortCircuitEntries[key] = (ShortCircuit)instance;
                                else
                                    _logger.Error(nameof(Import), $"Duplicate ShortCircuit key {key} at row {physicalRow} (skipped)");
                                break;

                            case "LVBreaker":
                                if (!targetDataSet.LVBreakerEntries.ContainsKey(key))
                                    targetDataSet.LVBreakerEntries[key] = (LVBreaker)instance;
                                else
                                    _logger.Error(nameof(Import), $"Duplicate LVBreaker key {key} at row {physicalRow} (skipped)");
                                break;

                            case "Fuse":
                                if (!targetDataSet.FuseEntries.ContainsKey(key))
                                    targetDataSet.FuseEntries[key] = (Fuse)instance;
                                else
                                    _logger.Error(nameof(Import), $"Duplicate Fuse key {key} at row {physicalRow} (skipped)");
                                break;

                            case "Cable":
                                if (!targetDataSet.CableEntries.ContainsKey(key))
                                    targetDataSet.CableEntries[key] = (Cable)instance;
                                else
                                    _logger.Error(nameof(Import), $"Duplicate Cable key {key} at row {physicalRow} (skipped)");
                                break;

                            case "Bus":
                                if (!targetDataSet.BusEntries.ContainsKey(key))
                                    targetDataSet.BusEntries[key] = (Bus)instance;
                                else
                                    _logger.Error(nameof(Import), $"Duplicate Bus key {key} at row {physicalRow} (skipped)");
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

