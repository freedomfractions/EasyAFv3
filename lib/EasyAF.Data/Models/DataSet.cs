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
        /// Gets or sets the dictionary of arc flash study results, keyed by (Id, Scenario).
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>Key Structure:</strong> Composite tuple of (Id, Scenario)
        /// </para>
        /// <list type="bullet">
        /// <item><description><strong>Id</strong>: The bus or location identifier where the arc flash study was performed</description></item>
        /// <item><description><strong>Scenario</strong>: The fault scenario name (e.g., "Main-Min", "Main-Max", "Service-Min")</description></item>
        /// </list>
        /// <para>
        /// Multiple scenarios can exist for the same Id (different operating conditions, configurations, etc.).
        /// </para>
        /// <para>
        /// <strong>Example Access:</strong>
        /// <code>
        /// var key = ("BUS-001", "Main-Min");
        /// if (dataSet.ArcFlashEntries.TryGetValue(key, out var entry))
        /// {
        ///     Console.WriteLine($"Incident Energy: {entry.IncidentEnergy} cal/cm²");
        /// }
        /// </code>
        /// </para>
        /// </remarks>
        public Dictionary<(string Id, string Scenario), ArcFlash>? ArcFlashEntries { get; set; } = new Dictionary<(string, string), ArcFlash>();

        /// <summary>
        /// Gets or sets the dictionary of short circuit study results, keyed by (Id, Bus, Scenario).
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>Key Structure:</strong> Composite triple tuple of (Id, Bus, Scenario)
        /// </para>
        /// <list type="bullet">
        /// <item><description><strong>Id</strong>: The device or equipment identifier</description></item>
        /// <item><description><strong>Bus</strong>: The bus location where the device is connected</description></item>
        /// <item><description><strong>Scenario</strong>: The fault scenario name (e.g., "Main-Max", "Service-Max")</description></item>
        /// </list>
        /// <para>
        /// The triple key allows the same device (Id) to have study results at multiple buses
        /// and across multiple scenarios.
        /// </para>
        /// <para>
        /// <strong>Example Access:</strong>
        /// <code>
        /// var key = ("CB-123", "BUS-001", "Main-Max");
        /// if (dataSet.ShortCircuitEntries.TryGetValue(key, out var entry))
        /// {
        ///     Console.WriteLine($"Available Fault Current: {entry.DutyKA} kA");
        /// }
        /// </code>
        /// </para>
        /// </remarks>
        public Dictionary<(string Id, string Bus, string Scenario), ShortCircuit>? ShortCircuitEntries { get; set; } = new Dictionary<(string, string, string), ShortCircuit>();

        /// <summary>
        /// Gets or sets the dictionary of low voltage circuit breaker entries, keyed by Id.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Trip unit settings are flattened onto the LVCB model as TripUnit* properties.
        /// </para>
        /// </remarks>
        public Dictionary<string, LVCB>? LVCBEntries { get; set; } = new Dictionary<string, LVCB>();

        /// <summary>
        /// Gets or sets the dictionary of fuse entries, keyed by Id.
        /// </summary>
        /// <remarks>
        /// Fuses are protective devices with simpler structure than circuit breakers.
        /// Common properties include Manufacturer, Style, Rating, and Voltage.
        /// </remarks>
        public Dictionary<string, Fuse>? FuseEntries { get; set; } = new Dictionary<string, Fuse>();

        /// <summary>
        /// Gets or sets the dictionary of cable/conductor entries, keyed by Id.
        /// </summary>
        /// <remarks>
        /// Cables represent the conductors connecting electrical equipment.
        /// Properties typically include Size, Insulation type, Ampacity, and Length.
        /// </remarks>
        public Dictionary<string, Cable>? CableEntries { get; set; } = new Dictionary<string, Cable>();

        /// <summary>
        /// Gets or sets the dictionary of bus/switchgear entries, keyed by Id.
        /// </summary>
        /// <remarks>
        /// Buses are the primary nodes in the electrical system where equipment connects.
        /// Properties include Name, Voltage, Phase configuration, and Grounding.
        /// </remarks>
        public Dictionary<string, Bus>? BusEntries { get; set; } = new Dictionary<string, Bus>();

        /// <summary>
        /// Gets or sets the dictionary of transformer entries, keyed by Id.
        /// </summary>
        public Dictionary<string, Transformer>? TransformerEntries { get; set; } = new Dictionary<string, Transformer>();

        /// <summary>
        /// Gets or sets the dictionary of motor entries, keyed by Id.
        /// </summary>
        public Dictionary<string, Motor>? MotorEntries { get; set; } = new Dictionary<string, Motor>();

        /// <summary>
        /// Gets or sets the dictionary of generator entries, keyed by Id.
        /// </summary>
        public Dictionary<string, Generator>? GeneratorEntries { get; set; } = new Dictionary<string, Generator>();

        /// <summary>
        /// Gets or sets the dictionary of utility connection entries, keyed by Id.
        /// </summary>
        public Dictionary<string, Utility>? UtilityEntries { get; set; } = new Dictionary<string, Utility>();

        /// <summary>
        /// Gets or sets the dictionary of capacitor entries, keyed by Id.
        /// </summary>
        public Dictionary<string, Capacitor>? CapacitorEntries { get; set; } = new Dictionary<string, Capacitor>();

        /// <summary>
        /// Gets or sets the dictionary of load entries, keyed by Id.
        /// </summary>
        public Dictionary<string, Load>? LoadEntries { get; set; } = new Dictionary<string, Load>();

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
            DiffLVCBEntries(diff, newer);
            
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
            var oldArc = this.ArcFlashEntries ?? new Dictionary<(string, string), ArcFlash>();
            var newArc = newer?.ArcFlashEntries ?? new Dictionary<(string, string), ArcFlash>();

            var allArcKeys = new HashSet<(string, string)>(oldArc.Keys);
            foreach (var k in newArc.Keys) allArcKeys.Add(k);

            foreach (var key in allArcKeys)
            {
                var entryKey = $"ArcFlash:{key.Item1}|{key.Item2}";
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
            var oldSc = this.ShortCircuitEntries ?? new Dictionary<(string, string, string), ShortCircuit>();
            var newSc = newer?.ShortCircuitEntries ?? new Dictionary<(string, string, string), ShortCircuit>();
            var allScKeys = new HashSet<(string, string, string)>(oldSc.Keys);
            foreach (var k in newSc.Keys) allScKeys.Add(k);

            foreach (var key in allScKeys)
            {
                var entryKey = $"ShortCircuit:{key.Item1}|{key.Item2}|{key.Item3}";
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

        private void DiffLVCBEntries(DataSetDiff diff, DataSet? newer)
        {
            var oldLv = this.LVCBEntries ?? new Dictionary<string, LVCB>();
            var newLv = newer?.LVCBEntries ?? new Dictionary<string, LVCB>();
            var allLvKeys = new HashSet<string>(oldLv.Keys);
            foreach (var k in newLv.Keys) allLvKeys.Add(k);

            foreach (var key in allLvKeys)
            {
                var entryKey = $"LVCB:{key}";
                if (!oldLv.ContainsKey(key))
                {
                    diff.EntryDiffs.Add(new EntryDiff
                    {
                        EntryKey = entryKey,
                        EntryType = "LVCB",
                        ChangeType = ChangeType.Added,
                        PropertyChanges = DiffUtil.DiffObjects<LVCB>(null, newLv[key])
                    });
                }
                else if (!newLv.ContainsKey(key))
                {
                    diff.EntryDiffs.Add(new EntryDiff
                    {
                        EntryKey = entryKey,
                        EntryType = "LVCB",
                        ChangeType = ChangeType.Removed,
                        PropertyChanges = DiffUtil.DiffObjects<LVCB>(oldLv[key], null)
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
                            EntryType = "LVCB",
                            ChangeType = ChangeType.Modified,
                            PropertyChanges = changes
                        });
                    }
                }
            }
        }

        private void DiffFuseEntries(DataSetDiff diff, DataSet? newer)
        {
            var oldFuse = this.FuseEntries ?? new Dictionary<string, Fuse>();
            var newFuse = newer?.FuseEntries ?? new Dictionary<string, Fuse>();
            var allFuseKeys = new HashSet<string>(oldFuse.Keys);
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
            var oldC = this.CableEntries ?? new Dictionary<string, Cable>();
            var newC = newer?.CableEntries ?? new Dictionary<string, Cable>();
            var allCKeys = new HashSet<string>(oldC.Keys);
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
            var oldB = this.BusEntries ?? new Dictionary<string, Bus>();
            var newB = newer?.BusEntries ?? new Dictionary<string, Bus>();
            var allBKeys = new HashSet<string>(oldB.Keys);
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
