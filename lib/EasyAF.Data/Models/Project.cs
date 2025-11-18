using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace EasyAF.Data.Models
{
    /// <summary>
    /// Represents a complete project, including all study data, device entries, and project metadata.
    /// </summary>
    public class Project
    {
        /// <summary>Date of the study.</summary>
        public string? StudyDate { get; set; }
        /// <summary>Revision identifier for the project.</summary>
        public string? Revision { get; set; }
        /// <summary>Project number assigned by LB.</summary>
        public string? LBProjectNumber { get; set; }
        /// <summary>Client name.</summary>
        public string? Client { get; set; }
        /// <summary>Name of the site.</summary>
        public string? SiteName { get; set; }
        /// <summary>Legacy single-line address (kept for backward compatibility).</summary>
        public string? SiteAddress { get; set; }
        /// <summary>Address line 1.</summary>
        public string? AddressLine1 { get; set; }
        /// <summary>Address line 2.</summary>
        public string? AddressLine2 { get; set; }
        /// <summary>Address line 3.</summary>
        public string? AddressLine3 { get; set; }
        /// <summary>City.</summary>
        public string? City { get; set; }
        /// <summary>State/Province.</summary>
        public string? State { get; set; }
        /// <summary>ZIP/Postal Code.</summary>
        public string? Zip { get; set; }
        /// <summary>Name of the study engineer.</summary>
        public string? StudyEngineer { get; set; }
        /// <summary>Additional comments or notes.</summary>
        public string? Comments { get; set; }
        /// <summary>Optional per-project preferred date format template (overrides global settings.json DateFormat if provided).</summary>
        public string? PreferredDateFormat { get; set; }

        /// <summary>Data set for new entries.</summary>
        public DataSet? NewData { get; set; } = new DataSet();
        /// <summary>Data set for old entries.</summary>
        public DataSet? OldData { get; set; } = new DataSet();
        /// <summary>Project log entries (timestamped).</summary>
        public Dictionary<long, string>? ProjectLog { get; set; } =
            new Dictionary<long, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Project"/> class.
        /// </summary>
        public Project() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Project"/> class with all metadata properties.
        /// </summary>
        public Project(
            string? studyDate,
            string? revision,
            string? lbProjectNumber,
            string? client,
            string? siteName,
            string? siteAddress,
            string? studyEngineer,
            string? comments,
            string? preferredDateFormat = null,
            string? addressLine1 = null,
            string? addressLine2 = null,
            string? addressLine3 = null,
            string? city = null,
            string? state = null,
            string? zip = null)
        {
            StudyDate = studyDate;
            Revision = revision;
            LBProjectNumber = lbProjectNumber;
            Client = client;
            SiteName = siteName;
            SiteAddress = siteAddress;
            StudyEngineer = studyEngineer;
            Comments = comments;
            PreferredDateFormat = preferredDateFormat;
            AddressLine1 = addressLine1;
            AddressLine2 = addressLine2;
            AddressLine3 = addressLine3;
            City = city;
            State = state;
            Zip = zip;
            NewData = new DataSet();
            OldData = new DataSet();
        }

        /// <summary>
        /// Saves the project to a JSON file at the specified path.
        /// </summary>
        /// <param name="filePath">The file path to save the project JSON.</param>
        public void SaveToFile(string filePath)
        {
            var persist = ProjectPersist.FromProject(this);
            var json = JsonConvert.SerializeObject(persist, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Loads a project from a JSON file at the specified path.
        /// After deserialization performs manufacturer normalization so downstream label generation has data even if import missed headers.
        /// </summary>
        /// <param name="filePath">The file path to load the project JSON from.</param>
        /// <returns>The deserialized <see cref="Project"/> instance, or null if deserialization fails.</returns>
        public static Project? LoadFromFile(string filePath)
        {
            var json = File.ReadAllText(filePath);
            Project? proj = null;
            try
            {
                var persist = JsonConvert.DeserializeObject<ProjectPersist>(json);
                if (persist != null) proj = persist.ToProject();
            }
            catch { /* fall back */ }
            if (proj == null)
            {
                try { proj = JsonConvert.DeserializeObject<Project>(json); }
                catch { return null; }
            }
            if (proj != null) NormalizeManufacturers(proj);
            return proj;
        }

        /// <summary>
        /// Adds a log entry to the project log with the current UTC timestamp.
        /// </summary>
        /// <param name="message">The log message to add.</param>
        public void AddLog(string message)
        {
            if (ProjectLog == null) ProjectLog = new Dictionary<long, string>();
            ProjectLog[DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()] = message;
        }

        /// <summary>Path or name of the import mapping file for new data.</summary>
        public string? NewDataImportMapFile { get; set; }
        /// <summary>Path or name of the import mapping file for old data.</summary>
        public string? OldDataImportMapFile { get; set; }
        /// <summary>Additional custom properties for the project.</summary>
        public Dictionary<string,string>? Properties { get; set; } = new();

        /// <summary>
        /// Produce a ProjectDiff comparing this project (old) to the provided project (new).
        /// Compares top-level metadata and datasets.
        /// </summary>
        public ProjectDiff Diff(Project? newer)
        {
            var pd = new ProjectDiff();

            if (newer == null)
            {
                // Everything in this project is considered removed
                pd.ProjectPropertyChanges.Add(new PropertyChange { PropertyPath = "Project", OldValue = JsonConvert.SerializeObject(this), NewValue = null, ChangeType = ChangeType.Removed });
                pd.DataDiff = this.NewData?.Diff(null) ?? new DataSetDiff();
                return pd;
            }

            // Compare simple metadata properties
            void CompareProp(string name, string? oldVal, string? newVal)
            {
                if (!string.Equals(oldVal, newVal, StringComparison.Ordinal))
                {
                    pd.ProjectPropertyChanges.Add(new PropertyChange { PropertyPath = name, OldValue = oldVal, NewValue = newVal, ChangeType = ChangeType.Modified });
                }
            }

            CompareProp(nameof(StudyDate), this.StudyDate, newer.StudyDate);
            CompareProp(nameof(PreferredDateFormat), this.PreferredDateFormat, newer.PreferredDateFormat);
            CompareProp(nameof(Revision), this.Revision, newer.Revision);
            CompareProp(nameof(LBProjectNumber), this.LBProjectNumber, newer.LBProjectNumber);
            CompareProp(nameof(Client), this.Client, newer.Client);
            CompareProp(nameof(SiteName), this.SiteName, newer.SiteName);
            CompareProp(nameof(SiteAddress), this.SiteAddress, newer.SiteAddress);
            CompareProp(nameof(AddressLine1), this.AddressLine1, newer.AddressLine1);
            CompareProp(nameof(AddressLine2), this.AddressLine2, newer.AddressLine2);
            CompareProp(nameof(AddressLine3), this.AddressLine3, newer.AddressLine3);
            CompareProp(nameof(City), this.City, newer.City);
            CompareProp(nameof(State), this.State, newer.State);
            CompareProp(nameof(Zip), this.Zip, newer.Zip);
            CompareProp(nameof(StudyEngineer), this.StudyEngineer, newer.StudyEngineer);
            CompareProp(nameof(Comments), this.Comments, newer.Comments);

            // Diff datasets (OldData vs NewData of the newer project)
            pd.DataDiff = (this.NewData ?? new DataSet()).Diff(newer.NewData ?? new DataSet());

            return pd;
        }

        /// <summary>Embedded JSON specification data.</summary>
        public string? EmbeddedSpecJson { get; set; }
        /// <summary>Embedded JSON mapping data.</summary>
        public string? EmbeddedMapJson { get; set; }
        /// <summary>Checksum of the embedded specification data.</summary>
        public string? EmbeddedSpecChecksum { get; set; }
        /// <summary>Checksum of the embedded mapping data.</summary>
        public string? EmbeddedMapChecksum { get; set; }
        /// <summary>Checksum of the last external specification file.</summary>
        public string? LastExternalSpecChecksum { get; set; }
        /// <summary>Checksum of the last external mapping file.</summary>
        public string? LastExternalMapChecksum { get; set; }
        /// <summary>History of specification file paths.</summary>
        public List<string>? SpecPathHistory { get; set; } = new();
        /// <summary>History of mapping file paths.</summary>
        public List<string>? MapPathHistory { get; set; } = new();
        /// <summary>Path to the report template file (.docx/.dotx) used for report generation.</summary>
        public string? TemplatePath { get; set; }
        /// <summary>History of template file paths.</summary>
        public List<string>? TemplatePathHistory { get; set; } = new();

        /// <summary>
        /// Embeds a specification file's content as JSON into the project.
        /// </summary>
        /// <param name="path">The file path of the specification file to embed.</param>
        public void EmbedSpecFromPath(string path)
        {
            if (!File.Exists(path)) return;
            var json = File.ReadAllText(path);
            EmbeddedSpecJson = json;
            EmbeddedSpecChecksum = ComputeSha256(json);
            RememberPath(SpecPathHistory, path);
            LastExternalSpecChecksum = EmbeddedSpecChecksum;
        }

        /// <summary>
        /// Embeds a mapping file's content as JSON into the project.
        /// </summary>
        /// <param name="path">The file path of the mapping file to embed.</param>
        public void EmbedMapFromPath(string path)
        {
            if (!File.Exists(path)) return;
            var json = File.ReadAllText(path);
            EmbeddedMapJson = json;
            EmbeddedMapChecksum = ComputeSha256(json);
            RememberPath(MapPathHistory, path);
            LastExternalMapChecksum = EmbeddedMapChecksum;
        }

        /// <summary>
        /// Extracts the embedded specification JSON to a file.
        /// </summary>
        /// <param name="outputPath">The output file path.</param>
        /// <returns>True if extraction was successful, otherwise false.</returns>
        public bool TryExtractEmbeddedSpec(string outputPath)
        {
            if (string.IsNullOrWhiteSpace(EmbeddedSpecJson)) return false;
            File.WriteAllText(outputPath, EmbeddedSpecJson);
            return true;
        }

        /// <summary>
        /// Extracts the embedded mapping JSON to a file.
        /// </summary>
        /// <param name="outputPath">The output file path.</param>
        /// <returns>True if extraction was successful, otherwise false.</returns>
        public bool TryExtractEmbeddedMap(string outputPath)
        {
            if (string.IsNullOrWhiteSpace(EmbeddedMapJson)) return false;
            File.WriteAllText(outputPath, EmbeddedMapJson);
            return true;
        }

        /// <summary>
        /// Determines if the external specification file has been updated by comparing checksums.
        /// </summary>
        /// <param name="path">The file path of the external specification file.</param>
        /// <returns>True if the external specification file is updated, otherwise false.</returns>
        public bool ExternalSpecUpdated(string path)
        {
            if (!File.Exists(path) || string.IsNullOrWhiteSpace(LastExternalSpecChecksum)) return false;
            var checksum = ComputeSha256(File.ReadAllText(path));
            return !string.Equals(checksum, LastExternalSpecChecksum, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines if the external mapping file has been updated by comparing checksums.
        /// </summary>
        /// <param name="path">The file path of the external mapping file.</param>
        /// <returns>True if the external mapping file is updated, otherwise false.</returns>
        public bool ExternalMapUpdated(string path)
        {
            if (!File.Exists(path) || string.IsNullOrWhiteSpace(LastExternalMapChecksum)) return false;
            var checksum = ComputeSha256(File.ReadAllText(path));
            return !string.Equals(checksum, LastExternalMapChecksum, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Compares provided external spec/map paths against embedded checksums and optionally re-embeds.
        /// Returns a tuple of (changed, updated, messages).
        /// </summary>
        public (bool changed, bool updated, List<string> messages) CompareExternalSpecMap(string? specPath, string? mapPath, bool autoSync)
        {
            var messages = new List<string>();
            bool changed = false; bool updated = false;
            string ComputeSha(string p) { using var sha=SHA256.Create(); return Convert.ToHexString(sha.ComputeHash(File.ReadAllBytes(p))); }
            if (!string.IsNullOrWhiteSpace(specPath) && File.Exists(specPath))
            {
                var ext = ComputeSha(specPath);
                var embedded = LastExternalSpecChecksum ?? EmbeddedSpecChecksum;
                if (embedded == null || !ext.Equals(embedded, StringComparison.OrdinalIgnoreCase))
                {
                    changed = true; messages.Add($"Spec differs (embedded={embedded ?? "<none>"} external={ext})");
                    if (autoSync)
                    {
                        EmbedSpecFromPath(specPath); updated = true; messages.Add("Spec auto-synced");
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(mapPath) && File.Exists(mapPath))
            {
                var ext = ComputeSha(mapPath);
                var embedded = LastExternalMapChecksum ?? EmbeddedMapChecksum;
                if (embedded == null || !ext.Equals(embedded, StringComparison.OrdinalIgnoreCase))
                {
                    changed = true; messages.Add($"Map differs (embedded={embedded ?? "<none>"} external={ext})");
                    if (autoSync)
                    {
                        EmbedMapFromPath(mapPath); updated = true; messages.Add("Map auto-synced");
                    }
                }
            }
            return (changed, updated, messages);
        }

        private static void RememberPath(List<string>? list, string path)
        {
            if (list == null) return;
            var full = Path.GetFullPath(path);
            list.RemoveAll(p => string.Equals(Path.GetFullPath(p), full, StringComparison.OrdinalIgnoreCase));
            list.Insert(0, full);
            if (list.Count > 10) list.RemoveRange(10, list.Count - 10);
        }
        private static string ComputeSha256(string text)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
            return Convert.ToHexString(hash);
        }

        // Known manufacturer name tokens for heuristic inference when explicit Manufacturer column missing.
        private static readonly string[] KnownManufacturerPrefixes = new[]
        {
            "ABB","EATON","SIEMENS","SCHNEIDER","SQUARE D","SQ D","GE","GENERAL ELECTRIC","ALLEN-BRADLEY","ALLEN BRADLEY","CUTLER-HAMMER","CUTLER HAMMER","MITSUBISHI","HITACHI","FUJI","KLOCKNER","TOSHIBA"
        };

        /// <summary>
        /// Populate missing Manufacturer fields (breaker) using fallbacks:
        /// 1. If LVBreaker.BreakerMfr empty but TripMfr present → copy.
        /// 2. If TripMfr empty but LVBreaker.BreakerMfr present → copy.
        /// 3. If still empty attempt heuristic from BreakerStyle (leading tokens matched to known manufacturers).
        /// </summary>
        private static void NormalizeManufacturers(Project proj)
        {
            var dsList = new[] { proj.NewData, proj.OldData };
            foreach (var ds in dsList)
            {
                if (ds?.LVBreakerEntries == null) continue;
                foreach (var kv in ds.LVBreakerEntries)
                {
                    var b = kv.Value;
                    if (b == null) continue;
                    
                    // Step 1 / 2 cross-copy between BreakerMfr and TripMfr
                    if (string.IsNullOrWhiteSpace(b.BreakerMfr) && !string.IsNullOrWhiteSpace(b.TripMfr))
                        b.BreakerMfr = b.TripMfr!.Trim();
                    else if (string.IsNullOrWhiteSpace(b.TripMfr) && !string.IsNullOrWhiteSpace(b.BreakerMfr))
                        b.TripMfr = b.BreakerMfr!.Trim();
                    
                    // Step 3 heuristics on breaker style
                    if (string.IsNullOrWhiteSpace(b.BreakerMfr))
                    {
                        var inferred = InferManufacturerFromStyle(b.BreakerStyle);
                        if (!string.IsNullOrEmpty(inferred)) b.BreakerMfr = inferred;
                    }
                    
                    if (string.IsNullOrWhiteSpace(b.TripMfr))
                    {
                        var inferredTu = InferManufacturerFromStyle(b.TripStyle) ?? b.BreakerMfr;
                        if (!string.IsNullOrWhiteSpace(inferredTu)) b.TripMfr = inferredTu;
                    }
                }
            }
        }

        private static string? InferManufacturerFromStyle(string? style)
        {
            if (string.IsNullOrWhiteSpace(style)) return null;
            var s = style.Trim();
            // Normalize non-breaking space and collapse whitespace
            s = Regex.Replace(s, "\u00A0", " ");
            s = Regex.Replace(s, @"\s+", " "); // use verbatim string literal for pattern
            var upper = s.ToUpperInvariant();
            foreach (var pref in KnownManufacturerPrefixes.OrderByDescending(p=>p.Length))
            {
                if (upper.StartsWith(pref + " ") || string.Equals(upper, pref, StringComparison.OrdinalIgnoreCase))
                    return CultureInfoInvariantTitle(pref);
            }
            return null;
        }

        private static string CultureInfoInvariantTitle(string text)
        {
            // Title-case basic tokens; keep common uppercase like GE
            if (string.Equals(text, "GE", StringComparison.OrdinalIgnoreCase)) return "GE";
            var parts = text.Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries);
            for (int i=0;i<parts.Length;i++)
            {
                var p = parts[i].ToLowerInvariant();
                if (p.Length==1) { parts[i] = p.ToUpperInvariant(); continue; }
                parts[i] = char.ToUpperInvariant(p[0]) + p.Substring(1);
            }
            return string.Join(" ", parts);
        }
    }

    internal class ProjectPersist
    {
        public string? StudyDate { get; set; }
        public string? Revision { get; set; }
        public string? LBProjectNumber { get; set; }
        public string? Client { get; set; }
        public string? SiteName { get; set; }
        public string? SiteAddress { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Zip { get; set; }
        public string? StudyEngineer { get; set; }
        public string? Comments { get; set; }
        public string? PreferredDateFormat { get; set; }
        public string? NewDataImportMapFile { get; set; }
        public string? OldDataImportMapFile { get; set; }
        public Dictionary<long,string>? ProjectLog { get; set; }
        public DataSetPersist? NewData { get; set; }
        public DataSetPersist? OldData { get; set; }
        public Dictionary<string,string>? Properties { get; set; } = new();
        public string? EmbeddedSpecJson { get; set; }
        public string? EmbeddedMapJson { get; set; }
        public string? EmbeddedSpecChecksum { get; set; }
        public string? EmbeddedMapChecksum { get; set; }
        public string? LastExternalSpecChecksum { get; set; }
        public string? LastExternalMapChecksum { get; set; }
        public List<string>? SpecPathHistory { get; set; }
        public List<string>? MapPathHistory { get; set; }
        public string? TemplatePath { get; set; }
        public List<string>? TemplatePathHistory { get; set; }

        public static ProjectPersist FromProject(Project p)
        {
            return new ProjectPersist
            {
                StudyDate = p.StudyDate,
                Revision = p.Revision,
                LBProjectNumber = p.LBProjectNumber,
                Client = p.Client,
                SiteName = p.SiteName,
                SiteAddress = p.SiteAddress,
                AddressLine1 = p.AddressLine1,
                AddressLine2 = p.AddressLine2,
                AddressLine3 = p.AddressLine3,
                City = p.City,
                State = p.State,
                Zip = p.Zip,
                StudyEngineer = p.StudyEngineer,
                Comments = p.Comments,
                PreferredDateFormat = p.PreferredDateFormat,
                NewDataImportMapFile = p.NewDataImportMapFile,
                OldDataImportMapFile = p.OldDataImportMapFile,
                ProjectLog = p.ProjectLog,
                NewData = DataSetPersist.FromDataSet(p.NewData),
                OldData = DataSetPersist.FromDataSet(p.OldData),
                Properties = p.Properties,
                EmbeddedSpecJson = p.EmbeddedSpecJson,
                EmbeddedMapJson = p.EmbeddedMapJson,
                EmbeddedSpecChecksum = p.EmbeddedSpecChecksum,
                EmbeddedMapChecksum = p.EmbeddedMapChecksum,
                LastExternalSpecChecksum = p.LastExternalSpecChecksum,
                LastExternalMapChecksum = p.LastExternalMapChecksum,
                SpecPathHistory = p.SpecPathHistory,
                MapPathHistory = p.MapPathHistory,
                TemplatePath = p.TemplatePath,
                TemplatePathHistory = p.TemplatePathHistory
            };
        }
        public Project ToProject()
        {
            var proj = new Project(StudyDate, Revision, LBProjectNumber, Client, SiteName, SiteAddress, StudyEngineer, Comments, PreferredDateFormat,
                                   AddressLine1, AddressLine2, AddressLine3, City, State, Zip)
            {
                NewDataImportMapFile = NewDataImportMapFile,
                OldDataImportMapFile = OldDataImportMapFile,
                ProjectLog = ProjectLog,
                NewData = NewData?.ToDataSet(),
                OldData = OldData?.ToDataSet(),
                Properties = Properties ?? new Dictionary<string,string>(),
                EmbeddedSpecJson = EmbeddedSpecJson,
                EmbeddedMapJson = EmbeddedMapJson,
                EmbeddedSpecChecksum = EmbeddedSpecChecksum,
                EmbeddedMapChecksum = EmbeddedMapChecksum,
                LastExternalSpecChecksum = LastExternalSpecChecksum,
                LastExternalMapChecksum = LastExternalMapChecksum,
                SpecPathHistory = SpecPathHistory ?? new List<string>(),
                MapPathHistory = MapPathHistory ?? new List<string>(),
                TemplatePath = TemplatePath,
                TemplatePathHistory = TemplatePathHistory ?? new List<string>()
            };

            // If legacy SiteAddress is provided but normalized fields are empty, attempt a simple split heuristic
            if (!string.IsNullOrWhiteSpace(SiteAddress) && string.IsNullOrWhiteSpace(AddressLine1))
            {
                var lines = SiteAddress.Replace("\r", string.Empty).Split('\n');
                if (lines.Length > 1)
                {
                    proj.AddressLine1 = lines.ElementAtOrDefault(0);
                    proj.AddressLine2 = lines.ElementAtOrDefault(1);
                    proj.AddressLine3 = lines.ElementAtOrDefault(2);
                }
                else
                {
                    // Fallback: split by comma
                    var parts = SiteAddress.Split(',');
                    proj.AddressLine1 = parts.ElementAtOrDefault(0)?.Trim();
                    if (parts.Length >= 3)
                    {
                        proj.City = parts.ElementAtOrDefault(1)?.Trim();
                        var stateZip = parts.ElementAtOrDefault(2)?.Trim();
                        if (!string.IsNullOrWhiteSpace(stateZip))
                        {
                            var sz = Regex.Split(stateZip, "\\s+");
                            proj.State = sz.ElementAtOrDefault(0);
                            proj.Zip = sz.ElementAtOrDefault(1);
                        }
                    }
                }
            }
            return proj;
        }
    }

    internal class DataSetPersist
    {
        public string? SoftwareVersion { get; set; }
        public List<ArcFlashEntry>? ArcFlashEntries { get; set; }
        public List<ShortCircuitEntry>? ShortCircuitEntries { get; set; }
        
        // Original 6 equipment types
        public Dictionary<string,LVBreaker>? LVBreakerEntries { get; set; }
        public Dictionary<string,Fuse>? FuseEntries { get; set; }
        public Dictionary<string,Cable>? CableEntries { get; set; }
        public Dictionary<string,Bus>? BusEntries { get; set; }
        
        // Extended equipment types (alphabetically)
        public Dictionary<string,AFD>? AFDEntries { get; set; }
        public Dictionary<string,ATS>? ATSEntries { get; set; }
        public Dictionary<string,Battery>? BatteryEntries { get; set; }
        public Dictionary<string,Busway>? BuswayEntries { get; set; }
        public Dictionary<string,Capacitor>? CapacitorEntries { get; set; }
        public Dictionary<string,CLReactor>? CLReactorEntries { get; set; }
        public Dictionary<string,CT>? CTEntries { get; set; }
        public Dictionary<string,Filter>? FilterEntries { get; set; }
        public Dictionary<string,Generator>? GeneratorEntries { get; set; }
        public Dictionary<string,HVBreaker>? HVBreakerEntries { get; set; }
        public Dictionary<string,Inverter>? InverterEntries { get; set; }
        public Dictionary<string,Load>? LoadEntries { get; set; }
        public Dictionary<string,MCC>? MCCEntries { get; set; }
        public Dictionary<string,Meter>? MeterEntries { get; set; }
        public Dictionary<string,Motor>? MotorEntries { get; set; }
        public Dictionary<string,Panel>? PanelEntries { get; set; }
        public Dictionary<string,Photovoltaic>? PhotovoltaicEntries { get; set; }
        public Dictionary<string,POC>? POCEntries { get; set; }
        public Dictionary<string,Rectifier>? RectifierEntries { get; set; }
        public Dictionary<string,Relay>? RelayEntries { get; set; }
        public Dictionary<string,Shunt>? ShuntEntries { get; set; }
        public Dictionary<string,Switch>? SwitchEntries { get; set; }
        public Dictionary<string,Transformer2W>? Transformer2WEntries { get; set; }
        public Dictionary<string,Transformer3W>? Transformer3WEntries { get; set; }
        public Dictionary<string,TransmissionLine>? TransmissionLineEntries { get; set; }
        public Dictionary<string,UPS>? UPSEntries { get; set; }
        public Dictionary<string,Utility>? UtilityEntries { get; set; }
        public Dictionary<string,ZigzagTransformer>? ZigzagTransformerEntries { get; set; }

        public static DataSetPersist? FromDataSet(DataSet? ds)
        {
            if (ds == null) return null;
            return new DataSetPersist
            {
                SoftwareVersion = ds.SoftwareVersion,
                ArcFlashEntries = ds.ArcFlashEntries?.Select(kv => new ArcFlashEntry { Id = kv.Key.Id, Scenario = kv.Key.Scenario, Value = kv.Value }).ToList(),
                ShortCircuitEntries = ds.ShortCircuitEntries?.Select(kv => new ShortCircuitEntry { Id = kv.Key.Id, Bus = kv.Key.Bus, Scenario = kv.Key.Scenario, Value = kv.Value }).ToList(),
                
                // Original 6
                LVBreakerEntries = ds.LVBreakerEntries,
                FuseEntries = ds.FuseEntries,
                CableEntries = ds.CableEntries,
                BusEntries = ds.BusEntries,
                
                // Extended (alphabetically)
                AFDEntries = ds.AFDEntries,
                ATSEntries = ds.ATSEntries,
                BatteryEntries = ds.BatteryEntries,
                BuswayEntries = ds.BuswayEntries,
                CapacitorEntries = ds.CapacitorEntries,
                CLReactorEntries = ds.CLReactorEntries,
                CTEntries = ds.CTEntries,
                FilterEntries = ds.FilterEntries,
                GeneratorEntries = ds.GeneratorEntries,
                HVBreakerEntries = ds.HVBreakerEntries,
                InverterEntries = ds.InverterEntries,
                LoadEntries = ds.LoadEntries,
                MCCEntries = ds.MCCEntries,
                MeterEntries = ds.MeterEntries,
                MotorEntries = ds.MotorEntries,
                PanelEntries = ds.PanelEntries,
                PhotovoltaicEntries = ds.PhotovoltaicEntries,
                POCEntries = ds.POCEntries,
                RectifierEntries = ds.RectifierEntries,
                RelayEntries = ds.RelayEntries,
                ShuntEntries = ds.ShuntEntries,
                SwitchEntries = ds.SwitchEntries,
                Transformer2WEntries = ds.Transformer2WEntries,
                Transformer3WEntries = ds.Transformer3WEntries,
                TransmissionLineEntries = ds.TransmissionLineEntries,
                UPSEntries = ds.UPSEntries,
                UtilityEntries = ds.UtilityEntries,
                ZigzagTransformerEntries = ds.ZigzagTransformerEntries
            };
        }
        
        public DataSet ToDataSet()
        {
            var ds = new DataSet { SoftwareVersion = SoftwareVersion };
            
            // Composite key entries
            if (ArcFlashEntries != null)
                ds.ArcFlashEntries = ArcFlashEntries.ToDictionary(e => (e.Id, e.Scenario), e => e.Value!);
            if (ShortCircuitEntries != null)
                ds.ShortCircuitEntries = ShortCircuitEntries.ToDictionary(e => (e.Id, e.Bus, e.Scenario), e => e.Value!);
            
            // All equipment dictionaries (alphabetically)
            ds.AFDEntries = AFDEntries ?? new Dictionary<string, AFD>();
            ds.ATSEntries = ATSEntries ?? new Dictionary<string, ATS>();
            ds.BatteryEntries = BatteryEntries ?? new Dictionary<string, Battery>();
            ds.BusEntries = BusEntries ?? new Dictionary<string, Bus>();
            ds.BuswayEntries = BuswayEntries ?? new Dictionary<string, Busway>();
            ds.CableEntries = CableEntries ?? new Dictionary<string, Cable>();
            ds.CapacitorEntries = CapacitorEntries ?? new Dictionary<string, Capacitor>();
            ds.CLReactorEntries = CLReactorEntries ?? new Dictionary<string, CLReactor>();
            ds.CTEntries = CTEntries ?? new Dictionary<string, CT>();
            ds.FilterEntries = FilterEntries ?? new Dictionary<string, Filter>();
            ds.FuseEntries = FuseEntries ?? new Dictionary<string, Fuse>();
            ds.GeneratorEntries = GeneratorEntries ?? new Dictionary<string, Generator>();
            ds.HVBreakerEntries = HVBreakerEntries ?? new Dictionary<string, HVBreaker>();
            ds.InverterEntries = InverterEntries ?? new Dictionary<string, Inverter>();
            ds.LoadEntries = LoadEntries ?? new Dictionary<string, Load>();
            ds.LVBreakerEntries = LVBreakerEntries ?? new Dictionary<string, LVBreaker>();
            ds.MCCEntries = MCCEntries ?? new Dictionary<string, MCC>();
            ds.MeterEntries = MeterEntries ?? new Dictionary<string, Meter>();
            ds.MotorEntries = MotorEntries ?? new Dictionary<string, Motor>();
            ds.PanelEntries = PanelEntries ?? new Dictionary<string, Panel>();
            ds.PhotovoltaicEntries = PhotovoltaicEntries ?? new Dictionary<string, Photovoltaic>();
            ds.POCEntries = POCEntries ?? new Dictionary<string, POC>();
            ds.RectifierEntries = RectifierEntries ?? new Dictionary<string, Rectifier>();
            ds.RelayEntries = RelayEntries ?? new Dictionary<string, Relay>();
            ds.ShuntEntries = ShuntEntries ?? new Dictionary<string, Shunt>();
            ds.SwitchEntries = SwitchEntries ?? new Dictionary<string, Switch>();
            ds.Transformer2WEntries = Transformer2WEntries ?? new Dictionary<string, Transformer2W>();
            ds.Transformer3WEntries = Transformer3WEntries ?? new Dictionary<string, Transformer3W>();
            ds.TransmissionLineEntries = TransmissionLineEntries ?? new Dictionary<string, TransmissionLine>();
            ds.UPSEntries = UPSEntries ?? new Dictionary<string, UPS>();
            ds.UtilityEntries = UtilityEntries ?? new Dictionary<string, Utility>();
            ds.ZigzagTransformerEntries = ZigzagTransformerEntries ?? new Dictionary<string, ZigzagTransformer>();
            
            return ds;
        }
    }
    internal class ArcFlashEntry { public string Id { get; set; } = string.Empty; public string Scenario { get; set; } = string.Empty; public ArcFlash? Value { get; set; } }
    internal class ShortCircuitEntry { public string Id { get; set; } = string.Empty; public string Bus { get; set; } = string.Empty; public string Scenario { get; set; } = string.Empty; public ShortCircuit? Value { get; set; } }
}


