using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using ClosedXML.Excel;

namespace EasyAF.Import
{
    using EasyAF.Data.Models;
    public class ExcelImporter : IImporter
    {
        private readonly ILogger _logger;
        private readonly HashSet<string>? _worksheetNames; // null => all worksheets
        public ExcelImporter(ILogger? logger = null, IEnumerable<string>? worksheetNames = null)
        { _logger = logger ?? new FileLogger("import.log", LogMode.Standard); if (worksheetNames != null) _worksheetNames = new HashSet<string>(worksheetNames, StringComparer.OrdinalIgnoreCase); }

        public void Import(string filePath, IMappingConfig mappingConfig, DataSet targetDataSet, ImportOptions? options = null)
        {
            options ??= new ImportOptions();
            var strict = options.StrictMissingRequiredHeaders;
            var missingRequired = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _logger.Verbose(nameof(Import), $"--- Excel import started for file: {filePath} ---");
            targetDataSet.SoftwareVersion = mappingConfig.SoftwareVersion;
            targetDataSet.ArcFlashEntries ??= new Dictionary<(string, string), ArcFlash>();
            targetDataSet.ShortCircuitEntries ??= new Dictionary<(string, string, string), ShortCircuit>();
            targetDataSet.LVBreakerEntries ??= new Dictionary<string, LVBreaker>();
            targetDataSet.FuseEntries ??= new Dictionary<string, Fuse>();
            targetDataSet.CableEntries ??= new Dictionary<string, Cable>();
            targetDataSet.BusEntries ??= new Dictionary<string, Bus>();
            var groupsByType = mappingConfig.ImportMap.GroupBy(m => m.TargetType).ToDictionary(g => g.Key, g => g.ToList());
            var knownHeaders = new HashSet<string>(mappingConfig.ImportMap.Select(m => m.ColumnHeader.Trim()), StringComparer.OrdinalIgnoreCase);
            bool IsHeaderRow(IEnumerable<string> cells) { var arr = cells as string[] ?? cells.ToArray(); return arr.Count(f => knownHeaders.Contains((f ?? string.Empty).Trim())) >= 2; }
            var globalMissing = new HashSet<string>(StringComparer.OrdinalIgnoreCase); var duplicateCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            int startArc = targetDataSet.ArcFlashEntries.Count, startSc = targetDataSet.ShortCircuitEntries.Count, startLvcb = targetDataSet.LVBreakerEntries.Count, startFuse = targetDataSet.FuseEntries.Count, startCable = targetDataSet.CableEntries.Count, startBus = targetDataSet.BusEntries.Count;
            try
            {
                using var wb = new XLWorkbook(filePath); var worksheets = wb.Worksheets.AsEnumerable(); if (_worksheetNames?.Count > 0) worksheets = worksheets.Where(ws => _worksheetNames.Contains(ws.Name)); bool anyProcessed = false;
                foreach (var ws in worksheets)
                {
                    anyProcessed = true; _logger.Verbose(nameof(Import), $"Processing worksheet: {ws.Name}");
                    string[] currentHeader = Array.Empty<string>(); var currentHeaderIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase); var activeTargetTypes = new HashSet<string>(); bool inKnownSection = false; int physicalRow = 0; var worksheetMissing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var row in ws.RowsUsed())
                    {
                        physicalRow++; var cells = row.Cells().Select(c => c.GetString()).ToArray(); if (cells.All(string.IsNullOrWhiteSpace)) continue;
                        if (IsHeaderRow(cells))
                        {
                            currentHeader = cells.Select(f => (f ?? string.Empty).Trim()).ToArray();
                            _logger.Info(nameof(Import), $"Header row detected in '{ws.Name}' at {row.RowNumber()}", string.Join(" | ", currentHeader));
                            currentHeaderIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                            for (int i = 0; i < currentHeader.Length; i++)
                            { var h = currentHeader[i]; if (!currentHeaderIndex.ContainsKey(h)) currentHeaderIndex[h] = i + 1; }
                            activeTargetTypes.Clear();
                            foreach (var kvp in groupsByType)
                            {
                                if (kvp.Key.Contains('.')) continue; // skip nested groups here
                                var mapEntries = kvp.Value;
                                var idEntry = mapEntries.FirstOrDefault(e => e.PropertyName == "Id");
                                bool hasIdHeader = idEntry != null && currentHeaderIndex.ContainsKey(idEntry.ColumnHeader.Trim());
                                if (hasIdHeader) activeTargetTypes.Add(kvp.Key);
                                else
                                {
                                    // Heuristic: if multiple headers for this type are present but Id is missing, warn for diagnostics
                                    int presentCount = mapEntries.Count(e => currentHeaderIndex.ContainsKey(e.ColumnHeader.Trim()));
                                    if (presentCount >= 2)
                                    {
                                        _logger.Info(nameof(Import), $"Warning: Detected headers for '{kvp.Key}' but missing Id header '{idEntry?.ColumnHeader}'. Cannot import without Id.");
                                    }
                                }
                            }
                            inKnownSection = activeTargetTypes.Count > 0;
                            _logger.Verbose(nameof(Import), (inKnownSection ? "Activated mapped" : "Skipped unknown") + $" section in worksheet '{ws.Name}' at row {row.RowNumber()} for types: [{string.Join(", ", activeTargetTypes)}]");
                            continue;
                        }
                        if (!inKnownSection) continue;
                        foreach (var targetType in activeTargetTypes.ToList())
                        {
                            if (!groupsByType.TryGetValue(targetType, out var mapEntries)) continue; var idEntry = mapEntries.FirstOrDefault(e => e.PropertyName == "Id"); if (idEntry == null) continue; if (!currentHeaderIndex.TryGetValue(idEntry.ColumnHeader.Trim(), out int idCol)) continue; var idValue = row.Cell(idCol).GetString()?.Trim(); if (string.IsNullOrWhiteSpace(idValue)) continue; try { switch (targetType) { case "ArcFlash": var af = new ArcFlash(); PopulateObject(af, mapEntries, row, currentHeaderIndex, worksheetMissing, missingRequired, strict); if (!string.IsNullOrWhiteSpace(af.Id) && !string.IsNullOrWhiteSpace(af.Scenario)) { var key = (af.Id, af.Scenario); if (!targetDataSet.ArcFlashEntries.ContainsKey(key)) targetDataSet.ArcFlashEntries[key] = af; else LogDuplicate("ArcFlash"); } break; case "ShortCircuit": var sc = new ShortCircuit(); PopulateObject(sc, mapEntries, row, currentHeaderIndex, worksheetMissing, missingRequired, strict); var bus = sc.GetType().GetProperty("Bus")?.GetValue(sc) as string; var scen = sc.GetType().GetProperty("Scenario")?.GetValue(sc) as string; if (!string.IsNullOrWhiteSpace(sc.Id) && !string.IsNullOrWhiteSpace(bus) && !string.IsNullOrWhiteSpace(scen)) { var key = (sc.Id!, bus!, scen!); if (!targetDataSet.ShortCircuitEntries.ContainsKey(key)) targetDataSet.ShortCircuitEntries[key] = sc; else LogDuplicate("ShortCircuit"); } break; case "LVBreaker": var LVBreaker = new LVBreaker(); PopulateObject(LVBreaker, mapEntries, row, currentHeaderIndex, worksheetMissing, missingRequired, strict); if (!string.IsNullOrWhiteSpace(LVBreaker.Id)) { if (!targetDataSet.LVBreakerEntries.ContainsKey(LVBreaker.Id)) targetDataSet.LVBreakerEntries[LVBreaker.Id] = LVBreaker; else LogDuplicate("LVBreaker"); } break; case "Fuse": var fuse = new Fuse(); PopulateObject(fuse, mapEntries, row, currentHeaderIndex, worksheetMissing, missingRequired, strict); if (!string.IsNullOrWhiteSpace(fuse.Id)) { if (!targetDataSet.FuseEntries.ContainsKey(fuse.Id)) targetDataSet.FuseEntries[fuse.Id] = fuse; else LogDuplicate("Fuse"); } break; case "Cable": var cable = new Cable(); PopulateObject(cable, mapEntries, row, currentHeaderIndex, worksheetMissing, missingRequired, strict); if (!string.IsNullOrWhiteSpace(cable.Id)) { if (!targetDataSet.CableEntries.ContainsKey(cable.Id)) targetDataSet.CableEntries[cable.Id] = cable; else LogDuplicate("Cable"); } break; case "Bus": var busObj = new Bus(); PopulateObject(busObj, mapEntries, row, currentHeaderIndex, worksheetMissing, missingRequired, strict); if (!string.IsNullOrWhiteSpace(busObj.Id)) { if (!targetDataSet.BusEntries.ContainsKey(busObj.Id)) targetDataSet.BusEntries[busObj.Id] = busObj; else LogDuplicate("Bus"); } break; } } catch (Exception ex) { _logger.Error(nameof(Import), $"Exception processing {targetType} in worksheet '{ws.Name}' at row {row.RowNumber()}: {ex.Message}"); } }
                    }
                    if (worksheetMissing.Count > 0) { foreach (var m in worksheetMissing) globalMissing.Add(m); _logger.Error(nameof(Import), $"Missing headers in worksheet '{ws.Name}'", string.Join(", ", worksheetMissing)); }
                }
                if (_worksheetNames?.Count > 0 && !anyProcessed) _logger.Error(nameof(Import), $"None of the requested worksheets were found: {string.Join(", ", _worksheetNames)}");
            }
            catch (Exception ex) { _logger.Error(nameof(Import), $"Exception reading Excel file: {ex.Message}"); }

            int importedArc = targetDataSet.ArcFlashEntries.Count - startArc;
            int importedSc = targetDataSet.ShortCircuitEntries.Count - startSc;
            int importedLvcb = targetDataSet.LVBreakerEntries.Count - startLvcb;
            int importedFuse = targetDataSet.FuseEntries.Count - startFuse;
            int importedCable = targetDataSet.CableEntries.Count - startCable;
            int importedBus = targetDataSet.BusEntries.Count - startBus;
            var summary = $"Imported: ArcFlash={importedArc}, ShortCircuit={importedSc}, LVBreaker={importedLvcb}, Fuse={importedFuse}, Cable={importedCable}, Bus={importedBus}";
            _logger.Info(nameof(Import), "Validation summary", summary);
            if (importedLvcb == 0)
            {
                var idEntry = groupsByType.TryGetValue("LVBreaker", out var lmap) ? lmap.FirstOrDefault(e => e.PropertyName == "Id") : null;
                _logger.Info(nameof(Import), $"Warning: No LVBreaker entries imported from workbook. Verify header '{idEntry?.ColumnHeader ?? "Id column"}' exists and contains data.");
            }
            if (globalMissing.Count > 0) _logger.Error(nameof(Import), "Global missing headers", string.Join(", ", globalMissing.OrderBy(s => s)));
            if (duplicateCounts.Count > 0) _logger.Error(nameof(Import), "Duplicate key counts", string.Join(", ", duplicateCounts.Select(k => k.Key + ":" + k.Value)));

            if (string.IsNullOrWhiteSpace(targetDataSet.SoftwareVersion)) targetDataSet.SoftwareVersion = mappingConfig.SoftwareVersion; else if (!string.IsNullOrWhiteSpace(mappingConfig.SoftwareVersion) && !string.Equals(targetDataSet.SoftwareVersion, mappingConfig.SoftwareVersion, StringComparison.OrdinalIgnoreCase)) _logger.Error(nameof(Import), $"VersionMismatch: Dataset SoftwareVersion '{targetDataSet.SoftwareVersion}' differs from Mapping SoftwareVersion '{mappingConfig.SoftwareVersion}'");

            void LogDuplicate(string type) { if (!duplicateCounts.ContainsKey(type)) duplicateCounts[type] = 0; duplicateCounts[type]++; }
        }

        private void PopulateObject(object target, List<MappingEntry> mapEntries, IXLRow row, Dictionary<string, int> headerIndex, HashSet<string> missing, HashSet<string> missingRequired, bool strict)
        {
            foreach (var entry in mapEntries)
            {
                var col = entry.ColumnHeader.Trim();
                if (!headerIndex.TryGetValue(col, out int idx))
                {
                    missing.Add(col);
                    if (entry.Required && entry.Severity == MappingSeverity.Error)
                    {
                        if (strict) missingRequired.Add(col);
                        _logger.Error(nameof(PopulateObject), $"Required header missing: {col} for {entry.TargetType}.{entry.PropertyName}");
                    }
                    else if (!string.IsNullOrWhiteSpace(entry.DefaultValue))
                        SetProperty(target, entry.PropertyName, entry.DefaultValue);
                    continue;
                }
                var value = row.Cell(idx).GetString();
                SetProperty(target, entry.PropertyName, value);
            }
        }
        private void SetProperty(object obj, string propertyName, string? value)
        {
            var prop = obj.GetType().GetProperty(propertyName); if (prop == null || !prop.CanWrite) return;
            try
            {
                var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                object? converted = null;
                if (targetType == typeof(string)) converted = value;
                else if (targetType == typeof(bool))
                {
                    if (string.IsNullOrWhiteSpace(value)) converted = false;
                    else
                    {
                        var v = value.Trim().ToLowerInvariant();
                        // positives
                        if (v is "true" or "t" or "yes" or "y" or "1" or "x" or "adj" or "adjustable") converted = true;
                        // negatives (including domain token 'fixed')
                        else if (v is "false" or "f" or "no" or "n" or "0" or "fixed") converted = false;
                        else converted = false; // default safe
                    }
                }
                else if (targetType.IsEnum)
                {
                    if (!string.IsNullOrWhiteSpace(value) && Enum.IsDefined(targetType, value)) converted = Enum.Parse(targetType, value, true);
                }
                else if (targetType == typeof(int)) { if (int.TryParse(value, out var i)) converted = i; }
                else if (targetType == typeof(double)) { if (double.TryParse(value, out var d)) converted = d; }
                else if (targetType == typeof(decimal)) { if (decimal.TryParse(value, out var m)) converted = m; }
                else converted = value;
                prop.SetValue(obj, converted);
            }
            catch (Exception ex)
            {
                _logger.Error(nameof(Import), $"Property conversion failed for {obj.GetType().Name}.{propertyName}: {ex.Message}", value ?? "<null>");
            }
        }
    }
}

