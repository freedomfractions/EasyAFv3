using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.IO.Compression;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml; // provides OpenXmlElement
using EasyAF.Data.Models;

namespace EasyAF.Export
{
    public static class BreakerLabelGenerator
    {
        private static readonly Dictionary<string, PropertyInfo> LvBreakerProps = typeof(LVBreaker)
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);
        
        private static readonly Dictionary<string, PropertyInfo> ProjectProps = typeof(Project)
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

        // Units (presentation only) - updated to use LVBreaker property names
        private static readonly Dictionary<string, string> UnitSuffixes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["LVBreaker.FrameA"] = "A",
            ["LVBreaker.TripA"] = "A",
            ["LVBreaker.STPUA"] = "A",
            ["LVBreaker.InstA"] = "A",
            ["LVBreaker.MaintA"] = "A",
            ["LVBreaker.GndA"] = "A",
            ["LVBreaker.PlugTapTrip"] = "A",
            // Legacy mappings for backward compatibility
            ["LVCB.FrameSize"] = "A",
            ["LVCB.FrameRating"] = "A",
            ["LVCB.InterruptRating"] = "kA",
            ["LVCB.TripUnitTripPlug"] = "A",
            ["LVCB.TripUnitLtpuAmps"] = "A",
            ["LVCB.TripUnitStpuAmps"] = "A",
            ["LVCB.TripUnitInstAmps"] = "A",
            ["LVCB.TripUnitMaintAmps"] = "A",
            ["LVCB.TripUnitGfpuAmps"] = "A",
            // Legacy TripUnit.* tags (for backward compatibility with old templates)
            ["TripUnit.TripPlug"] = "A",
            ["TripUnit.LtpuAmps"] = "A",
            ["TripUnit.StpuAmps"] = "A",
            ["TripUnit.InstAmps"] = "A",
            ["TripUnit.MaintAmps"] = "A",
            ["TripUnit.GfpuAmps"] = "A"
        };

        // Fields that are considered strictly indicative of adjustability (heuristic Option A)
        // Updated to use new LVBreaker property names
        private static readonly string[] StrictIndicatorPropertyNames = new[]
        {
            "LTPUMult", "LtdBand", "LtCurve",
            "STPUSetting", "STPUBand", "STPUI2t",
            "InstSetting", "TripAdjust", "InstA",
            "MaintSetting", "GndA", "GndDelay", "GndI2t"
        };
        private static readonly HashSet<string> StrictIndicatorSet = new(StrictIndicatorPropertyNames, StringComparer.OrdinalIgnoreCase);

        private static readonly string[] AlwaysIgnoreIndicatorNames = new[]
        {
            "TripA", "STPUA", "MaintA", "PlugTapTrip", "GndPickup"
        };
        private static readonly HashSet<string> IgnoreIndicatorSet = new(AlwaysIgnoreIndicatorNames, StringComparer.OrdinalIgnoreCase);

        /// <summary>Result/summary of label generation.</summary>
        public sealed class BreakerLabelGenerationResult
        {
            public int BreakersProcessed { get; set; }
            public int PrototypeBlocks { get; set; }
            public int TagsReplaced { get; set; }
            public List<string> UnknownTags { get; set; } = new();
            public List<string> MissingMappedValues { get; set; } = new();
            // Map of Breaker.Id (or generated index) -> indicators that triggered adjustability (only populated when adjustableOnly filtering is applied)
            public Dictionary<string, List<string>> AdjustableTriggerMap { get; set; } = new();
        }

        /// <summary>
        /// Generate a labels document for the provided breakers using the embedded template (e.g. "Breaker.dotx").
        /// </summary>
        /// <param name="project">Optional project to supply Project.* tags.</param>
        /// <param name="breakers">Breakers to include.</param>
        /// <param name="templateName">Template file name (embedded resource lookup, case-insensitive).</param>
        /// <param name="outputPath">Destination .docx path.</param>
        /// <param name="adjustableOnly">If true include only breakers with adjustable trip units (auto inferred from settings).</param>
        /// <param name="clearUnmatchedTags">If true clears (empties) any SDT whose tag cannot be resolved; otherwise leaves placeholder text.</param>
        public static BreakerLabelGenerationResult GenerateBreakerLabels(Project? project, IEnumerable<LVBreaker> breakers, string templateName, string outputPath, bool adjustableOnly = true, bool clearUnmatchedTags = true)
        {
            if (breakers == null) throw new ArgumentNullException(nameof(breakers));
            var result = new BreakerLabelGenerationResult();
            var all = breakers.ToList();
            List<LVBreaker> filtered;
            if (adjustableOnly)
            {
                filtered = new List<LVBreaker>();
                foreach (var b in all)
                {
                    if (b == null) continue;
                    if (IsEffectivelyAdjustable(b, out var indicators))
                    {
                        filtered.Add(b);
                        var key = b.Id ?? ($"IDX_{filtered.Count}");
                        result.AdjustableTriggerMap[key] = indicators;
                    }
                }
            }
            else filtered = all.Where(b=>b!=null).ToList()!;

            if (filtered.Count == 0)
                throw new InvalidOperationException("No breakers matched criteria (adjustableOnly filter may have excluded all).");
            result.BreakersProcessed = filtered.Count;

            PrepareOutputFromTemplate(templateName, outputPath);

            using var doc = WordprocessingDocument.Open(outputPath, true);
            var main = doc.MainDocumentPart?.Document ?? throw new InvalidOperationException("Template main document part missing.");
            var body = main.Body ?? throw new InvalidOperationException("Template body missing.");

            var prototypeBlocks = body.ChildElements
                .Where(e => (e is Paragraph || e is Table) && e.Descendants<SdtElement>().Any(s => IsKnownTag(GetTag(s))))
                .ToList();
            if (prototypeBlocks.Count == 0)
                throw new InvalidOperationException("Template does not contain any SDT content controls with LVBreaker./TripUnit./Project. tags.");
            result.PrototypeBlocks = prototypeBlocks.Count;

            result.TagsReplaced += ReplaceTagsForBreaker(project, filtered[0], prototypeBlocks, result, clearUnmatchedTags);
            for (int i = 1; i < filtered.Count; i++)
            {
                var clones = prototypeBlocks.Select(b => (OpenXmlElement)b.CloneNode(true)).ToList();
                foreach (var c in clones) body.Append(c);
                result.TagsReplaced += ReplaceTagsForBreaker(project, filtered[i], clones, result, clearUnmatchedTags);
            }
            main.Save();
            return result;
        }

        /// <summary>
        /// Overload for backward compatibility (no Project metadata tags).
        /// </summary>
        public static BreakerLabelGenerationResult GenerateBreakerLabels(IEnumerable<LVBreaker> breakers, string templateName, string outputPath, bool adjustableOnly = true)
            => GenerateBreakerLabels(null, breakers, templateName, outputPath, adjustableOnly, clearUnmatchedTags: true);

        /// <summary>
        /// Determines if a breaker has an adjustable trip unit based on heuristics (Option A).
        /// Checks Trip field and presence of meaningful trip unit settings.
        /// </summary>
        /// <param name="lvBreaker">The breaker to check.</param>
        /// <param name="indicators">Output list of property names that indicate adjustability.</param>
        /// <returns>True if the breaker is considered adjustable, otherwise false.</returns>
        public static bool IsEffectivelyAdjustable(LVBreaker lvBreaker, out List<string> indicators)
        {
            indicators = new();
            if (lvBreaker == null) return false;

            // Check explicit adjustable flag from Trip field (if it contains "Adj", "Adjustable", etc.)
            bool flagged = false;
            if (!string.IsNullOrWhiteSpace(lvBreaker.Trip))
            {
                var adj = lvBreaker.Trip.Trim().ToLowerInvariant();
                flagged = adj.Contains("adj") || adj.Contains("adjustable") || adj.Contains("electronic");
            }

            // Collect strict indicators regardless of current flag
            foreach (var name in StrictIndicatorPropertyNames)
            {
                var prop = typeof(LVBreaker).GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop == null) continue;
                var valObj = prop.GetValue(lvBreaker);
                if (valObj is string s && IsMeaningfulValue(s))
                {
                    // special handling: InstSetting counted only if not literal "fixed"
                    if (string.Equals(name, "InstSetting", StringComparison.OrdinalIgnoreCase) && string.Equals(s.Trim(), "fixed", StringComparison.OrdinalIgnoreCase))
                        continue;
                    indicators.Add(name);
                }
            }

            // Determine effective adjustability: explicit flag OR indicators present
            bool indicated = indicators.Count > 0;
            if (!(flagged || indicated)) return false;

            // If InstSetting is fixed AND only indicator is InstA -> treat as not adjustable
            if (string.Equals(lvBreaker.InstSetting?.Trim(), "fixed", StringComparison.OrdinalIgnoreCase) && 
                indicators.Count == 1 && 
                indicators[0].Equals("InstA", StringComparison.OrdinalIgnoreCase))
            {
                indicators.Clear();
                return false;
            }
            return true;
        }

        private static bool IsMeaningfulValue(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            var t = s.Trim();
            if (t.Length == 0) return false;
            if (string.Equals(t, "fixed", StringComparison.OrdinalIgnoreCase)) return true; // 'fixed' is meaningful for Inst (handled separately) but not for others
            // treat placeholders as not meaningful
            if (t == "0" || t.Equals("n/a", StringComparison.OrdinalIgnoreCase) || t.Equals("na", StringComparison.OrdinalIgnoreCase) || t.Equals("none", StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        /// <summary>
        /// Creates the output file from the embedded template. If the template is a .dotx and the output uses .docx
        /// we normalize the content type from template to document to avoid Word "corruption" repair prompts.
        /// </summary>
        private static void PrepareOutputFromTemplate(string templateName, string outputPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
            var templateExt = Path.GetExtension(templateName);
            var outputExt = Path.GetExtension(outputPath);
            using var ms = TemplateResources.OpenTemplate(templateName); // returns raw bytes of template
            if (string.Equals(templateExt, ".dotx", StringComparison.OrdinalIgnoreCase) && string.Equals(outputExt, ".docx", StringComparison.OrdinalIgnoreCase))
            {
                using var working = new MemoryStream();
                ms.CopyTo(working); working.Position = 0;
                using (var zip = new ZipArchive(working, ZipArchiveMode.Update, true))
                {
                    var ctEntry = zip.GetEntry("[Content_Types].xml");
                    if (ctEntry != null)
                    {
                        string xml;
                        using (var entryStream = ctEntry.Open())
                        using (var sr = new StreamReader(entryStream)) xml = sr.ReadToEnd();
                        if (xml.Contains("template.main+xml", StringComparison.OrdinalIgnoreCase))
                        {
                            xml = xml.Replace("application/vnd.openxmlformats-officedocument.wordprocessingml.template.main+xml", "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml", StringComparison.OrdinalIgnoreCase);
                            ctEntry.Delete();
                            var newEntry = zip.CreateEntry("[Content_Types].xml");
                            using var sw = new StreamWriter(newEntry.Open());
                            sw.Write(xml);
                        }
                    }
                }
                File.WriteAllBytes(outputPath, working.ToArray());
            }
            else
            {
                using var fs = File.Create(outputPath);
                ms.Position = 0; ms.CopyTo(fs);
            }
        }

        private static int ReplaceTagsForBreaker(Project? project, LVBreaker breaker, IEnumerable<OpenXmlElement> scopeElements, BreakerLabelGenerationResult result, bool clearUnmatched)
        {
            int replaced = 0;
            var map = BuildValueMap(project, breaker);
            foreach (var sdt in scopeElements.SelectMany(e => e.Descendants<SdtElement>()))
            {
                var tag = GetTag(sdt);
                if (string.IsNullOrWhiteSpace(tag)) continue;
                if (map.TryGetValue(tag, out var value))
                {
                    foreach (var t in sdt.Descendants<Text>()) t.Text = value ?? string.Empty;
                    replaced++;
                    if (string.IsNullOrEmpty(value)) result.MissingMappedValues.Add(tag);
                }
                else if (IsKnownPrefix(tag))
                {
                    // Recognized prefix but property not found
                    if (clearUnmatched) foreach (var t in sdt.Descendants<Text>()) t.Text = string.Empty;
                    result.UnknownTags.Add(tag);
                }
            }
            return replaced;
        }

        private static Dictionary<string, string?> BuildValueMap(Project? project, LVBreaker b)
        {
            var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            if (b != null)
            {
                // Add all LVBreaker properties
                foreach (var kvp in LvBreakerProps)
                {
                    var raw = kvp.Value.GetValue(b);
                    var propName = kvp.Value.Name;
                    
                    // Add as LVBreaker.PropertyName
                    var lvBreakerTag = $"LVBreaker.{propName}";
                    var formatted = Format(raw);
                    formatted = AppendUnitIfNeeded(lvBreakerTag, formatted);
                    dict[lvBreakerTag] = formatted;
                    
                    // Legacy LVCB.* compatibility mappings
                    var legacyTag = $"LVCB.{propName}";
                    dict[legacyTag] = formatted;
                    
                    // Map new property names to legacy TripUnit.* names for backward compatibility
                    if (propName.StartsWith("Trip") || propName.StartsWith("LTPU") || propName.StartsWith("STPU") || 
                        propName.StartsWith("Inst") || propName.StartsWith("Maint") || propName.StartsWith("Gnd"))
                    {
                        // Map to TripUnit.* for old templates
                        var tripUnitTag = $"TripUnit.{propName}";
                        dict[tripUnitTag] = formatted;
                    }
                }
                
                AddTripModuleDerivedTags(b, dict);

                // Effective manufacturer fallbacks
                var effectiveBreakerMfr = !string.IsNullOrWhiteSpace(b.BreakerMfr) ? b.BreakerMfr!.Trim() : b.TripMfr?.Trim();
                var effectiveTripMfr = !string.IsNullOrWhiteSpace(b.TripMfr)
                    ? b.TripMfr!.Trim()
                    : effectiveBreakerMfr;

                // Expose effective manufacturers explicitly
                dict["LVBreaker.ManufacturerEffective"] = effectiveBreakerMfr ?? string.Empty;
                dict["LVBreaker.BreakerMfrEffective"] = effectiveBreakerMfr ?? string.Empty;
                dict["TripUnit.ManufacturerEffective"] = effectiveTripMfr ?? string.Empty;
                dict["TripUnit.TripMfrEffective"] = effectiveTripMfr ?? string.Empty;
                // Legacy
                dict["LVCB.ManufacturerEffective"] = effectiveBreakerMfr ?? string.Empty;

                // Composite display tags using effective manufacturers
                var breakerTypeDisplay = ComposeBreakerTypeDisplay(effectiveBreakerMfr, b.BreakerStyle);
                dict["LVBreaker.BreakerTypeDisplay"] = breakerTypeDisplay;
                dict["LVCB.BreakerTypeDisplay"] = breakerTypeDisplay; // Legacy

                // Trip unit type display
                var hasTripUnitData = !string.IsNullOrWhiteSpace(b.TripType) || !string.IsNullOrWhiteSpace(b.TripStyle);
                if (hasTripUnitData)
                {
                    var tripTypeDisplay = ComposeBreakerTypeDisplay(effectiveTripMfr, b.TripStyle ?? b.TripType);
                    dict["TripUnit.TripTypeDisplay"] = tripTypeDisplay;
                    dict["LVBreaker.TripTypeDisplay"] = tripTypeDisplay;
                }
                else
                {
                    dict["TripUnit.TripTypeDisplay"] = string.Empty;
                    dict["LVBreaker.TripTypeDisplay"] = string.Empty;
                }
            }
            if (project != null)
            {
                foreach (var kvp in ProjectProps)
                {
                    var pt = kvp.Value.PropertyType;
                    if (pt == typeof(DataSet)) continue;
                    var raw = kvp.Value.GetValue(project);
                    var tag = $"Project.{kvp.Value.Name}";
                    var formatted = Format(raw);
                    dict[tag] = formatted;
                }
                if (project.Properties != null)
                {
                    foreach (var pkvp in project.Properties)
                    {
                        var tag = $"Project.Prop_{pkvp.Key}";
                        dict[tag] = pkvp.Value;
                    }
                }
            }
            return dict;
        }

        // Implements legacy VBA logic for trip module descriptor (tmpStr / tmpStr2)
        private static void AddTripModuleDerivedTags(LVBreaker breaker, Dictionary<string, string?> dict)
        {
            var hasTripUnitData = !string.IsNullOrWhiteSpace(breaker.TripType) || !string.IsNullOrWhiteSpace(breaker.TripStyle);
            if (!hasTripUnitData)
            {
                dict["LVBreaker.MfrStyleLabel"] = (breaker.BreakerMfr + " " + breaker.BreakerStyle).Trim();
                dict["LVCB.MfrStyleLabel"] = dict["LVBreaker.MfrStyleLabel"]; // Legacy
                dict["TripUnit.TripModuleInfo"] = string.Empty;
                return;
            }
            
            // Base label: BreakerMfr + BreakerStyle
            string baseLabel = string.Join(" ", new[]{breaker.BreakerMfr, breaker.BreakerStyle}.Where(s=>!string.IsNullOrWhiteSpace(s))).Trim();
            string? tripType = breaker.TripType?.Trim();
            string? tripStyle = breaker.TripStyle?.Trim();
            bool hasTripType = !string.IsNullOrWhiteSpace(tripType);
            bool hasTripStyle = !string.IsNullOrWhiteSpace(tripStyle);
            string tripTypeLower = (tripType ?? string.Empty).Trim().ToLowerInvariant();
            bool tripTypeIsStd = tripTypeLower == "(std)";
            
            // Determine if style already contains trip style (case-insensitive contains)
            bool styleContainsTripStyle = false;
            if (hasTripStyle && !string.IsNullOrWhiteSpace(breaker.BreakerStyle))
            {
                styleContainsTripStyle = breaker.BreakerStyle!.IndexOf(tripStyle!, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            
            // Show trip module only if breaker considered adjustable by heuristic
            bool isAdjustable = IsEffectivelyAdjustable(breaker, out _);
            string moduleInfo = string.Empty;
            
            if (isAdjustable)
            {
                // If tripType not (std) OR style does not already contain tripStyle add "/" + newline separator
                if ((!tripTypeIsStd) || !styleContainsTripStyle)
                {
                    baseLabel += "/" + Environment.NewLine;
                }
                // Append tripType if not (std)
                if (hasTripType && !tripTypeIsStd)
                {
                    baseLabel += tripType;
                }
                // Append tripStyle if not already present inside breaker style
                string tmpStr2 = string.Empty;
                if (hasTripStyle && !styleContainsTripStyle)
                {
                    if (!tripTypeIsStd && hasTripType) baseLabel += " ";
                    baseLabel += tripStyle;
                    tmpStr2 = tripTypeIsStd ? string.Empty : tripType ?? string.Empty;
                }
                // Build moduleInfo
                if (hasTripStyle)
                {
                    var pref = string.IsNullOrWhiteSpace(breaker.BreakerMfr) ? string.Empty : breaker.BreakerMfr!.Trim() + " ";
                    if (!string.IsNullOrWhiteSpace(tmpStr2))
                        moduleInfo = $"{pref}{tripStyle} ({tmpStr2})";
                    else
                        moduleInfo = $"{pref}{tripStyle} Trip Module";
                }
            }
            dict["LVBreaker.MfrStyleLabel"] = baseLabel;
            dict["LVCB.MfrStyleLabel"] = baseLabel; // Legacy
            dict["TripUnit.TripModuleInfo"] = moduleInfo;
        }

        private static string? AppendUnitIfNeeded(string tag, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            if (!UnitSuffixes.TryGetValue(tag, out var unit) || string.IsNullOrWhiteSpace(unit)) return value;
            // Avoid double appending if already ends with unit (case-insensitive) or non-digit final char
            var trimmed = value.Trim();
            if (!char.IsDigit(trimmed[^1])) return value; // only append when last char is digit
            if (trimmed.EndsWith(unit, StringComparison.OrdinalIgnoreCase)) return value; // already present
            return trimmed + unit; // no space (e.g. 400A)
        }

        private static string? Format(object? val)
        {
            if (val == null) return string.Empty;
            if (val is bool b) return b ? "True" : "False";
            if (val is DateTime dt) return dt.ToString("yyyy-MM-dd");
            return val.ToString();
        }

        private static string? GetTag(SdtElement sdt)
        {
            return sdt.SdtProperties?.GetFirstChild<Tag>()?.Val?.Value
                ?? sdt.SdtProperties?.GetFirstChild<SdtAlias>()?.Val?.Value;
        }

        private static bool IsKnownTag(string? tag) => !string.IsNullOrWhiteSpace(tag) && IsKnownPrefix(tag!);
        private static bool IsKnownPrefix(string tag) => tag.StartsWith("LVBreaker.", StringComparison.OrdinalIgnoreCase) || tag.StartsWith("TripUnit.", StringComparison.OrdinalIgnoreCase) || tag.StartsWith("Project.", StringComparison.OrdinalIgnoreCase);

        private static string ComposeBreakerTypeDisplay(string? manufacturer, string? style)
        {
            var m = (manufacturer ?? string.Empty).Trim();
            var s = (style ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(m)) return s; // no manufacturer
            if (string.IsNullOrEmpty(s)) return m; // no style
            // Avoid duplicating manufacturer if already at start of style or contained (case-insensitive)
            if (s.StartsWith(m + " ", StringComparison.OrdinalIgnoreCase) || string.Equals(s, m, StringComparison.OrdinalIgnoreCase))
                return s; // already prefixed
            return m + " " + s;
        }
    }
}

