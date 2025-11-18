using System;
using System.Collections.Generic;
using System.Linq;
using EasyAF.Data; // for DocxTemplate and TableFormattingOptions
using EasyAF.Data.Models;
using System.Text.Json;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace EasyAF.Engine
{
    public class EasyAFEngine
    {
        // expose helper methods for spec loader
        public static string EvaluatePropertyPath(object obj, string path) => EvaluatePropertyPathInternal(obj, path);
        public static string RenderFormat(object o, string template) => RenderFormatInternal(o, template);
        public static string CleanOutput(string s) => CleanOutputInternal(s);

        public int DebugFilterSampleCount { get; set; } = 0; // when >0 capture and print filter pass/fail samples

        private readonly Dictionary<string, TableDefinition> _tables = new(StringComparer.OrdinalIgnoreCase);
        private List<PropertyMappingSpec>? _propertyMappings; // holds loaded property mappings
        private readonly ILogger _logger;
        public EasyAFEngine(ILogger<EasyAFEngine>? logger = null)
        { _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<EasyAFEngine>.Instance; }

        public void Register(TableDefinition def) => _tables[def.Id] = def;
        public IEnumerable<TableDefinition> List() => _tables.Values;

        public class RunSummary
        {
            public DateTime StartedUtc { get; set; }
            public DateTime FinishedUtc { get; set; }
            public double TotalSeconds { get; set; }
            public List<TableRunSummary> Tables { get; set; } = new();
            public int TablesProcessed => Tables.Count(t => t.Status == "ok");
            public int TablesSkipped => Tables.Count(t => t.Status != "ok");
            public int TotalRows => Tables.Sum(t => t.RowCount);
            public long TotalCells => Tables.Sum(t => t.CellCount);
            public long TotalExpressionCells => Tables.Sum(t => t.ExpressionCellCount);
            public double TotalEvalMs => Tables.Sum(t => t.EvalMs);
            public double TotalRenderMs => Tables.Sum(t => t.RenderMs);
            public List<string> Warnings { get; set; } = new();
            public List<string> Errors { get; set; } = new();
        }
        public class TableRunSummary
        {
            public string TableId { get; set; } = string.Empty;
            public string AltText { get; set; } = string.Empty;
            public string Mode { get; set; } = string.Empty;
            public int RowCount { get; set; }
            public int ColumnCount { get; set; }
            public long CellCount { get; set; }
            public int ExpressionColumnCount { get; set; }
            public long ExpressionCellCount { get; set; }
            public double EvalMs { get; set; }
            public double RenderMs { get; set; }
            public string Status { get; set; } = "ok"; // ok / skipped / error
            public string? Message { get; set; }
        }

        public void PopulateTemplate(string templatePath, string outputPath, ProjectContext ctx, IEnumerable<string>? tableIdsToProcess = null)
        {
            var summary = new RunSummary { StartedUtc = DateTime.UtcNow };
            var swTotal = Stopwatch.StartNew();
            var evaluator = new DefaultTableEvaluator(this);
            var renderer = new DocxTableRenderer();
            var toProcess = (tableIdsToProcess == null)
                ? _tables.Values.ToList()
                : _tables.Values.Where(t => tableIdsToProcess.Contains(t.Id, StringComparer.OrdinalIgnoreCase)).ToList();
            _logger.LogInformation("[Populate] Processing {count} table specs", toProcess.Count);
            File.Copy(templatePath, outputPath, true);
            using var tpl = DocxTemplate.OpenReadWrite(outputPath);
            var templateTables = DocxTemplate.ListTables(templatePath).Select(t => new { t.Caption, t.Description }).ToList();
            int processed = 0;
            foreach (var td in toProcess)
            {
                var ts = new TableRunSummary { TableId = td.Id, AltText = td.AltText, Mode = td.Mode.ToString(), ColumnCount = td.Columns.Count };
                summary.Tables.Add(ts);
                processed++;
                if (processed % 5 == 0) _logger.LogInformation("[Populate] Progress: {done}/{total} tables", processed, toProcess.Count);
                bool tableExists = templateTables.Any(x => (x.Caption!=null && string.Equals(x.Caption, td.AltText, StringComparison.OrdinalIgnoreCase)) || (x.Description!=null && string.Equals(x.Description, td.AltText, StringComparison.OrdinalIgnoreCase)));
                if (!tableExists)
                {
                    _logger.LogWarning("[Populate] Skipping '{id}' alt '{alt}' - not in template", td.Id, td.AltText);
                    ts.Status = "skipped"; ts.Message = "AltText not found"; continue;
                }
                var headerTexts = td.Columns.Select(c => c.Header).ToArray();
                try { tpl.SetHeaderRowByAltText(td.AltText, headerTexts); }
                catch (Exception ex) { _logger.LogError(ex, "[Populate] Header update failed {id}", td.Id); summary.Errors.Add($"Header:{td.Id}:{ex.Message}"); }
                try {
                    TableEvalResult eval;
                    var swEval = Stopwatch.StartNew();
                    try { eval = evaluator.Evaluate(td, ctx); }
                    catch (Exception ex) { _logger.LogError(ex, "[Populate] Eval failed {id}", td.Id); summary.Errors.Add($"Eval:{td.Id}:{ex.Message}"); ts.Status="error"; ts.Message = ex.Message; continue; }
                    swEval.Stop();
                    ts.EvalMs = swEval.Elapsed.TotalMilliseconds;
                    ts.RowCount = eval.Rows.Count;
                    _logger.LogInformation("[Populate] {id} rows={rows} evalMs={ms:F1}", td.Id, eval.Rows.Count, ts.EvalMs);
                    if (eval.Rows.Count == 0)
                    {
                        try
                        {
                            var msg = string.IsNullOrWhiteSpace(td.EmptyMessage) ? "No entries." : td.EmptyMessage!;
                            var optsE = td.Formatting ?? new TableFormattingOptions();
                            tpl.ReplaceTableWithSingleCellMessage(td.AltText, msg, optsE, td.EmptyFormatting?.FontName, td.EmptyFormatting?.FontSize.HasValue==true? (int?)Math.Round(td.EmptyFormatting.FontSize.Value*2):null, td.EmptyFormatting?.HorizontalAlignment, td.EmptyFormatting?.VerticalAlignment, td.EmptyFormatting?.Bold, td.EmptyFormatting?.Fill, td.EmptyFormatting?.TextColor);
                            _logger.LogInformation("[Populate] Inserted empty placeholder for {id}", td.Id);
                        }
                        catch (Exception ex) { _logger.LogError(ex, "[Populate] Empty placeholder failed {id}", td.Id); summary.Warnings.Add($"EmptyPlaceholder:{td.Id}:{ex.Message}"); }
                        continue;
                    }
                    if (td.SortSpecs!=null && td.SortSpecs.Count>0 && eval.Rows.Count>0)
                    {
                        try
                        {
                            var rows = eval.Rows; var indices = Enumerable.Range(0, rows.Count).ToList(); IOrderedEnumerable<int>? orderedIdx = null;
                            for (int si=0; si<td.SortSpecs.Count; si++)
                            {
                                var s=td.SortSpecs[si]; int colIndex=Math.Max(0,s.Column-1);
                                Func<int, object> keySel = i => rows[i].Length>colIndex? (object)rows[i][colIndex]:string.Empty;
                                if (s.Numeric) keySel = i => { var v = rows[i].Length>colIndex? rows[i][colIndex]:string.Empty; return double.TryParse(v,out var d)? (object)d : double.NaN; };
                                bool desc = string.Equals(s.Direction,"desc",StringComparison.OrdinalIgnoreCase);
                                orderedIdx = si==0 ? (desc? indices.OrderByDescending(keySel): indices.OrderBy(keySel)) : (desc? orderedIdx!.ThenByDescending(keySel): orderedIdx!.ThenBy(keySel));
                            }
                            if (orderedIdx!=null)
                            {
                                var newOrder = orderedIdx.ToList();
                                eval.Rows = newOrder.Select(i=> eval.Rows[i]).ToList();
                                if (td.LastSourceObjects!=null && td.LastSourceObjects.Count==newOrder.Count)
                                    td.LastSourceObjects = newOrder.Select(i=> td.LastSourceObjects![i]).ToList();
                                // Keep formatting matrices in sync with sorted rows
                                if (eval.CellFills!=null && eval.CellFills.Count==newOrder.Count)
                                    eval.CellFills = newOrder.Select(i=> eval.CellFills![i]).ToList();
                                if (eval.CellTextColors!=null && eval.CellTextColors.Count==newOrder.Count)
                                    eval.CellTextColors = newOrder.Select(i=> eval.CellTextColors![i]).ToList();
                            }
                        }
                        catch (Exception ex) { _logger.LogError(ex, "[Populate] Sort failed {id}", td.Id); summary.Warnings.Add($"Sort:{td.Id}:{ex.Message}"); }
                    }
                    var swRender = Stopwatch.StartNew();
                    try { renderer.Render(tpl, td, eval); _logger.LogInformation("[Populate] Populated {id}", td.Id); }
                    catch (Exception ex) { _logger.LogError(ex, "[Populate] Render failed {id}", td.Id); summary.Errors.Add($"Render:{td.Id}:{ex.Message}"); ts.Status="error"; ts.Message = ex.Message; continue; }
                    swRender.Stop(); ts.RenderMs = swRender.Elapsed.TotalMilliseconds;
                    ts.CellCount = (long)ts.RowCount * ts.ColumnCount;
                    // Expression metrics not available on ColumnDefinition (placeholder for future when expression retained)
                    ts.ExpressionColumnCount = 0;
                    ts.ExpressionCellCount = 0;
                } catch (InvalidOperationException) { throw; } catch (Exception ex) { _logger.LogError(ex, "[Populate] Unexpected table failure {id}", td.Id); summary.Errors.Add($"Unexpected:{td.Id}:{ex.Message}"); ts.Status="error"; ts.Message = ex.Message; }
            }
            tpl.SaveAs(outputPath); _logger.LogInformation("[Populate] Completed all tables");
            swTotal.Stop(); summary.FinishedUtc = DateTime.UtcNow; summary.TotalSeconds = swTotal.Elapsed.TotalSeconds;
            var summaryPath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(outputPath)) ?? ".", "run-summary.json");
            try { var opts = new JsonSerializerOptions{ WriteIndented = true }; File.WriteAllText(summaryPath, JsonSerializer.Serialize(summary, opts)); _logger.LogInformation("[Populate] Wrote run summary {path}", summaryPath); }
            catch (Exception ex) { _logger.LogError(ex, "[Populate] Failed writing run summary"); }
        }

        public PopulateResult PopulateTemplateResult(string templatePath, string outputPath, ProjectContext ctx, IEnumerable<string>? tableIdsToProcess = null, bool throwOnError = false)
        {
            var result = new PopulateResult { OutputPath = outputPath };
            try
            {
                PopulateTemplate(templatePath, outputPath, ctx, tableIdsToProcess); // existing implementation captures its own logging + run-summary file
                // Best-effort read back run-summary.json to populate Summary result
                try
                {
                    var summaryPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(outputPath)) ?? ".", "run-summary.json");

                    if (System.IO.File.Exists(summaryPath))
                    {
                        var json = System.IO.File.ReadAllText(summaryPath);
                        var rs = System.Text.Json.JsonSerializer.Deserialize<RunSummary>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        result.Summary = rs;
                        if (rs != null)
                        {
                            result.TablesRequested = rs.Tables.Count;
                            result.TablesPopulated = rs.Tables.Count(t => t.Status=="ok");
                            result.TablesSkipped = rs.Tables.Count(t => t.Status!="ok");
                            result.Warnings.AddRange(rs.Warnings);
                            result.Errors.AddRange(rs.Errors);
                        }
                    }
                }
                catch (Exception ex) { _logger.LogWarning(ex, "[PopulateResult] Failed to parse run-summary.json"); }
                result.Success = result.Errors.Count == 0;
                if (!result.Success && string.IsNullOrWhiteSpace(result.FailReason)) result.FailReason = string.Join("; ", result.Errors);
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
                result.FailReason = ex.Message;
                result.Success = false;
                if (throwOnError) throw new InvalidOperationException("PopulateTemplate failed", ex);
            }
            return result;
        }

        public TableEvalResult EvaluateTable(string tableId, ProjectContext ctx)
        {
            if (!_tables.TryGetValue(tableId, out var td)) throw new ArgumentException($"Unknown table id: {tableId}");
            var rows = td.Mode == TableMode.Diff ? BuildDiffRows(td, ctx) : BuildNewRows(td, ctx);
            return new TableEvalResult { TableId = tableId, Rows = rows, SourceObjects = td.LastSourceObjects ?? new List<object>() };
        }

        private List<string[]> BuildNewRows(TableDefinition td, ProjectContext ctx)
        {
            var rows = new List<string[]>();
            var sourceObjects = new List<object>();
            IEnumerable<object> items = Enumerable.Empty<object>();
            if (td.Id.StartsWith("LVCB.Breakers", StringComparison.OrdinalIgnoreCase))
            {
                items = ctx.LVCBs().Cast<object>();
            }
            else if (td.Id.StartsWith("Fuse", StringComparison.OrdinalIgnoreCase))
            {
                items = ctx.Fuses().Cast<object>();
            }
            else if (td.Id.StartsWith("Cable", StringComparison.OrdinalIgnoreCase))
            {
                items = ctx.Cables().Cast<object>();
            }
            else if (td.Id.StartsWith("ArcFlash", StringComparison.OrdinalIgnoreCase))
            {
                items = ctx.ArcFlashEntries().Cast<object>();
            }
            else if (td.Id.StartsWith("ShortCircuit", StringComparison.OrdinalIgnoreCase))
            {
                items = ctx.ShortCircuitEntries().Cast<object>();
            }

            if (td.FilterSpecs != null && td.FilterSpecs.Count > 0)
            {
                var passSamples = new List<string>();
                var failSamples = new List<string>();
                var filtered = new List<object>();
                foreach (var it in items)
                {
                    string? reason;
                    var ok = ApplyFilters(it, td.FilterSpecs, td.FilterGroups, out reason);
                    if (ok)
                    {
                        filtered.Add(it);
                        if (DebugFilterSampleCount > 0 && passSamples.Count < DebugFilterSampleCount)
                            passSamples.Add(BuildSampleDescription(it, td, "PASS"));
                    }
                    else
                    {
                        if (DebugFilterSampleCount > 0 && failSamples.Count < DebugFilterSampleCount)
                            failSamples.Add(BuildSampleDescription(it, td, "FAIL: " + reason));
                    }
                    if (DebugFilterSampleCount > 0 && passSamples.Count >= DebugFilterSampleCount && failSamples.Count >= DebugFilterSampleCount)
                        break;
                }
                items = filtered;
                if (DebugFilterSampleCount > 0)
                {
                    _logger.LogInformation($"[FilterDebug] Table {td.Id} samples (pass {passSamples.Count}, fail {failSamples.Count})");
                    foreach (var s in passSamples) _logger.LogInformation("[FilterDebug][PASS] " + s);
                    foreach (var s in failSamples) _logger.LogInformation("[FilterDebug][FAIL] " + s);
                }
            }

            foreach (var it in items)
            {
                rows.Add(RenderRow(td, ctx, it));
                sourceObjects.Add(it);
            }
            td.LastSourceObjects = sourceObjects;
            return rows;
        }

        private string BuildSampleDescription(object it, TableDefinition td, string status)
        {
            // Try Id, else first column render, else type name
            string id = EvaluatePropertyPathInternal(it, "Id");
            if (string.IsNullOrWhiteSpace(id))
            {
                var firstCol = td.Columns.FirstOrDefault();
                if (firstCol?.Renderer != null) id = firstCol.Renderer(new ProjectContext(null,null), it); // lightweight context; may be empty
                if (string.IsNullOrWhiteSpace(id)) id = it.GetType().Name;
            }
            return id + " => " + status;
        }

        private List<string[]> BuildDiffRows(TableDefinition td, ProjectContext ctx)
        {
            if (ctx.OldData == null) return BuildNewRows(td, ctx);
            if (td.Id.StartsWith("LVCB.Breakers", StringComparison.OrdinalIgnoreCase)) return BuildDiffFor(td, ctx, ctx.NewData.LVCBEntries, ctx.OldData.LVCBEntries);
            if (td.Id.StartsWith("Fuse", StringComparison.OrdinalIgnoreCase)) return BuildDiffFor(td, ctx, ctx.NewData.FuseEntries, ctx.OldData.FuseEntries);
            if (td.Id.StartsWith("Cable", StringComparison.OrdinalIgnoreCase)) return BuildDiffFor(td, ctx, ctx.NewData.CableEntries, ctx.OldData.CableEntries);
            if (td.Id.StartsWith("ShortCircuit", StringComparison.OrdinalIgnoreCase)) return BuildDiffForComposite(td, ctx,
                ctx.NewData.ShortCircuitEntries == null ? null : ctx.NewData.ShortCircuitEntries.ToDictionary(k => CompositeKey(k.Key), v => v.Value, StringComparer.OrdinalIgnoreCase),
                ctx.OldData?.ShortCircuitEntries == null ? null : ctx.OldData.ShortCircuitEntries.ToDictionary(k => CompositeKey(k.Key), v => v.Value, StringComparer.OrdinalIgnoreCase));
            if (td.Id.StartsWith("ArcFlash", StringComparison.OrdinalIgnoreCase)) return BuildDiffForComposite(td, ctx,
                ctx.NewData.ArcFlashEntries == null ? null : ctx.NewData.ArcFlashEntries.ToDictionary(k => CompositeKey(k.Key), v => v.Value, StringComparer.OrdinalIgnoreCase),
                ctx.OldData?.ArcFlashEntries == null ? null : ctx.OldData.ArcFlashEntries.ToDictionary(k => CompositeKey(k.Key), v => v.Value, StringComparer.OrdinalIgnoreCase));
            return new List<string[]>();
        }

        private static string CompositeKey(object keyTuple) => keyTuple.ToString() ?? string.Empty;

        private List<string[]> BuildDiffForComposite<T>(TableDefinition td, ProjectContext ctx, IDictionary<string, T>? newMapRaw, IDictionary<string, T>? oldMapRaw)
            => BuildDiffFor(td, ctx, newMapRaw, oldMapRaw);

        private List<string[]> BuildDiffFor<T>(TableDefinition td, ProjectContext ctx, IDictionary<string, T>? newMapRaw, IDictionary<string, T>? oldMapRaw)
        {
            var rows = new List<string[]>();
            var newMap = newMapRaw ?? new Dictionary<string, T>();
            var oldMap = oldMapRaw ?? new Dictionary<string, T>();
            var keys = new HashSet<string>(newMap.Keys, StringComparer.OrdinalIgnoreCase); foreach (var k in oldMap.Keys) keys.Add(k);
            foreach (var key in keys)
            {
                newMap.TryGetValue(key, out var newObj); oldMap.TryGetValue(key, out var oldObj);
                if (newObj != null && td.FilterSpecs != null && td.FilterSpecs.Count > 0 && !ApplyFilters(newObj!, td.FilterSpecs, td.FilterGroups)) continue;
                var newRow = newObj != null ? RenderRow(td, ctx, newObj!) : new string[td.Columns.Count];
                var oldRow = oldObj != null ? RenderRow(td, ctx, oldObj!) : new string[td.Columns.Count];
                var merged = new string[td.Columns.Count];
                for (int i = 0; i < merged.Length; i++)
                {
                    var a = newRow[i] ?? string.Empty; var b = oldRow[i] ?? string.Empty;
                    if (i == 0)
                    {
                        // Keep identifiers stable: use new when available else fall back to old
                        merged[i] = string.IsNullOrEmpty(a) ? b : a;
                    }
                    else
                    {
                        merged[i] = IsDifferent(a, b) ? a + DiffDefaults.DiffMarker + b : a;
                    }
                }
                rows.Add(merged);
            }
            return rows;
        }

        private static bool IsDifferent(string newVal, string oldVal)
        { string Strip(string s) => string.IsNullOrEmpty(s) ? string.Empty : Regex.Replace(s, "\\s+", string.Empty); return !string.Equals(Strip(newVal), Strip(oldVal), StringComparison.Ordinal); }

        private string[] RenderRow(TableDefinition td, ProjectContext ctx, object item)
            => td.Columns.Select(col =>
            {
                if (col.Renderer != null) return col.Renderer(ctx, item);
                if (!string.IsNullOrEmpty(col.Name)) { var prop = item.GetType().GetProperty(col.Name); if (prop != null) return prop.GetValue(item)?.ToString() ?? string.Empty; }
                return string.Empty;
            }).ToArray();

        private static bool ApplyFilters(object it, List<FilterSpec> filters, List<FilterGroup>? groups = null)
        {
            foreach (var f in filters)
            {
                if (!EvaluateFilter(it, f)) return false;
            }
            if (groups != null && groups.Count > 0)
            {
                foreach (var g in groups)
                {
                    bool groupResult = string.Equals(g.Logic, "OR", StringComparison.OrdinalIgnoreCase)
                        ? g.Filters.Any(f => EvaluateFilter(it, f))
                        : g.Filters.All(f => EvaluateFilter(it, f));
                    if (!groupResult) return false;
                }
            }
            return true;
        }

        private static bool ApplyFilters(object it, List<FilterSpec> filters, List<FilterGroup>? groups, out string? failReason)
        {
            foreach (var f in filters)
            {
                if (!EvaluateFilter(it, f, out failReason)) return false;
            }
            if (groups != null && groups.Count > 0)
            {
                foreach (var g in groups)
                {
                    if (string.Equals(g.Logic, "OR", StringComparison.OrdinalIgnoreCase))
                    {
                        bool any = false; string? lastReason = null;
                        foreach (var f in g.Filters)
                        {
                            if (EvaluateFilter(it, f, out lastReason)) { any = true; break; }
                        }
                        if (!any) { failReason = "Group(OR) all failed"; return false; }
                    }
                    else
                    {
                        foreach (var f in g.Filters)
                        {
                            if (!EvaluateFilter(it, f, out failReason)) return false;
                        }
                    }
                }
            }
            failReason = null; return true;
        }

        private static bool EvaluateFilter(object it, FilterSpec f) => EvaluateFilter(it, f, out _);
        private static bool EvaluateFilter(object it, FilterSpec f, out string? reason)
        {
            reason = null;
            var leftRaw = EvaluatePropertyPath(it, f.PropertyPath);
            string? rightRaw = null;
            if (!string.IsNullOrWhiteSpace(f.RightPropertyPath))
                rightRaw = EvaluatePropertyPath(it, f.RightPropertyPath);
            bool wantsNumeric = f.Numeric || (!string.IsNullOrWhiteSpace(f.RightPropertyPath));
            if (wantsNumeric)
            {
                if (!TryParseNumeric(leftRaw, out var lv)) { reason = $"numeric parse fail left '{f.PropertyPath}' value='{leftRaw}'"; return false; }
                double rv;
                if (rightRaw != null)
                {
                    if (!TryParseNumeric(rightRaw, out rv)) { reason = $"numeric parse fail right '{f.RightPropertyPath}' value='{rightRaw}'"; return false; }
                }
                else
                {
                    if (!TryParseNumeric(f.Value, out rv)) { reason = $"numeric parse fail literal '{f.Value}'"; return false; }
                }
                bool ok = f.Operator switch
                {
                    "eq" => lv == rv,
                    "neq" => lv != rv,
                    "lt" => lv < rv,
                    "lte" => lv <= rv,
                    "gt" => lv > rv,
                    "gte" => lv >= rv,
                    _ => false
                };
                if (!ok) reason = $"{f.PropertyPath} {f.Operator} {(rightRaw ?? f.Value)} failed (left={lv} right={rv})";
                return ok;
            }
            var left = leftRaw ?? string.Empty; var right = (rightRaw ?? f.Value) ?? string.Empty;
            if (f.IgnoreCase) { left = left.ToLowerInvariant(); right = right.ToLowerInvariant(); }
            bool res = f.Operator switch
            {
                "eq" => left == right,
                "neq" => left != right,
                "contains" => left.Contains(right),
                _ => false
            };
            if (!res) reason = $"{f.PropertyPath} {f.Operator} '{right}' failed (left='{leftRaw}')";
            return res;
        }

        private static bool TryParseNumeric(string? raw, out double value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(raw)) return false;
            raw = raw.Trim();
            // Pull first numeric token (handles leading number with trailing units e.g. "10 kA", "105%", "1.25sec")
            var m = System.Text.RegularExpressions.Regex.Match(raw, @"[-+]?\d*\.?\d+(?:[eE][-+]?\d+)?");
            if (m.Success && double.TryParse(m.Value, out value)) return true;
            return double.TryParse(raw, out value);
        }

        private static string RenderFormatInternal(object o, string template)
        {
            string withGroups = Regex.Replace(template, "<(.*?)>", m =>
            {
                var content = m.Groups[1].Value;
                var tokenMatches = Regex.Matches(content, "{([^}]+)}");
                bool allHaveValues = true; var tokenValues = new Dictionary<string, string>();
                foreach (Match tm in tokenMatches)
                {
                    var token = tm.Groups[1].Value;
                    var val = (EvaluatePropertyPathInternal(o, token) ?? string.Empty).Trim();
                    tokenValues[token] = val; if (string.IsNullOrWhiteSpace(val)) { allHaveValues = false; break; }
                }
                if (!allHaveValues) return string.Empty;
                foreach (var kv in tokenValues) content = content.Replace("{" + kv.Key + "}", kv.Value);
                return content;
            }, RegexOptions.Singleline);
            return ReplaceTokens(withGroups, o);
        }

        private static string ReplaceTokens(string template, object o)
        {
            var result = template; int safety = 0;
            while (true)
            {
                int sIdx = result.IndexOf('{'); if (sIdx < 0) break; int e = result.IndexOf('}', sIdx + 1); if (e < 0) break;
                var token = result.Substring(sIdx + 1, e - sIdx - 1);
                var val = (EvaluatePropertyPathInternal(o, token) ?? string.Empty).Trim();
                result = result.Substring(0, sIdx) + val + result.Substring(e + 1);
                if (++safety > 2000) break;
            }
            return result;
        }

        private static string CleanOutputInternal(string s)
        {
            var lines = s.Split('\n'); var cleaned = new List<string>();
            foreach (var line in lines)
            { var trimmed = line.TrimEnd(); if (trimmed.Length == 0 && (cleaned.Count == 0 || cleaned.Last().Length == 0)) continue; cleaned.Add(trimmed); }
            return string.Join('\n', cleaned).Trim();
        }

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, Dictionary<string, Func<object, object?>>> _accessorCache = new();
        private static Dictionary<string, Func<object, object?>> BuildAccessors(Type t)
        {
            var dict = new Dictionary<string, Func<object, object?>>(StringComparer.OrdinalIgnoreCase);
            foreach (var pi in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!pi.CanRead) continue;
                var param = System.Linq.Expressions.Expression.Parameter(typeof(object), "obj");
                var cast = System.Linq.Expressions.Expression.Convert(param, t);
                var prop = System.Linq.Expressions.Expression.Property(cast, pi);
                var box = System.Linq.Expressions.Expression.Convert(prop, typeof(object));
                var lambda = System.Linq.Expressions.Expression.Lambda<Func<object, object?>>(box, param).Compile();
                dict[pi.Name] = lambda;
            }
            return dict;
        }
        private static string EvaluatePropertyPathInternal(object obj, string path)
        {
            if (obj == null) return string.Empty;
            object? cur = obj;
            foreach (var rawPart in path.Split('.'))
            {
                if (cur == null) return string.Empty;
                var part = rawPart;
                int? idx = null;
                if (part.Contains('['))
                {
                    var i1 = part.IndexOf('['); var i2 = part.IndexOf(']');
                    if (i1 >= 0 && i2 > i1)
                    {
                        var inside = part.Substring(i1 + 1, i2 - i1 - 1);
                        part = part.Substring(0, i1);
                        if (int.TryParse(inside, out var ii)) idx = ii;
                    }
                }
                var type = cur.GetType();
                var accessors = _accessorCache.GetOrAdd(type, BuildAccessors);
                if (!accessors.TryGetValue(part, out var getter)) return string.Empty;
                cur = getter(cur);
                if (idx != null)
                {
                    if (cur is System.Collections.IEnumerable en && cur is not string)
                    {
                        var list = en.Cast<object>().ToList();
                        cur = (idx.Value >= 0 && idx.Value < list.Count) ? list[idx.Value] : null;
                    }
                    else cur = null;
                }
            }
            if (cur is bool b) return b ? "true" : "false";
            return cur?.ToString() ?? string.Empty;
        }

        // Re-introduced: Register table specifications from JSON file content.
        public void RegisterFromJson(string json)
        {
            var load = SpecLoader.LoadFromJson(json);
            _propertyMappings = load.PropertyMappings;
            if (_propertyMappings != null)
            {
                _logger.LogInformation($"[SpecLoader] Loaded {_propertyMappings.Count} property mappings");
            }
            foreach (var t in load.Tables) Register(t);
            foreach (var w in load.Warnings) _logger.LogWarning("[SpecLoader][Warn] " + w);
            foreach (var e in load.Errors) _logger.LogError("[SpecLoader][Error] " + e);
        }

        public void RegisterFromJsonStrict(string json)
        {
            var load = SpecLoader.LoadFromJson(json);
            if (load.HasErrors)
            {
                throw new SpecLoadException("Spec load failed: " + string.Join("; ", load.Errors));
            }
            _propertyMappings = load.PropertyMappings;
            if (_propertyMappings != null)
            {
                _logger.LogInformation("[SpecLoader] Loaded {count} property mappings (strict)", _propertyMappings.Count);
            }
            foreach (var t in load.Tables) Register(t);
            foreach (var w in load.Warnings) _logger.LogWarning("[SpecLoader][Warn] " + w);
        }

        // Condition evaluation wrappers (exposed for evaluator).
        internal static bool EvalRowCondition(RowConditionSpec rc, string[] renderedRow, TableDefinition td, int rowIndex, object? source)
        {
            // Minimal inline implementation replicating prior logic (simplified)
            if (rc == null) return false;
            if (string.IsNullOrWhiteSpace(rc.PropertyPath) || rc.PropertyPath.Equals("Any", StringComparison.OrdinalIgnoreCase)) return true;
            if (source != null)
            {
                var left = EvaluatePropertyPath(source, rc.PropertyPath) ?? string.Empty;
                string? right = !string.IsNullOrWhiteSpace(rc.RightPropertyPath) ? EvaluatePropertyPath(source, rc.RightPropertyPath!) : rc.Value;
                return EvaluateConditionCore(left, rc.Operator, right, rc.Numeric, rc.IgnoreCase);
            }
            return EvaluateConditionCore(string.Join("|", renderedRow), rc.Operator, rc.Value, rc.Numeric, rc.IgnoreCase);
        }
        internal static bool EvalCellCondition(CellConditionSpec cond, string cellText, object? source, bool renderedOnly = false)
        {
            if (cond == null) return false;
            if (cond.MatchRendered || renderedOnly || source == null || string.IsNullOrWhiteSpace(cond.PropertyPath))
                return EvaluateConditionCore(cellText, cond.Operator, cond.Value, cond.Numeric, cond.IgnoreCase);
            var left = EvaluatePropertyPath(source, cond.PropertyPath);
            return EvaluateConditionCore(left, cond.Operator, cond.Value, cond.Numeric, cond.IgnoreCase);
        }

        private static bool EvaluateConditionCore(string? left, string op, string? right, bool numeric, bool ignoreCase)
        {
            left ??= string.Empty; right ??= string.Empty;
            if (numeric)
            {
                if (!TryParseNumeric(left, out var lv) || !TryParseNumeric(right, out var rv)) return false;
                return op switch { "eq" => lv==rv, "neq"=> lv!=rv, "lt"=> lv<rv, "lte"=> lv<=rv, "gt"=> lv>rv, "gte"=> lv>=rv, _=> false };
            }
            if (ignoreCase) { left = left.ToLowerInvariant(); right = right.ToLowerInvariant(); }
            return op switch { "contains" => left.Contains(right), "eq" => left==right, "neq" => left!=right, _=> false };
        }

        // === Restored utilities (ScrapeTables & ApplyPropertyMappings) ===
        public static string ScrapeTables(string templatePath, string? existingSpecJsonPath = null, bool includeExamplesPerTable = true)
        {
            return GenerateSkeletonSpecFromTemplateInternal(templatePath, existingSpecJsonPath, includeExamplesPerTable);
        }

        private static string GenerateSkeletonSpecFromTemplateInternal(string templatePath, string? existingSpecJsonPath, bool includeExamplesPerTable)
        {
            var tables = DocxTemplate.ListTables(templatePath).ToList();
            var exampleSpecs = new List<TableSpec>();
            if (!string.IsNullOrWhiteSpace(existingSpecJsonPath) && System.IO.File.Exists(existingSpecJsonPath))
            {
                try
                {
                    var existingJson = System.IO.File.ReadAllText(existingSpecJsonPath);
                    var existing = JsonSerializer.Deserialize<TableSpec[]>(existingJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (existing != null) exampleSpecs.AddRange(existing);
                }
                catch { }
            }
            var sampleColumns = new List<ColumnSpec>();
            foreach (var ex in exampleSpecs)
            {
                foreach (var c in ex.Columns)
                {
                    sampleColumns.Add(new ColumnSpec
                    {
                        Header = c.Header,
                        Format = c.Format,
                        Name = c.Name,
                        PropertyPaths = c.PropertyPaths,
                        Literal = c.Literal,
                        Expression = c.Expression,
                        NumberFormat = c.NumberFormat,
                        MergeVertically = c.MergeVertically,
                        WidthPercent = c.WidthPercent
                    });
                }
            }
            if (sampleColumns.Count == 0)
            {
                sampleColumns.AddRange(new[]
                {
                    new ColumnSpec{ Header = "ID", PropertyPaths = new[]{"Id"}, WidthPercent=15, MergeVertically=true },
                    new ColumnSpec{ Header = "NAME", Format="{Name}", WidthPercent=20 },
                    new ColumnSpec{ Header = "DETAIL", Format="{Type}<\n({Style})>", WidthPercent=25 },
                    new ColumnSpec{ Header = "VALUE", Expression="({New}-{Old})/{Old}*100", NumberFormat="0.0", WidthPercent=15 },
                    new ColumnSpec{ Header = "COMMENTS", PropertyPaths = new[]{"Comments"}, WidthPercent=25 }
                });
            }
            int sampleColCursor = 0;
            var outSpecs = new List<TableSpec>();
            foreach (var t in tables)
            {
                var alt = t.Caption ?? t.Description ?? $"Table{t.Index}";
                var id = MakeIdFromAlt(alt, out var category);
                int colCount = Math.Max(1, t.Cols);
                var colList = new List<ColumnSpec>();
                for (int i = 0; i < colCount; i++)
                {
                    ColumnSpec sample;
                    if (includeExamplesPerTable && sampleColumns.Count > 0)
                    {
                        sample = sampleColumns[sampleColCursor % sampleColumns.Count];
                        sampleColCursor++;
                        sample = new ColumnSpec
                        {
                            Header = sample.Header + (colCount > 1 ? $"_{i+1}" : string.Empty),
                            Format = sample.Format,
                            Name = sample.Name,
                            PropertyPaths = sample.PropertyPaths,
                            Literal = sample.Literal,
                            Expression = sample.Expression,
                            NumberFormat = sample.NumberFormat,
                            MergeVertically = sample.MergeVertically && i == 0,
                            WidthPercent = null
                        };
                    }
                    else
                    {
                        sample = new ColumnSpec { Header = $"COL{i+1}", Literal = $"Sample {i+1}" };
                    }
                    colList.Add(sample);
                }
                double each = 100.0 / colList.Count;
                foreach (var c in colList) c.WidthPercent ??= each;
                var ts = new TableSpec
                {
                    Id = id,
                    AltText = alt,
                    Mode = category.Equals("Diff", StringComparison.OrdinalIgnoreCase) ? "diff" : "new",
                    AllowRowBreakAcrossPages = false,
                    EmptyMessage = $"No {alt.ToLowerInvariant()} data.",
                    Formatting = new TableFormattingSpec { FontName = "Arial", FontSize = 10, HorizontalAlignment = "left", VerticalAlignment = "center" },
                    Columns = colList.ToArray(),
                    SortSpecs = new[] { new SortSpec { Column = 1, Direction = "asc", Numeric = false } },
                    GlobalCellConditions = new[] { new CellConditionSpec { Operator = "contains", Value = "Was", Fill = "FFFF00", MatchRendered = true } },
                };
                if (outSpecs.Count == 0)
                {
                    ts.FilterSpecs = new[] { new FilterSpec { PropertyPath = "Id", Operator = "contains", Value = "ABC", Numeric = false } };
                }
                outSpecs.Add(ts);
            }
            return SerializeTableSpecsCompact(outSpecs);
        }

        private static string SerializeTableSpecsCompact(IEnumerable<TableSpec> specs)
        {
            string Q(string s) => "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n") + "\"";
            var sb = new System.Text.StringBuilder();
            sb.Append("[\n");
            int tIndex = 0; var specList = specs.ToList();
            foreach (var ts in specList)
            {
                if (tIndex > 0) sb.Append(",\n");
                sb.Append("  {");
                void Prop(string name, string value, bool comma = true) => sb.Append($"\n    \"{name}\": {value}" + (comma ? "," : string.Empty));
                Prop("Id", Q(ts.Id));
                Prop("AltText", Q(ts.AltText));
                if (!string.IsNullOrWhiteSpace(ts.Mode)) Prop("Mode", Q(ts.Mode));
                if (ts.AllowRowBreakAcrossPages != null) Prop("AllowRowBreakAcrossPages", ts.AllowRowBreakAcrossPages.Value.ToString().ToLowerInvariant());
                if (!string.IsNullOrWhiteSpace(ts.EmptyMessage)) Prop("EmptyMessage", Q(ts.EmptyMessage));
                if (ts.Formatting != null)
                {
                    var fmtParts = new List<string>();
                    if (!string.IsNullOrWhiteSpace(ts.Formatting.FontName)) fmtParts.Add($"\"FontName\": {Q(ts.Formatting.FontName!)}");
                    if (ts.Formatting.FontSize != null) fmtParts.Add($"\"FontSize\": {ts.Formatting.FontSize.Value}");
                    if (!string.IsNullOrWhiteSpace(ts.Formatting.HorizontalAlignment)) fmtParts.Add($"\"HorizontalAlignment\": {Q(ts.Formatting.HorizontalAlignment)}");
                    if (!string.IsNullOrWhiteSpace(ts.Formatting.VerticalAlignment)) fmtParts.Add($"\"VerticalAlignment\": {Q(ts.Formatting.VerticalAlignment)}");
                    Prop("Formatting", "{ " + string.Join(", ", fmtParts) + " }");
                }
                if (ts.SortSpecs != null && ts.SortSpecs.Length > 0)
                {
                    var sortStr = string.Join(", ", ts.SortSpecs.Select(s => $"{{ \"Column\": {s.Column}, \"Direction\": {Q(s.Direction)}, \"Numeric\": {s.Numeric.ToString().ToLowerInvariant()} }}"));
                    Prop("SortSpecs", "[ " + sortStr + " ]");
                }
                if (ts.FilterSpecs != null && ts.FilterSpecs.Length > 0)
                {
                    var filtStr = string.Join(", ", ts.FilterSpecs.Select(f => $"{{ \"PropertyPath\": {Q(f.PropertyPath)}, \"Operator\": {Q(f.Operator)}, \"Value\": {Q(f.Value ?? string.Empty)}, \"Numeric\": {f.Numeric.ToString().ToLowerInvariant()} }}"));
                    Prop("FilterSpecs", "[ " + filtStr + " ]");
                }
                if (ts.GlobalCellConditions != null && ts.GlobalCellConditions.Length > 0)
                {
                    var condStr = string.Join(", ", ts.GlobalCellConditions.Select(c => $"{{ \"Operator\": {Q(c.Operator)}, \"Value\": {Q(c.Value ?? string.Empty)}, \"MatchRendered\": {c.MatchRendered.ToString().ToLowerInvariant()}, \"Fill\": {Q(c.Fill ?? string.Empty)} }}"));
                    Prop("GlobalCellConditions", "[ " + condStr + " ]");
                }
                sb.Append("\n    \"Columns\": [\n");
                for (int i = 0; i < ts.Columns.Length; i++)
                {
                    var c = ts.Columns[i];
                    var parts = new List<string> { $"\"Header\": {Q(c.Header)}" };
                    if (!string.IsNullOrWhiteSpace(c.Format)) parts.Add($"\"Format\": {Q(c.Format)}");
                    if (c.PropertyPaths != null && c.PropertyPaths.Length > 0) parts.Add("\"PropertyPaths\": [ " + string.Join(", ", c.PropertyPaths.Select(Q)) + " ]");
                    if (!string.IsNullOrWhiteSpace(c.Literal)) parts.Add($"\"Literal\": {Q(c.Literal)}");
                    if (!string.IsNullOrWhiteSpace(c.Expression)) parts.Add($"\"Expression\": {Q(c.Expression)}");
                    if (!string.IsNullOrWhiteSpace(c.NumberFormat)) parts.Add($"\"NumberFormat\": {Q(c.NumberFormat)}");
                    if (c.MergeVertically) parts.Add("\"MergeVertically\": true");
                    if (c.WidthPercent != null) parts.Add($"\"WidthPercent\": {c.WidthPercent.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                    sb.Append("      { " + string.Join(", ", parts) + " }");
                    if (i < ts.Columns.Length - 1) sb.Append(",");
                    sb.Append("\n");
                }
                sb.Append("    ]\n  }");
                tIndex++;
            }
            sb.Append("\n]\n");
            return sb.ToString();
        }

        private static string MakeIdFromAlt(string alt, out string category)
        {
            category = "New";
            var baseClean = Regex.Replace(alt.Trim(), "[^A-Za-z0-9]+", ".").Trim('.');
            if (baseClean.Length == 0) baseClean = "Table" + Guid.NewGuid().ToString("N").Substring(0, 8);
            if (Regex.IsMatch(alt, "(?i)over|diff|change|modified")) category = "Diff";
            return baseClean;
        }

        public void ApplyPropertyMappings(string docxPath, string outputPath, string? mappingsJson = null, Project? project = null)
        {
            if (!string.IsNullOrWhiteSpace(mappingsJson))
            {
                try { _propertyMappings = JsonSerializer.Deserialize<List<PropertyMappingSpec>>(mappingsJson); }
                catch (Exception ex) { _logger.LogError($"[Mappings] JSON parse error: {ex.Message}"); }
            }
            if (_propertyMappings == null || _propertyMappings.Count == 0)
            {
                _logger.LogWarning("[Mappings] No property mappings defined");
                if (docxPath != outputPath) System.IO.File.Copy(docxPath, outputPath, true);
                return;
            }
            if (!string.Equals(System.IO.Path.GetFullPath(docxPath), System.IO.Path.GetFullPath(outputPath), StringComparison.OrdinalIgnoreCase))
            {
                System.IO.File.Copy(docxPath, outputPath, true);
            }
            var props = project?.Properties ?? new Dictionary<string,string>();
            using var wordDoc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Open(outputPath, true);
            var custom = wordDoc.CustomFilePropertiesPart ?? wordDoc.AddCustomFilePropertiesPart();
            if (custom.Properties == null) custom.Properties = new DocumentFormat.OpenXml.CustomProperties.Properties();
            foreach (var m in _propertyMappings)
            {
                if (!props.TryGetValue(m.ProjectProperty, out var value) || string.IsNullOrWhiteSpace(value))
                {
                    if (!string.IsNullOrWhiteSpace(m.DefaultValue) && !m.Required)
                    {
                        value = m.DefaultValue;
                        _logger.LogInformation($"[Mappings] Using default for '{m.ProjectProperty}' -> '{m.DocProperty}'");
                    }
                    else
                    {
                        var level = m.Required ? "Error" : "Warning";
                        var msg = m.Required ? $"Required project property '{m.ProjectProperty}' missing for mapping to '{m.DocProperty}'" : $"Missing project property '{m.ProjectProperty}' -> '{m.DocProperty}' (skipped)";
                        if (m.Required) _logger.LogError("[Mappings] " + msg); else _logger.LogWarning("[Mappings] " + msg);
                        continue;
                    }
                }
                try
                {
                    if (m.BuiltIn)
                    {
                        var pkgProps = wordDoc.PackageProperties;
                        bool handled = true;
                        switch (m.DocProperty.ToLowerInvariant())
                        {
                            case "title": pkgProps.Title = value; break;
                            case "subject": pkgProps.Subject = value; break;
                            case "author": pkgProps.Creator = value; break;
                            case "comments": pkgProps.Description = value; break;
                            case "status": pkgProps.ContentStatus = value; break;
                            case "category": pkgProps.Category = value; break;
                            default: handled = false; break;
                        }
                        if (handled) { _logger.LogInformation($"[Mappings] Set built-in '{m.DocProperty}'"); continue; }
                    }
                    var propsRoot = custom.Properties!;
                    var existing = propsRoot.Elements<DocumentFormat.OpenXml.CustomProperties.CustomDocumentProperty>()
                        .FirstOrDefault(p => string.Equals(p.Name?.Value, m.DocProperty, StringComparison.OrdinalIgnoreCase));
                    if (existing != null) existing.Remove();
                    int nextId = 2;
                    var last = propsRoot.Elements<DocumentFormat.OpenXml.CustomProperties.CustomDocumentProperty>().LastOrDefault();
                    if (last?.PropertyId != null) nextId = Math.Max(nextId, last.PropertyId.Value + 1);
                    var prop = new DocumentFormat.OpenXml.CustomProperties.CustomDocumentProperty
                    {
                        Name = m.DocProperty,
                        FormatId = "{D5CDD505-2E9C-101B-9397-08002B2CF9AE}",
                        PropertyId = nextId,
                        VTLPWSTR = new DocumentFormat.OpenXml.VariantTypes.VTLPWSTR(value)
                    };
                    propsRoot.Append(prop); propsRoot.Save();
                    _logger.LogInformation($"[Mappings] Set custom '{m.DocProperty}'");
                }
                catch (Exception ex) { _logger.LogError($"[Mappings] Failed mapping '{m.ProjectProperty}' -> '{m.DocProperty}': {ex.Message}"); }
            }
            wordDoc.Save();
        }
        // === End restored utilities ===

        public class PopulateResult
        {
            public bool Success { get; set; }
            public string OutputPath { get; set; } = string.Empty;
            public List<string> Warnings { get; set; } = new();
            public List<string> Errors { get; set; } = new();
            public int TablesRequested { get; set; }
            public int TablesPopulated { get; set; }
            public int TablesSkipped { get; set; }
            public RunSummary? Summary { get; set; }
            public string? FailReason { get; set; }
        }
    }

    public interface ITableEvaluator { TableEvalResult Evaluate(TableDefinition def, ProjectContext ctx); }
    public interface IDocTableRenderer { void Render(DocxTemplate tpl, TableDefinition def, TableEvalResult eval); }

    internal sealed class DefaultTableEvaluator : ITableEvaluator
    {
        private readonly EasyAFEngine _engine;
        public DefaultTableEvaluator(EasyAFEngine engine) { _engine = engine; }
        public TableEvalResult Evaluate(TableDefinition def, ProjectContext ctx)
        {
            var eval = _engine.EvaluateTable(def.Id, ctx);
            eval.MergeVerticalColumnIndexes = def.Columns.Select((c,i)=> new {c,i}).Where(x=>x.c.MergeVertically).Select(x=>x.i).ToArray();
            if ((def.RowConditions != null && def.RowConditions.Count > 0) || def.Columns.Any(c => c.Conditions != null && c.Conditions.Count > 0) || def.Mode == TableMode.Diff || (def.GlobalCellConditions!=null && def.GlobalCellConditions.Count>0))
            {
                var fills = new List<List<string?>>();
                var colors = new List<List<string?>>();
                for (int ri=0; ri<eval.Rows.Count; ri++)
                {
                    var row = eval.Rows[ri];
                    object? source = (def.LastSourceObjects!=null && ri < def.LastSourceObjects.Count) ? def.LastSourceObjects[ri] : null;
                    string? rowFill=null; string? rowColor=null;
                    if (def.RowConditions!=null)
                    {
                        foreach (var rc in def.RowConditions)
                        {
                            if (EasyAFEngine.EvalRowCondition(rc, row, def, ri, source))
                            {
                                if (rc.Target.Equals("Fill", System.StringComparison.OrdinalIgnoreCase) || rc.Target.Equals("Both", System.StringComparison.OrdinalIgnoreCase)) rowFill = rc.Fill;
                                if (!string.IsNullOrWhiteSpace(rc.TextColor) && (rc.Target.Equals("Text", System.StringComparison.OrdinalIgnoreCase) || rc.Target.Equals("Both", System.StringComparison.OrdinalIgnoreCase))) rowColor = rc.TextColor;
                                break;
                            }
                        }
                    }
                    var fillRow = new List<string?>();
                    var colorRow = new List<string?>();
                    for (int ci=0; ci<row.Length; ci++)
                    {
                        string? fill = rowFill; string? tcolor = rowColor;
                        var cellText = row[ci] ?? string.Empty;
                        if (def.Mode == TableMode.Diff && cellText.IndexOf(DiffDefaults.DiffMarker, System.StringComparison.OrdinalIgnoreCase) >=0)
                            fill = fill ?? "FFFF00";
                        if (def.GlobalCellConditions!=null)
                        {
                            foreach (var g in def.GlobalCellConditions)
                            {
                                if (EasyAFEngine.EvalCellCondition(g, cellText, source))
                                {
                                    if (g.ApplyToRow)
                                    {
                                        if (g.Target.Equals("Fill", System.StringComparison.OrdinalIgnoreCase) || g.Target.Equals("Both", System.StringComparison.OrdinalIgnoreCase)) { rowFill = fill = g.Fill; }
                                        if (!string.IsNullOrWhiteSpace(g.TextColor) && (g.Target.Equals("Text", System.StringComparison.OrdinalIgnoreCase) || g.Target.Equals("Both", System.StringComparison.OrdinalIgnoreCase))) { rowColor = tcolor = g.TextColor; }
                                    }
                                    else
                                    {
                                        if (g.Target.Equals("Fill", System.StringComparison.OrdinalIgnoreCase) || g.Target.Equals("Both", System.StringComparison.OrdinalIgnoreCase)) fill = g.Fill;
                                        if (!string.IsNullOrWhiteSpace(g.TextColor) && (g.Target.Equals("Text", System.StringComparison.OrdinalIgnoreCase) || g.Target.Equals("Both", System.StringComparison.OrdinalIgnoreCase))) tcolor = g.TextColor;
                                    }
                                }
                            }
                        }
                        var colDef = def.Columns[ci];
                        if (colDef.Conditions!=null)
                        {
                            foreach (var c in colDef.Conditions)
                            {
                                if (EasyAFEngine.EvalCellCondition(c, cellText, source))
                                {
                                    if (c.ApplyToRow)
                                    {
                                        if (c.Target.Equals("Fill", System.StringComparison.OrdinalIgnoreCase) || c.Target.Equals("Both", System.StringComparison.OrdinalIgnoreCase)) { rowFill = fill = c.Fill; }
                                        if (!string.IsNullOrWhiteSpace(c.TextColor) && (c.Target.Equals("Text", System.StringComparison.OrdinalIgnoreCase) || c.Target.Equals("Both", System.StringComparison.OrdinalIgnoreCase))) { rowColor = tcolor = c.TextColor; }
                                    }
                                    else
                                    {
                                        if (c.Target.Equals("Fill", System.StringComparison.OrdinalIgnoreCase) || c.Target.Equals("Both", System.StringComparison.OrdinalIgnoreCase)) fill = c.Fill;
                                        if (!string.IsNullOrWhiteSpace(c.TextColor) && (c.Target.Equals("Text", System.StringComparison.OrdinalIgnoreCase) || c.Target.Equals("Both", System.StringComparison.OrdinalIgnoreCase))) tcolor = c.TextColor;
                                    }
                                    break;
                                }
                            }
                        }
                        try {
                            fillRow.Add(rowFill ?? fill);
                            colorRow.Add(rowColor ?? tcolor);
                        } catch (Exception ex) { throw new TemplatePopulationException("Cell processing failed", ex, def.Id, ci); }
                    }
                    fills.Add(fillRow); eval.CellTextColors = colors;
                }
                eval.CellFills = fills; eval.CellTextColors = colors;
            }
            return eval;
        }
    }

    internal sealed class DocxTableRenderer : IDocTableRenderer
    {
        public void Render(DocxTemplate tpl, TableDefinition def, TableEvalResult eval)
        {
            var opts = def.Formatting ?? new TableFormattingOptions { LightWeight = true };
            if (eval.CellFills!=null && eval.CellFills.Count>0) opts.CellFills = eval.CellFills;
            if (eval.CellTextColors!=null && eval.CellTextColors.Count>0) opts.CellTextColors = eval.CellTextColors;
            tpl.ReplaceTableByAltText(def.AltText, eval.Rows, mergeColumnIndexes: eval.MergeVerticalColumnIndexes ?? Array.Empty<int>(), options: opts);
        }
    }

    public static class DiffDefaults
    {
        public const string DiffMarker = "\nWas\n";
    }
}
