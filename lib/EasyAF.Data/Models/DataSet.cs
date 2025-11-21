using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyAF.Data.Models
{
    /// <summary>
    /// Encapsulates all device and study entry dictionaries for a complete electrical project dataset.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Purpose:</strong> DataSet is the central container for all imported electrical equipment
    /// and study data in an EasyAF project. It maintains separate dictionaries for each data type,
    /// keyed appropriately to support efficient lookup and comparison operations.
    /// </para>
    /// <para>
    /// <strong>Data Types Contained:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Bus</strong> - Electrical buses/switchgear (keyed by Id)</description></item>
    /// <item><description><strong>LVCB</strong> - Low voltage circuit breakers with trip unit settings flattened onto LVCB (keyed by Id)</description></item>
    /// <item><description><strong>Fuse</strong> - Fuse protection devices (keyed by Id)</description></item>
    /// <item><description><strong>Cable</strong> - Cable/conductor data (keyed by Id)</description></item>
    /// <item><description><strong>ArcFlash</strong> - Arc flash study results (keyed by (Id, Scenario))</description></item>
    /// <item><description><strong>ShortCircuit</strong> - Short circuit study results (keyed by (Id, Bus, Scenario))</description></item>
    /// <item><description><strong>Transformer</strong> - Transformer data (keyed by Id)</description></item>
    /// <item><description><strong>Motor</strong> - Motor data (keyed by Id)</description></item>
    /// <item><description><strong>Generator</strong> - Generator data (keyed by Id)</description></item>
    /// <item><description><strong>Utility</strong> - Utility connection data (keyed by Id)</description></item>
    /// <item><description><strong>Capacitor</strong> - Capacitor bank data (keyed by Id)</description></item>
    /// <item><description><strong>Load</strong> - Electrical load data (keyed by Id)</description></item>
    /// </list>
    /// <para>
    /// <strong>Dictionary Key Structures:</strong>
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>Entry Type</term>
    /// <description>Key Structure</description>
    /// </listheader>
    /// <item>
    /// <term>Bus, LVCB, Fuse, Cable</term>
    /// <description>Single string Id (e.g., "BUS-001", "CB-123")</description>
    /// </item>
    /// <item>
    /// <term>ArcFlash</term>
    /// <description>Composite (Id, Scenario) tuple (e.g., ("BUS-001", "Main-Min"))</description>
    /// </item>
    /// <item>
    /// <term>ShortCircuit</term>
    /// <description>Composite (Id, Bus, Scenario) tuple (e.g., ("BUS-001", "MAIN", "Main-Max"))</description>
    /// </item>
    /// </list>
    /// <para>
    /// <strong>Diff/Comparison Support:</strong> The <see cref="Diff"/> method enables comparison
    /// between two DataSets, producing a detailed <see cref="DataSetDiff"/> that identifies:
    /// - Added entries (new in the compared dataset)
    /// - Removed entries (missing from the compared dataset)
    /// - Modified entries (changed property values)
    /// </para>
    /// <para>
    /// <strong>Thread Safety:</strong> DataSet is NOT thread-safe. Synchronize access if used concurrently.
    /// </para>
    /// <para>
    /// <strong>Null Handling:</strong> All dictionary properties are nullable but initialized to empty
    /// dictionaries by default. This allows for optional exclusion of entire data types if not needed.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para><strong>Example 1: Creating and populating a DataSet</strong></para>
    /// <code>
    /// var dataSet = new DataSet
    /// {
    ///     SoftwareVersion = "3.0.0"
    /// };
    /// 
    /// // Add some buses
    /// dataSet.BusEntries["BUS-001"] = new Bus
    /// {
    ///     Id = "BUS-001",
    ///     Name = "Main Switchgear",
    ///     Voltage = "480V"
    /// };
    /// 
    /// // Add arc flash study results
    /// dataSet.ArcFlashEntries[("BUS-001", "Main-Min")] = new ArcFlash
    /// {
    ///     Bus = "BUS-001",
    ///     Scenario = "Main-Min",
    ///     IncidentEnergy = "8.5"
    /// };
    /// 
    /// Console.WriteLine($"Dataset contains {dataSet.BusEntries.Count} buses");
    /// </code>
    /// 
    /// <para><strong>Example 2: Comparing two datasets</strong></para>
    /// <code>
    /// var oldDataSet = LoadFromFile("project-v1.json");
    /// var newDataSet = LoadFromFile("project-v2.json");
    /// 
    /// // Generate diff (old -> new)
    /// var diff = oldDataSet.Diff(newDataSet);
    /// 
    /// Console.WriteLine($"Added: {diff.EntryDiffs.Count(d => d.ChangeType == ChangeType.Added)}");
    /// Console.WriteLine($"Removed: {diff.EntryDiffs.Count(d => d.ChangeType == ChangeType.Removed)}");
    /// Console.WriteLine($"Modified: {diff.EntryDiffs.Count(d => d.ChangeType == ChangeType.Modified)}");
    /// 
    /// // Show details of modified entries
    /// foreach (var entry in diff.EntryDiffs.Where(d => d.ChangeType == ChangeType.Modified))
    /// {
    ///     Console.WriteLine($"{entry.EntryType} {entry.EntryKey}:");
    ///     foreach (var change in entry.PropertyChanges)
    ///     {
    ///         Console.WriteLine($"  {change.PropertyPath}: {change.OldValue} -> {change.NewValue}");
    ///     }
    /// }
    /// </code>
    /// 
    /// <para><strong>Example 3: Accessing composite-keyed entries</strong></para>
    /// <code>
    /// var dataSet = new DataSet();
    /// 
    /// // ArcFlash entries use composite key (Id, Scenario)
    /// var key = ("BUS-001", "Main-Min");
    /// if (dataSet.ArcFlashEntries.TryGetValue(key, out var arcFlash))
    /// {
    ///     Console.WriteLine($"Incident Energy: {arcFlash.IncidentEnergy}");
    /// }
    /// 
    /// // ShortCircuit entries use triple composite key (Id, Bus, Scenario)
    /// var scKey = ("DEVICE-001", "BUS-001", "Main-Max");
    /// if (dataSet.ShortCircuitEntries.TryGetValue(scKey, out var shortCircuit))
    /// {
    ///     Console.WriteLine($"Duty: {shortCircuit.DutyKA}");
    /// }
    /// </code>
    /// </example>
    public class DataSet
    {
        /// <summary>
        /// Gets or sets the software version for which this dataset was imported or created.
        /// </summary>
        /// <remarks>
        /// Used to track compatibility and data format versioning. Typically set during import
        /// from the mapping configuration's SoftwareVersion property.
        /// </remarks>
        public string? SoftwareVersion { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of arc flash study results, keyed by CompositeKey.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>Key Structure:</strong> CompositeKey with 2 components (discovered from [Required] attributes)
        /// </para>
        /// <list type="bullet">
        /// <item><description><strong>Component 0:</strong> ArcFaultBusName - The bus or location identifier where the arc flash study was performed</description></item>
        /// <item><description><strong>Component 1:</strong> Scenario - The fault scenario name (e.g., "Main-Min", "Main-Max", "Service-Min")</description></item>
        /// </list>
        /// <para>
        /// Multiple scenarios can exist for the same bus (different operating conditions, configurations, etc.).
        /// </para>
        /// <para>
        /// <strong>Example Access:</strong>
        /// <code>
        /// var key = new CompositeKey("BUS-001", "Main-Min");
        /// if (dataSet.ArcFlashEntries.TryGetValue(key, out var entry))
        /// {
        ///     Console.WriteLine($"Incident Energy: {entry.IncidentEnergy} cal/cm²");
        /// }
        /// </code>
        /// </para>
        /// </remarks>
        public Dictionary<CompositeKey, ArcFlash>? ArcFlashEntries { get; set; } = new Dictionary<CompositeKey, ArcFlash>();

        /// <summary>
        /// Gets or sets the dictionary of short circuit study results, keyed by CompositeKey.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>Key Structure:</strong> CompositeKey with 3 components (discovered from [Required] attributes)
        /// </para>
        /// <list type="bullet">
        /// <item><description><strong>Component 0:</strong> BusName - The bus location identifier</description></item>
        /// <item><description><strong>Component 1:</strong> EquipmentName - The device or equipment identifier</description></item>
        /// <item><description><strong>Component 2:</strong> Scenario - The fault scenario name (e.g., "Main-Max", "Service-Max")</description></item>
        /// </list>
        /// <para>
        /// The 3-component key allows the same equipment to have study results across multiple scenarios.
        /// </para>
        /// <para>
        /// <strong>Example Access:</strong>
        /// <code>
        /// var key = new CompositeKey("BUS-001", "CB-123", "Main-Max");
        /// if (dataSet.ShortCircuitEntries.TryGetValue(key, out var entry))
        /// {
        ///     Console.WriteLine($"Available Fault Current: {entry.OneTwoCycleDuty} kA");
        /// }
        /// </code>
        /// </para>
        /// </remarks>
        public Dictionary<CompositeKey, ShortCircuit>? ShortCircuitEntries { get; set; } = new Dictionary<CompositeKey, ShortCircuit>();

        /// <summary>
        /// Gets or sets the dictionary of low voltage circuit breaker entries, keyed by CompositeKey.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>Key Structure:</strong> CompositeKey with 1 component (LVBreakerName)
        /// </para>
        /// <para>
        /// Trip unit settings are flattened onto the LVBreaker model as TripUnit* properties.
        /// </para>
        /// </remarks>
        public Dictionary<CompositeKey, LVBreaker>? LVBreakerEntries { get; set; } = new Dictionary<CompositeKey, LVBreaker>();

        /// <summary>
        /// Gets or sets the dictionary of fuse entries, keyed by CompositeKey.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>Key Structure:</strong> CompositeKey with 1 component (FuseName)
        /// </para>
        /// <para>
        /// Fuses are protective devices with simpler structure than circuit breakers.
        /// Common properties include Manufacturer, Style, Rating, and Voltage.
        /// </para>
        /// </remarks>
        public Dictionary<CompositeKey, Fuse>? FuseEntries { get; set; } = new Dictionary<CompositeKey, Fuse>();

        /// <summary>
        /// Gets or sets the dictionary of cable/conductor entries, keyed by CompositeKey.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>Key Structure:</strong> CompositeKey with 1 component (CableName)
        /// </para>
        /// <para>
        /// Cables represent the conductors connecting electrical equipment.
        /// Properties typically include Size, Insulation type, Ampacity, and Length.
        /// </para>
        /// </remarks>
        public Dictionary<CompositeKey, Cable>? CableEntries { get; set; } = new Dictionary<CompositeKey, Cable>();

        /// <summary>
        /// Gets or sets the dictionary of bus/switchgear entries, keyed by CompositeKey.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>Key Structure:</strong> CompositeKey with 1 component (Buss/Id)
        /// </para>
        /// <para>
        /// Buses are the primary nodes in the electrical system where equipment connects.
        /// Properties include Name, Voltage, Phase configuration, and Grounding.
        /// </para>
        /// </remarks>
        public Dictionary<CompositeKey, Bus>? BusEntries { get; set; } = new Dictionary<CompositeKey, Bus>();

        /// <summary>
        /// Gets or sets the dictionary of 2-winding transformer entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, Transformer2W>? Transformer2WEntries { get; set; } = new Dictionary<CompositeKey, Transformer2W>();

        /// <summary>
        /// Gets or sets the dictionary of motor entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, Motor>? MotorEntries { get; set; } = new Dictionary<CompositeKey, Motor>();

        /// <summary>
        /// Gets or sets the dictionary of generator entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, Generator>? GeneratorEntries { get; set; } = new Dictionary<CompositeKey, Generator>();

        /// <summary>
        /// Gets or sets the dictionary of utility connection entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, Utility>? UtilityEntries { get; set; } = new Dictionary<CompositeKey, Utility>();

        /// <summary>
        /// Gets or sets the dictionary of capacitor entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, Capacitor>? CapacitorEntries { get; set; } = new Dictionary<CompositeKey, Capacitor>();

        /// <summary>
        /// Gets or sets the dictionary of load entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, Load>? LoadEntries { get; set; } = new Dictionary<CompositeKey, Load>();

        // === Additional Equipment Types (Alphabetically) ===

        /// <summary>
        /// Gets or sets the dictionary of adjustable frequency drive entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, AFD>? AFDEntries { get; set; } = new Dictionary<CompositeKey, AFD>();

        /// <summary>
        /// Gets or sets the dictionary of automatic transfer switch entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, ATS>? ATSEntries { get; set; } = new Dictionary<CompositeKey, ATS>();

        /// <summary>
        /// Gets or sets the dictionary of battery entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, Battery>? BatteryEntries { get; set; } = new Dictionary<CompositeKey, Battery>();

        /// <summary>
        /// Gets or sets the dictionary of busway entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, Busway>? BuswayEntries { get; set; } = new Dictionary<CompositeKey, Busway>();

        /// <summary>
        /// Gets or sets the dictionary of current limiting reactor entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, CLReactor>? CLReactorEntries { get; set; } = new Dictionary<CompositeKey, CLReactor>();

        /// <summary>
        /// Gets or sets the dictionary of current transformer entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, CT>? CTEntries { get; set; } = new Dictionary<CompositeKey, CT>();

        /// <summary>
        /// Gets or sets the dictionary of filter entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, Filter>? FilterEntries { get; set; } = new Dictionary<CompositeKey, Filter>();

        /// <summary>
        /// Gets or sets the dictionary of high voltage breaker entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, HVBreaker>? HVBreakerEntries { get; set; } = new Dictionary<CompositeKey, HVBreaker>();

        /// <summary>
        /// Gets or sets the dictionary of inverter entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, Inverter>? InverterEntries { get; set; } = new Dictionary<CompositeKey, Inverter>();

        /// <summary>
        /// Gets or sets the dictionary of motor control center entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, MCC>? MCCEntries { get; set; } = new Dictionary<CompositeKey, MCC>();

        /// <summary>
        /// Gets or sets the dictionary of meter entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, Meter>? MeterEntries { get; set; } = new Dictionary<CompositeKey, Meter>();

        /// <summary>
        /// Gets or sets the dictionary of panel entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, Panel>? PanelEntries { get; set; } = new Dictionary<CompositeKey, Panel>();

        /// <summary>
        /// Gets or sets the dictionary of photovoltaic entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, Photovoltaic>? PhotovoltaicEntries { get; set; } = new Dictionary<CompositeKey, Photovoltaic>();

        /// <summary>
        /// Gets or sets the dictionary of point of common coupling entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, POC>? POCEntries { get; set; } = new Dictionary<CompositeKey, POC>();

        /// <summary>
        /// Gets or sets the dictionary of rectifier entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, Rectifier>? RectifierEntries { get; set; } = new Dictionary<CompositeKey, Rectifier>();

        /// <summary>
        /// Gets or sets the dictionary of relay entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, Relay>? RelayEntries { get; set; } = new Dictionary<CompositeKey, Relay>();

        /// <summary>
        /// Gets or sets the dictionary of shunt entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, Shunt>? ShuntEntries { get; set; } = new Dictionary<CompositeKey, Shunt>();

        /// <summary>
        /// Gets or sets the dictionary of switch entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, Switch>? SwitchEntries { get; set; } = new Dictionary<CompositeKey, Switch>();

        /// <summary>
        /// Gets or sets the dictionary of 3-winding transformer entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, Transformer3W>? Transformer3WEntries { get; set; } = new Dictionary<CompositeKey, Transformer3W>();

        /// <summary>
        /// Gets or sets the dictionary of transmission line entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, TransmissionLine>? TransmissionLineEntries { get; set; } = new Dictionary<CompositeKey, TransmissionLine>();

        /// <summary>
        /// Gets or sets the dictionary of UPS entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, UPS>? UPSEntries { get; set; } = new Dictionary<CompositeKey, UPS>();

        /// <summary>
        /// Gets or sets the dictionary of zigzag transformer entries, keyed by CompositeKey.
        /// </summary>
        public Dictionary<CompositeKey, ZigzagTransformer>? ZigzagTransformerEntries { get; set; } = new Dictionary<CompositeKey, ZigzagTransformer>();

        /// <summary>
        /// Produces a detailed comparison between this DataSet (old) and another DataSet (new).
        /// </summary>
        /// <param name="newer">
        /// The DataSet to compare against. If null, all entries in this dataset are treated as removed.
        /// </param>
        /// <returns>
        /// A <see cref="DataSetDiff"/> containing all detected changes (additions, removals, modifications).
        /// </returns>
        /// <remarks>
        /// <para>
        /// <strong>Comparison Algorithm:</strong>
        /// For each entry type (Bus, LVCB, Fuse, Cable, ArcFlash, ShortCircuit):
        /// </para>
        /// <list type="number">
        /// <item><description>Collect all unique keys from both old and new datasets</description></item>
        /// <item><description>For each key, determine if the entry was added, removed, or modified</description></item>
        /// <item><description>For modified entries, use <see cref="DiffUtil"/> to identify changed properties</description></item>
        /// </list>
        /// <para>
        /// <strong>Performance Considerations:</strong>
        /// - For large datasets (1000+ entries), diff operations may take several hundred milliseconds
        /// - Consider running diff operations on a background thread in UI applications
        /// - Property comparisons use reflection; results are not cached
        /// </para>
        /// <para>
        /// <strong>Change Detection:</strong>
        /// - <see cref="ChangeType.Added"/>: Entry exists in new dataset but not old
        /// - <see cref="ChangeType.Removed"/>: Entry exists in old dataset but not new
        /// - <see cref="ChangeType.Modified"/>: Entry exists in both with different property values
        /// </para>
        /// <para>
        /// <strong>Thread Safety:</strong> This method is thread-safe for read-only operations.
        /// Neither the source nor target DataSet should be modified during comparison.
        /// </para>
        /// </remarks>
        /// <example>
        /// <para><strong>Example: Generating and analyzing a diff</strong></para>
        /// <code>
        /// var oldData = LoadDataSet("project-2024-01.json");
        /// var newData = LoadDataSet("project-2024-02.json");
        /// 
        /// // Generate diff
        /// var diff = oldData.Diff(newData);
        /// 
        /// // Summary statistics
        /// var addedCount = diff.EntryDiffs.Count(d => d.ChangeType == ChangeType.Added);
        /// var removedCount = diff.EntryDiffs.Count(d => d.ChangeType == ChangeType.Removed);
        /// var modifiedCount = diff.EntryDiffs.Count(d => d.ChangeType == ChangeType.Modified);
        /// 
        /// Console.WriteLine($"Changes: +{addedCount} -{removedCount} ~{modifiedCount}");
        /// 
        /// // Show modified buses
        /// var modifiedBuses = diff.EntryDiffs
        ///     .Where(d => d.EntryType == "Bus" && d.ChangeType == ChangeType.Modified);
        /// 
        /// foreach (var entry in modifiedBuses)
        /// {
        ///     Console.WriteLine($"Bus {entry.EntryKey} changed:");
        ///     foreach (var change in entry.PropertyChanges)
        ///     {
        ///         Console.WriteLine($"  {change.PropertyPath}: {change.OldValue} -> {change.NewValue}");
        ///     }
        /// }
        /// </code>
        /// </example>
        public DataSetDiff Diff(DataSet? newer)
        {
            var diff = new DataSetDiff();

            // ArcFlash entries: keyed by (Id, Scenario)
            DiffArcFlashEntries(diff, newer);
            
            // ShortCircuit entries: keyed by (Id, Bus, Scenario)
            DiffShortCircuitEntries(diff, newer);
            
            // LVCB entries: keyed by ID
            DiffLVBreakerEntries(diff, newer);
            
            // Fuse entries: keyed by ID
            DiffFuseEntries(diff, newer);
            
            // Cable entries: keyed by ID
            DiffCableEntries(diff, newer);
            
            // Bus entries: keyed by ID
            DiffBusEntries(diff, newer);

            return diff;
        }

        #region Diff Helper Methods

        private void DiffArcFlashEntries(DataSetDiff diff, DataSet? newer)
        {
            var oldArc = this.ArcFlashEntries ?? new Dictionary<CompositeKey, ArcFlash>();
            var newArc = newer?.ArcFlashEntries ?? new Dictionary<CompositeKey, ArcFlash>();

            var allArcKeys = new HashSet<CompositeKey>(oldArc.Keys);
            foreach (var k in newArc.Keys) allArcKeys.Add(k);

            foreach (var key in allArcKeys)
            {
                // Build entryKey string from Components
                var entryKey = $"ArcFlash:{string.Join("|", key.Components)}";
                
                if (!oldArc.ContainsKey(key))
                {
                    diff.EntryDiffs.Add(new EntryDiff
                    {
                        EntryKey = entryKey,
                        EntryType = "ArcFlash",
                        ChangeType = ChangeType.Added,
                        PropertyChanges = DiffUtil.DiffObjects<ArcFlash>(null, newArc[key])
                    });
                }
                else if (!newArc.ContainsKey(key))
                {
                    diff.EntryDiffs.Add(new EntryDiff
                    {
                        EntryKey = entryKey,
                        EntryType = "ArcFlash",
                        ChangeType = ChangeType.Removed,
                        PropertyChanges = DiffUtil.DiffObjects<ArcFlash>(oldArc[key], null)
                    });
                }
                else
                {
                    var oldEntry = oldArc[key];
                    var newEntry = newArc[key];
                    var changes = DiffUtil.DiffObjects(oldEntry, newEntry);
                    if (changes.Count > 0)
                    {
                        diff.EntryDiffs.Add(new EntryDiff
                        {
                            EntryKey = entryKey,
                            EntryType = "ArcFlash",
                            ChangeType = ChangeType.Modified,
                            PropertyChanges = changes
                        });
                    }
                }
            }
        }

        private void DiffShortCircuitEntries(DataSetDiff diff, DataSet? newer)
        {
            var oldSc = this.ShortCircuitEntries ?? new Dictionary<CompositeKey, ShortCircuit>();
            var newSc = newer?.ShortCircuitEntries ?? new Dictionary<CompositeKey, ShortCircuit>();
            var allScKeys = new HashSet<CompositeKey>(oldSc.Keys);
            foreach (var k in newSc.Keys) allScKeys.Add(k);

            foreach (var key in allScKeys)
            {
                // Build entryKey string from Components
                var entryKey = $"ShortCircuit:{string.Join("|", key.Components)}";
                
                if (!oldSc.ContainsKey(key))
                {
                    diff.EntryDiffs.Add(new EntryDiff
                    {
                        EntryKey = entryKey,
                        EntryType = "ShortCircuit",
                        ChangeType = ChangeType.Added,
                        PropertyChanges = DiffUtil.DiffObjects<ShortCircuit>(null, newSc[key])
                    });
                }
                else if (!newSc.ContainsKey(key))
                {
                    diff.EntryDiffs.Add(new EntryDiff
                    {
                        EntryKey = entryKey,
                        EntryType = "ShortCircuit",
                        ChangeType = ChangeType.Removed,
                        PropertyChanges = DiffUtil.DiffObjects<ShortCircuit>(oldSc[key], null)
                    });
                }
                else
                {
                    var oldEntry = oldSc[key];
                    var newEntry = newSc[key];
                    var changes = DiffUtil.DiffObjects(oldEntry, newEntry);
                    if (changes.Count > 0)
                    {
                        diff.EntryDiffs.Add(new EntryDiff
                        {
                            EntryKey = entryKey,
                            EntryType = "ShortCircuit",
                            ChangeType = ChangeType.Modified,
                            PropertyChanges = changes
                        });
                    }
                }
            }
        }

        private void DiffLVBreakerEntries(DataSetDiff diff, DataSet? newer)
        {
            var oldLv = this.LVBreakerEntries ?? new Dictionary<CompositeKey, LVBreaker>();
            var newLv = newer?.LVBreakerEntries ?? new Dictionary<CompositeKey, LVBreaker>();
            var allLvKeys = new HashSet<CompositeKey>(oldLv.Keys);
            foreach (var k in newLv.Keys) allLvKeys.Add(k);

            foreach (var key in allLvKeys)
            {
                var entryKey = $"LVBreaker:{key}";
                if (!oldLv.ContainsKey(key))
                {
                    diff.EntryDiffs.Add(new EntryDiff
                    {
                        EntryKey = entryKey,
                        EntryType = "LVBreaker",
                        ChangeType = ChangeType.Added,
                        PropertyChanges = DiffUtil.DiffObjects<LVBreaker>(null, newLv[key])
                    });
                }
                else if (!newLv.ContainsKey(key))
                {
                    diff.EntryDiffs.Add(new EntryDiff
                    {
                        EntryKey = entryKey,
                        EntryType = "LVBreaker",
                        ChangeType = ChangeType.Removed,
                        PropertyChanges = DiffUtil.DiffObjects<LVBreaker>(oldLv[key], null)
                    });
                }
                else
                {
                    var oldEntry = oldLv[key];
                    var newEntry = newLv[key];
                    var changes = DiffUtil.DiffObjects(oldEntry, newEntry);
                    if (changes.Count > 0)
                    {
                        diff.EntryDiffs.Add(new EntryDiff
                        {
                            EntryKey = entryKey,
                            EntryType = "LVBreaker",
                            ChangeType = ChangeType.Modified,
                            PropertyChanges = changes
                        });
                    }
                }
            }
        }

        private void DiffFuseEntries(DataSetDiff diff, DataSet? newer)
        {
            var oldFuse = this.FuseEntries ?? new Dictionary<CompositeKey, Fuse>();
            var newFuse = newer?.FuseEntries ?? new Dictionary<CompositeKey, Fuse>();
            var allFuseKeys = new HashSet<CompositeKey>(oldFuse.Keys);
            foreach (var k in newFuse.Keys) allFuseKeys.Add(k);
            
            foreach (var key in allFuseKeys)
            {
                var entryKey = $"Fuse:{key}";
                if (!oldFuse.ContainsKey(key))
                {
                    diff.EntryDiffs.Add(new EntryDiff
                    {
                        EntryKey = entryKey,
                        EntryType = "Fuse",
                        ChangeType = ChangeType.Added,
                        PropertyChanges = DiffUtil.DiffObjects<Fuse>(null, newFuse[key])
                    });
                }
                else if (!newFuse.ContainsKey(key))
                {
                    diff.EntryDiffs.Add(new EntryDiff
                    {
                        EntryKey = entryKey,
                        EntryType = "Fuse",
                        ChangeType = ChangeType.Removed,
                        PropertyChanges = DiffUtil.DiffObjects<Fuse>(oldFuse[key], null)
                    });
                }
                else
                {
                    var changes = DiffUtil.DiffObjects(oldFuse[key], newFuse[key]);
                    if (changes.Count > 0)
                    {
                        diff.EntryDiffs.Add(new EntryDiff
                        {
                            EntryKey = entryKey,
                            EntryType = "Fuse",
                            ChangeType = ChangeType.Modified,
                            PropertyChanges = changes
                        });
                    }
                }
            }
        }

        private void DiffCableEntries(DataSetDiff diff, DataSet? newer)
        {
            var oldC = this.CableEntries ?? new Dictionary<CompositeKey, Cable>();
            var newC = newer?.CableEntries ?? new Dictionary<CompositeKey, Cable>();
            var allCKeys = new HashSet<CompositeKey>(oldC.Keys);
            foreach (var k in newC.Keys) allCKeys.Add(k);
            
            foreach (var key in allCKeys)
            {
                var entryKey = $"Cable:{key}";
                if (!oldC.ContainsKey(key))
                {
                    diff.EntryDiffs.Add(new EntryDiff
                    {
                        EntryKey = entryKey,
                        EntryType = "Cable",
                        ChangeType = ChangeType.Added,
                        PropertyChanges = DiffUtil.DiffObjects<Cable>(null, newC[key])
                    });
                }
                else if (!newC.ContainsKey(key))
                {
                    diff.EntryDiffs.Add(new EntryDiff
                    {
                        EntryKey = entryKey,
                        EntryType = "Cable",
                        ChangeType = ChangeType.Removed,
                        PropertyChanges = DiffUtil.DiffObjects<Cable>(oldC[key], null)
                    });
                }
                else
                {
                    var changes = DiffUtil.DiffObjects(oldC[key], newC[key]);
                    if (changes.Count > 0)
                    {
                        diff.EntryDiffs.Add(new EntryDiff
                        {
                            EntryKey = entryKey,
                            EntryType = "Cable",
                            ChangeType = ChangeType.Modified,
                            PropertyChanges = changes
                        });
                    }
                }
            }
        }

        private void DiffBusEntries(DataSetDiff diff, DataSet? newer)
        {
            var oldB = this.BusEntries ?? new Dictionary<CompositeKey, Bus>();
            var newB = newer?.BusEntries ?? new Dictionary<CompositeKey, Bus>();
            var allBKeys = new HashSet<CompositeKey>(oldB.Keys);
            foreach (var k in newB.Keys) allBKeys.Add(k);
            
            foreach (var key in allBKeys)
            {
                var entryKey = $"Bus:{key}";
                if (!oldB.ContainsKey(key))
                {
                    diff.EntryDiffs.Add(new EntryDiff
                    {
                        EntryKey = entryKey,
                        EntryType = "Bus",
                        ChangeType = ChangeType.Added,
                        PropertyChanges = DiffUtil.DiffObjects<Bus>(null, newB[key])
                    });
                }
                else if (!newB.ContainsKey(key))
                {
                    diff.EntryDiffs.Add(new EntryDiff
                    {
                        EntryKey = entryKey,
                        EntryType = "Bus",
                        ChangeType = ChangeType.Removed,
                        PropertyChanges = DiffUtil.DiffObjects<Bus>(oldB[key], null)
                    });
                }
                else
                {
                    var changes = DiffUtil.DiffObjects(oldB[key], newB[key]);
                    if (changes.Count > 0)
                    {
                        diff.EntryDiffs.Add(new EntryDiff
                        {
                            EntryKey = entryKey,
                            EntryType = "Bus",
                            ChangeType = ChangeType.Modified,
                            PropertyChanges = changes
                        });
                    }
                }
            }
        }

        #endregion
    }
}
