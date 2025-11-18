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
using System.Text.RegularExpressions;

namespace EasyAF.Export
{
    /// <summary>
    /// Generates equipment duty labels (ShortCircuit entries) from an SDT-based Word template (e.g., Equip.dotx).
    /// Template authoring: create a prototype paragraph/table containing content controls (plain text) with Tag values:
    ///   ShortCircuit.Id, ShortCircuit.Bus, ShortCircuit.Scenario, ShortCircuit.FaultType, ShortCircuit.Voltage,
    ///   ShortCircuit.Phase, ShortCircuit.Manufacturer, ShortCircuit.Style, ShortCircuit.TestStandard,
    ///   ShortCircuit.RatingKA, ShortCircuit.DutyKA, ShortCircuit.DutyPercent, ShortCircuit.EquipmentTypeDisplay,
    ///   ShortCircuit.RatingDutyLine, Project.StudyDate, Project.Revision, etc.
    /// The prototype block(s) are cloned for each qualifying (non over-dutied) short circuit entry.
    /// </summary>
    public static class EquipmentDutyLabelGenerator
    {
        private static readonly Dictionary<string, PropertyInfo> ScProps = typeof(ShortCircuit)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, PropertyInfo> ProjectProps = typeof(Project)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

        private static readonly string[] DefaultTagOrder = new []
        {
            "ShortCircuit.Id",
            "ShortCircuit.RatingKA",
            "ShortCircuit.DutyKA",
            "Project.StudyDate"
        };
        // Removed UnitSuffixes & automatic unit appending to avoid duplicate units when template already supplies them.
        // If units are desired inline, add static text (e.g. " kA") in the template after the SDT.

        public sealed class EquipmentDutyLabelGenerationResult
        {
            public int EntriesProcessed { get; set; }
            public int PrototypeBlocks { get; set; }
            public int TagsReplaced { get; set; }
            public List<string> UnknownTags { get; set; } = new();
            public List<string> MissingMappedValues { get; set; } = new();
        }

        public static IEnumerable<ShortCircuit> SelectRepresentative(IEnumerable<ShortCircuit> entries)
            => entries.Where(e => e != null && !string.IsNullOrWhiteSpace(e.Id))
                      .GroupBy(e => e.Id!, StringComparer.OrdinalIgnoreCase)
                      .Select(g => g.First());
        
        /// <summary>
        /// Select one ShortCircuit entry per Bus, choosing the worst-case duty.
        /// Preference order per bus group:
        ///1) Highest numeric DutyKA (after stripping any trailing "kA").
        ///2) If no numeric DutyKA, any entry flagged WorstCase (e.g., "X", "Yes", "1").
        ///3) Fallback: first entry.
        /// </summary>
        public static IEnumerable<ShortCircuit> SelectWorstPerBus(IEnumerable<ShortCircuit> entries)
        {
            if (entries == null) yield break;
            var groups = entries.Where(e => e != null && !string.IsNullOrWhiteSpace(e.BusName))
                .GroupBy(e => e.BusName!.Trim(), StringComparer.OrdinalIgnoreCase);
            foreach (var g in groups)
            {
                // Try numeric OneTwoCycleDuty first
                ShortCircuit? numericMax = null; double maxDuty = double.MinValue;
                foreach (var e in g)
                {
                    if (TryParseDutyKA(e.OneTwoCycleDuty, out var d))
                    {
                        if (d > maxDuty) { maxDuty = d; numericMax = e; }
                    }
                }
                if (numericMax != null) { yield return numericMax; continue; }

                // Fallback to WorstCase flag
                var flagged = g.FirstOrDefault(e => IsWorstCaseFlag(e.WorstCase));
                if (flagged != null) { yield return flagged; continue; }

                // Final fallback: first
                yield return g.First();
            }
        }

        private static int ReplaceTagsForEntry(Project? project, ShortCircuit entry, IEnumerable<OpenXmlElement> scopeElements, EquipmentDutyLabelGenerationResult result, bool clearUnmatched)
        {
            var map = BuildValueMap(project, entry);
            int replaced = 0;
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
                    if (clearUnmatched) foreach (var t in sdt.Descendants<Text>()) t.Text = string.Empty;
                    result.UnknownTags.Add(tag);
                }
            }
            return replaced;
        }

        private static Dictionary<string,string?> BuildValueMap(Project? project, ShortCircuit sc)
        {
            var dict = new Dictionary<string,string?>(StringComparer.OrdinalIgnoreCase);
            string GetSc(string name) => ScProps.TryGetValue(name, out var p) ? (Format(p.GetValue(sc)) ?? string.Empty) : string.Empty;
            
            // Provide raw values only; template supplies units.
            var rawRating = SanitizeNumeric(GetSc("OneTwoCycleRating"));
            dict["ShortCircuit.Id"] = GetSc("BusName"); // ID points to BusName
            dict["ShortCircuit.BusName"] = GetSc("BusName");
            dict["ShortCircuit.Bus"] = GetSc("BusName"); // Legacy compatibility
            
            // Current property names
            dict["ShortCircuit.RatingKA"] = RoundWhole(rawRating); // legacy tag for backward compatibility
            dict["ShortCircuit.OneTwoCycleRating"] = RoundWhole(rawRating);
            dict["ShortCircuit.DutyKA"] = SanitizeNumeric(GetSc("OneTwoCycleDuty")); // legacy tag
            dict["ShortCircuit.OneTwoCycleDuty"] = SanitizeNumeric(GetSc("OneTwoCycleDuty"));
            dict["ShortCircuit.DutyPercent"] = GetSc("OneTwoCycleDutyPercent"); // legacy tag
            dict["ShortCircuit.OneTwoCycleDutyPercent"] = GetSc("OneTwoCycleDutyPercent");
            
            // Legacy mappings for old templates
            dict["ShortCircuit.HalfCycleRatingKA"] = RoundWhole(rawRating);
            dict["ShortCircuit.HalfCycleDutyKA"] = SanitizeNumeric(GetSc("OneTwoCycleDuty"));
            dict["ShortCircuit.HalfCycleDutyPercent"] = GetSc("OneTwoCycleDutyPercent");
            
            // Add all other properties
            foreach (var prop in ScProps.Values)
            {
                var tag = $"ShortCircuit.{prop.Name}";
                if (!dict.ContainsKey(tag))
                {
                    dict[tag] = Format(prop.GetValue(sc));
                }
            }
            
            if (project != null)
            {
                var globalFmt = GlobalSettings.Current.DateFormat;
                var effectiveFmt = string.IsNullOrWhiteSpace(project.PreferredDateFormat) ? globalFmt : project.PreferredDateFormat;
                var formattedStudy = string.IsNullOrWhiteSpace(effectiveFmt) ? project.StudyDate : project.StudyDate; // TODO: Apply format
                dict["Project.StudyDate"] = formattedStudy;
                
                // Add all other project properties
                foreach (var prop in ProjectProps.Values)
                {
                    var tag = $"Project.{prop.Name}";
                    if (!dict.ContainsKey(tag) && prop.PropertyType != typeof(DataSet))
                    {
                        dict[tag] = Format(prop.GetValue(project));
                    }
                }
            }
            return dict;
        }

        private static string SanitizeNumeric(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            var v = System.Text.RegularExpressions.Regex.Replace(value.Trim(), @"\s*(kA)+$", "", RegexOptions.IgnoreCase).Trim();
            return v;
        }

        private static string RoundWhole(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            // Accept comma/space trimmed value
            var v = value.Trim();
            if (double.TryParse(v, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d))
            {
                return Math.Round(d, 0, MidpointRounding.AwayFromZero).ToString("0", System.Globalization.CultureInfo.InvariantCulture);
            }
            return value; // leave as-is if not numeric
        }

        private static string? Format(object? val) => val == null ? string.Empty : (val is DateTime dt ? dt.ToString("yyyy-MM-dd") : val.ToString());
        private static SdtRun CreateTaggedRun(string tag)
        {
            var sdt = new SdtRun(); var props = new SdtProperties(); props.Append(new Tag{ Val = tag }); props.Append(new SdtAlias{ Val = tag }); sdt.Append(props);
            var content = new SdtContentRun(); content.Append(new Run(new Text($"[{tag}]"))); sdt.Append(content); return sdt;
        }
        private static string? GetTag(SdtElement sdt) => sdt.SdtProperties?.GetFirstChild<Tag>()?.Val?.Value ?? sdt.SdtProperties?.GetFirstChild<SdtAlias>()?.Val?.Value;
        private static bool IsKnownTag(string? tag) => !string.IsNullOrWhiteSpace(tag) && IsKnownPrefix(tag!);
        private static bool IsKnownPrefix(string tag) => DefaultTagOrder.Contains(tag, StringComparer.OrdinalIgnoreCase);

        private static void PrepareOutputFromTemplate(string templateName, string outputPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
            var templateExt = Path.GetExtension(templateName);
            var outputExt = Path.GetExtension(outputPath);
            using var ms = TemplateResources.OpenTemplate(templateName);
            if (string.Equals(templateExt, ".dotx", StringComparison.OrdinalIgnoreCase) && string.Equals(outputExt, ".docx", StringComparison.OrdinalIgnoreCase))
            {
                using var working = new System.IO.MemoryStream(); ms.CopyTo(working); working.Position = 0;
                using (var zip = new ZipArchive(working, ZipArchiveMode.Update, true))
                {
                    var ctEntry = zip.GetEntry("[Content_Types].xml");
                    if (ctEntry != null)
                    {
                        string xml; using (var es = ctEntry.Open()) using (var sr = new StreamReader(es)) xml = sr.ReadToEnd();
                        if (xml.Contains("template.main+xml", StringComparison.OrdinalIgnoreCase))
                        {
                            xml = xml.Replace("application/vnd.openxmlformats-officedocument.wordprocessingml.template.main+xml", "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml", StringComparison.OrdinalIgnoreCase);
                            ctEntry.Delete(); var newEntry = zip.CreateEntry("[Content_Types].xml"); using var sw = new StreamWriter(newEntry.Open()); sw.Write(xml);
                        }
                    }
                }
                File.WriteAllBytes(outputPath, working.ToArray());
            }
            else
            {
                using var fs = File.Create(outputPath); ms.Position = 0; ms.CopyTo(fs);
            }
        }
        
        private static bool TryParseDutyKA(string? dutyKA, out double value)
        {
            value =0;
            if (string.IsNullOrWhiteSpace(dutyKA)) return false;
            var sanitized = SanitizeNumeric(dutyKA);
            return double.TryParse(sanitized, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value);
        }

        private static bool IsWorstCaseFlag(string? worst)
        {
            if (string.IsNullOrWhiteSpace(worst)) return false;
            var t = worst.Trim();
            return t.Equals("X", StringComparison.OrdinalIgnoreCase)
                || t.Equals("Yes", StringComparison.OrdinalIgnoreCase)
                || t.Equals("1", StringComparison.OrdinalIgnoreCase)
                || t.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        public static EquipmentDutyLabelGenerationResult Generate(Project? project, IEnumerable<ShortCircuit> entries, string templateName, string outputPath, bool nonOverDutiedOnly = true, bool clearUnmatchedTags = true, bool representativePerId = true, bool pageBreakBetween = false)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));
            // representativePerId now means: worst-case per Bus only (dedup by Bus)
            var baseList = representativePerId ? SelectWorstPerBus(entries).ToList() : entries.ToList();
            if (nonOverDutiedOnly) baseList = baseList.Where(e => e != null && !e.IsOverDutied()).ToList();
            if (baseList.Count ==0) throw new InvalidOperationException("No ShortCircuit entries matched criteria (possibly all filtered out).");

            PrepareOutputFromTemplate(templateName, outputPath);
            var result = new EquipmentDutyLabelGenerationResult();
            using var doc = WordprocessingDocument.Open(outputPath, true);
            var main = doc.MainDocumentPart?.Document ?? throw new InvalidOperationException("Template main document part missing.");
            if (main.Body == null) main.AppendChild(new Body());
            var body = main.Body!;

            var prototypeElements = body.ChildElements.Select(e => (OpenXmlElement)e).ToList();
            if (prototypeElements.Count ==0)
                throw new InvalidOperationException("Template body empty – add layout with SDT tags (ShortCircuit.* / Project.*) before generation.");
            if (!prototypeElements.Any(e => e.Descendants<SdtElement>().Any()))
                throw new InvalidOperationException("Template does not contain any SDT content controls. Add SDTs with tags: " + string.Join(", ", DefaultTagOrder));

            result.PrototypeBlocks = prototypeElements.Count;
            // First entry: replace in-place
            result.TagsReplaced += ReplaceTagsForEntry(project, baseList[0], prototypeElements, result, clearUnmatchedTags);

            // Subsequent entries: clone prototype blocks
            for (int i =1; i < baseList.Count; i++)
            {
                if (pageBreakBetween)
                {
                    body.AppendChild(new Paragraph(new Run(new Break { Type = BreakValues.Page }))); // page break
                }
                var clones = prototypeElements.Select(el => (OpenXmlElement)el.CloneNode(true)).ToList();
                foreach (var c in clones) body.AppendChild(c);
                result.TagsReplaced += ReplaceTagsForEntry(project, baseList[i], clones, result, clearUnmatchedTags);
            }
            result.EntriesProcessed = baseList.Count;
            main.Save();
            return result;
        }

        public static EquipmentDutyLabelGenerationResult GenerateOverDutied(Project? project, IEnumerable<ShortCircuit> entries, string templateName, string outputPath, bool clearUnmatchedTags = true, bool representativePerId = true, bool pageBreakBetween = false)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));
            // Worst-case per Bus among over-dutied if representativePerId, else all over-dutied
            var filtered = entries.Where(e => e != null && e.IsOverDutied());
            var list = representativePerId ? SelectWorstPerBus(filtered).ToList() : filtered.ToList();
            if (list.Count ==0) throw new InvalidOperationException("No over-dutied ShortCircuit entries found.");
            return Generate(project, list, templateName, outputPath, nonOverDutiedOnly: false, clearUnmatchedTags: clearUnmatchedTags, representativePerId: false, pageBreakBetween: pageBreakBetween);
        }
    }
}
