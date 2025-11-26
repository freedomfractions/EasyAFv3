using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using EasyAF.Data.Models;
using EasyAF.Data.Extensions;
using EasyAF.Modules.Project.Models;
using Serilog;

namespace EasyAF.Modules.Project.ViewModels
{
    /// <summary>
    /// Partial class containing Statistics and Tree Building functionality.
    /// </summary>
    public partial class ProjectSummaryViewModel
    {
        #region Statistics Management

        /// <summary>
        /// Refreshes statistics display.
        /// </summary>
        /// <remarks>
        /// This should be called manually after data import operations.
        /// Since Project class doesn't implement INotifyPropertyChanged,
        /// we can't automatically detect changes.
        /// </remarks>
        public void RefreshStatistics()
        {
            RefreshStatistics(triggerHighlights: false); // Don't trigger highlights by default
        }

        /// <summary>
        /// Refreshes statistics display with optional highlighting.
        /// </summary>
        /// <param name="triggerHighlights">If true, highlights changed cells after refresh.</param>
        private void RefreshStatistics(bool triggerHighlights)
        {
            // Disable highlights during row rebuild to prevent expand/collapse from triggering animations
            AllowCellHighlights = false;

            // Capture old counts before refreshing (for change detection)
            var oldCounts = new System.Collections.Generic.Dictionary<string, (int newCount, int oldCount)>();
            
            if (triggerHighlights)
            {
                foreach (var row in DataStatisticsRows)
                {
                    var key = row.ScenarioName != null 
                        ? $"{row.DataTypeName}:{row.ScenarioName}" 
                        : row.DataTypeName;
                    oldCounts[key] = (row.NewCount, row.OldCount);
                }
            }

            // Debug logging to diagnose load issues
            Log.Debug("RefreshStatistics called. NewData null: {NewDataNull}, OldData null: {OldDataNull}", 
                _document.Project.NewData == null, 
                _document.Project.OldData == null);
            
            if (_document.Project.NewData != null)
            {
                Log.Debug("NewData stats - Arc Flash: {AF}, Short Circuit: {SC}, Buses: {Bus}, Breakers: {BR}, Fuses: {F}",
                    _document.Project.NewData.ArcFlashEntries?.Count ?? 0,
                    _document.Project.NewData.ShortCircuitEntries?.Count ?? 0,
                    _document.Project.NewData.BusEntries?.Count ?? 0,
                    _document.Project.NewData.LVBreakerEntries?.Count ?? 0,
                    _document.Project.NewData.FuseEntries?.Count ?? 0);
            }

            // Rebuild table rows (new unified view)
            DataStatisticsRows.Clear();
            foreach (var row in BuildStatisticsRows(_document.Project.NewData, _document.Project.OldData))
            {
                DataStatisticsRows.Add(row);
                // Subscribe to IsExpanded changes
                row.PropertyChanged += StatisticsRow_PropertyChanged;

                // Only detect changes and trigger highlights if explicitly requested
                if (triggerHighlights)
                {
                    var key = row.ScenarioName != null 
                        ? $"{row.DataTypeName}:{row.ScenarioName}" 
                        : row.DataTypeName;

                    if (oldCounts.TryGetValue(key, out var oldValues))
                    {
                        // Check if New count changed
                        if (row.NewCount != oldValues.newCount && row.NewCount > 0)
                        {
                            row.IsNewCountHighlighted = true;
                            Log.Debug("Highlighting New count change for {Key}: {Old} ? {New}", 
                                key, oldValues.newCount, row.NewCount);
                        }

                        // Check if Old count changed
                        if (row.OldCount != oldValues.oldCount && row.OldCount > 0)
                        {
                            row.IsOldCountHighlighted = true;
                            Log.Debug("Highlighting Old count change for {Key}: {Old} ? {New}", 
                                key, oldValues.oldCount, row.OldCount);
                        }
                    }
                    else if (row.NewCount > 0 || row.OldCount > 0)
                    {
                        // New row appeared - highlight whichever has data
                        if (row.NewCount > 0) row.IsNewCountHighlighted = true;
                        if (row.OldCount > 0) row.IsOldCountHighlighted = true;
                        Log.Debug("Highlighting new row {Key}: New={New}, Old={Old}", 
                            key, row.NewCount, row.OldCount);
                    }
                }
            }

            // Auto-reset highlight flags after animation completes (2s hold + 1.5s fade = 3.5s total)
            if (triggerHighlights)
            {
                var resetTimer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(4) // Slightly longer than animation to be safe
                };
                resetTimer.Tick += (s, e) =>
                {
                    foreach (var row in DataStatisticsRows)
                    {
                        row.IsNewCountHighlighted = false;
                        row.IsOldCountHighlighted = false;
                        foreach (var child in row.Children)
                        {
                            child.IsNewCountHighlighted = false;
                            child.IsOldCountHighlighted = false;
                        }
                    }
                    resetTimer.Stop();
                    Log.Debug("Auto-reset all highlight flags after animation completion");
                };
                resetTimer.Start();
            }

            // Build flattened visible list
            RebuildVisibleRows();

            // Rebuild tree nodes (legacy view - kept for compatibility)
            NewDataTreeNodes.Clear();
            OldDataTreeNodes.Clear();

            foreach (var node in BuildTreeNodes(_document.Project.NewData))
                NewDataTreeNodes.Add(node);

            foreach (var node in BuildTreeNodes(_document.Project.OldData))
                OldDataTreeNodes.Add(node);

            // Legacy flat properties (kept for backward compatibility)
            RaisePropertyChanged(nameof(NewBusCount));
            RaisePropertyChanged(nameof(NewBreakerCount));
            RaisePropertyChanged(nameof(NewFuseCount));
            RaisePropertyChanged(nameof(NewShortCircuitCount));
            RaisePropertyChanged(nameof(NewArcFlashCount));
            RaisePropertyChanged(nameof(OldBusCount));
            RaisePropertyChanged(nameof(OldBreakerCount));
            RaisePropertyChanged(nameof(OldFuseCount));
            RaisePropertyChanged(nameof(OldShortCircuitCount));
            RaisePropertyChanged(nameof(OldArcFlashCount));

            // Re-enable highlights if we were triggering them for this refresh
            if (triggerHighlights)
            {
                AllowCellHighlights = true;
            }
        }

        private void StatisticsRow_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataStatisticsRowViewModel.IsExpanded))
            {
                // Just rebuild the visible list - no special highlight logic needed
                RebuildVisibleRows();
            }
        }

        private void RebuildVisibleRows()
        {
            VisibleStatisticsRows.Clear();
            foreach (var row in DataStatisticsRows)
            {
                VisibleStatisticsRows.Add(row);
                if (row.IsExpanded)
                {
                    foreach (var child in row.Children)
                    {
                        VisibleStatisticsRows.Add(child);
                    }
                }
            }
        }

        #endregion

        #region Dataset Management

        /// <summary>
        /// Checks if either NewData or OldData contains any entries.
        /// </summary>
        /// <returns>True if any data exists, false if both datasets are empty.</returns>
        private bool HasDatasetEntries()
        {
            return HasDatasetEntriesInternal(_document.Project.NewData) ||
                   HasDatasetEntriesInternal(_document.Project.OldData);
        }

        /// <summary>
        /// Checks if a specific DataSet has any entries.
        /// </summary>
        private bool HasDatasetEntriesInternal(DataSet? dataset)
        {
            if (dataset == null) return false;

            // Check all dictionary properties for entries
            return (dataset.BusEntries?.Count ?? 0) > 0 ||
                   (dataset.LVBreakerEntries?.Count ?? 0) > 0 ||
                   (dataset.FuseEntries?.Count ?? 0) > 0 ||
                   (dataset.CableEntries?.Count ?? 0) > 0 ||
                   (dataset.ArcFlashEntries?.Count ?? 0) > 0 ||
                   (dataset.ShortCircuitEntries?.Count ?? 0) > 0 ||
                   (dataset.AFDEntries?.Count ?? 0) > 0 ||
                   (dataset.ATSEntries?.Count ?? 0) > 0 ||
                   (dataset.BatteryEntries?.Count ?? 0) > 0 ||
                   (dataset.BuswayEntries?.Count ?? 0) > 0 ||
                   (dataset.CapacitorEntries?.Count ?? 0) > 0 ||
                   (dataset.CLReactorEntries?.Count ?? 0) > 0 ||
                   (dataset.CTEntries?.Count ?? 0) > 0 ||
                   (dataset.FilterEntries?.Count ?? 0) > 0 ||
                   (dataset.GeneratorEntries?.Count ?? 0) > 0 ||
                   (dataset.HVBreakerEntries?.Count ?? 0) > 0 ||
                   (dataset.InverterEntries?.Count ?? 0) > 0 ||
                   (dataset.LoadEntries?.Count ?? 0) > 0 ||
                   (dataset.MCCEntries?.Count ?? 0) > 0 ||
                   (dataset.MeterEntries?.Count ?? 0) > 0 ||
                   (dataset.MotorEntries?.Count ?? 0) > 0 ||
                   (dataset.PanelEntries?.Count ?? 0) > 0 ||
                   (dataset.PhotovoltaicEntries?.Count ?? 0) > 0 ||
                   (dataset.POCEntries?.Count ?? 0) > 0 ||
                   (dataset.RectifierEntries?.Count ?? 0) > 0 ||
                   (dataset.RelayEntries?.Count ?? 0) > 0 ||
                   (dataset.ShuntEntries?.Count ?? 0) > 0 ||
                   (dataset.SwitchEntries?.Count ?? 0) > 0 ||
                   (dataset.Transformer2WEntries?.Count ?? 0) > 0 ||
                   (dataset.Transformer3WEntries?.Count ?? 0) > 0 ||
                   (dataset.TransmissionLineEntries?.Count ?? 0) > 0 ||
                   (dataset.UPSEntries?.Count ?? 0) > 0 ||
                   (dataset.UtilityEntries?.Count ?? 0) > 0 ||
                   (dataset.ZigzagTransformerEntries?.Count ?? 0) > 0;
        }

        /// <summary>
        /// Purges all entries from NewData and OldData datasets.
        /// Summary information is preserved.
        /// </summary>
        private void PurgeDatasets()
        {
            // Create fresh empty datasets
            _document.Project.NewData = new DataSet();
            _document.Project.OldData = new DataSet();

            Log.Information("Datasets purged due to project type change");
        }

        /// <summary>
        /// Logs scenario discovery results for verification and debugging.
        /// </summary>
        private void LogScenarioDiscovery(DataSet dataSet, bool isNewData)
        {
            var target = isNewData ? "New" : "Old";
            
            // Get available scenarios
            var scenarios = dataSet.GetAvailableScenarios();
            
            if (scenarios.Count > 0)
            {
                Log.Information("{Target} Data: Discovered {Count} scenario(s): {Scenarios}", 
                    target, scenarios.Count, string.Join(", ", scenarios));

                // Get detailed statistics per scenario
                var stats = dataSet.GetStatisticsByScenario();
                
                foreach (var dataType in stats.Keys.OrderBy(k => k))
                {
                    var scenarioStats = stats[dataType];
                    
                    if (scenarioStats.ContainsKey("(All)"))
                    {
                        // Non-scenario type
                        Log.Debug("{Target} Data: {DataType} = {Count} entries (no scenarios)",
                            target, dataType, scenarioStats["(All)"]);
                    }
                    else
                    {
                        // Scenario-based type
                        var isUniform = dataSet.IsScenariosUniform(dataType);
                        var uniformIndicator = isUniform ? "?" : "?";
                        
                        var scenarioSummary = string.Join(", ", 
                            scenarioStats.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}={kvp.Value}"));
                        
                        Log.Information("{Target} Data: {DataType} {Indicator} = {Summary}",
                            target, dataType, uniformIndicator, scenarioSummary);
                    }
                }
            }
            else
            {
                Log.Debug("{Target} Data: No scenarios discovered (non-scenario data types only)", target);
            }
        }

        #endregion

        #region Tree Building

        /// <summary>
        /// Builds tree nodes from DataSet statistics.
        /// </summary>
        /// <param name="dataSet">The DataSet to analyze.</param>
        /// <returns>Observable collection of root-level tree nodes.</returns>
        private ObservableCollection<DataTypeNodeViewModel> BuildTreeNodes(DataSet? dataSet)
        {
            var nodes = new ObservableCollection<DataTypeNodeViewModel>();

            if (dataSet == null)
                return nodes;

            var stats = dataSet.GetStatisticsByScenario();

            // Sort data types: scenario-based types first, then simple types
            var scenarioTypes = stats.Where(kvp => !kvp.Value.ContainsKey("(All)")).OrderBy(kvp => kvp.Key);
            var simpleTypes = stats.Where(kvp => kvp.Value.ContainsKey("(All)")).OrderBy(kvp => kvp.Key);

            // Add scenario-based types (Arc Flash, Short Circuit)
            foreach (var kvp in scenarioTypes)
            {
                var dataTypeName = kvp.Key;
                var scenarioCounts = kvp.Value;
                var totalCount = scenarioCounts.Values.Sum();

                var node = new DataTypeNodeViewModel
                {
                    DataTypeName = dataTypeName,
                    DisplayName = $"{GetFriendlyName(dataTypeName)} ({totalCount})",
                    Count = totalCount,
                    IsScenariosUniform = dataSet.IsScenariosUniform(dataTypeName)
                };

                // Add child nodes for each scenario
                foreach (var scenario in scenarioCounts.OrderBy(s => s.Key))
                {
                    node.Children.Add(new DataTypeNodeViewModel
                    {
                        DataTypeName = dataTypeName,
                        ScenarioName = scenario.Key,
                        DisplayName = $"{scenario.Key} ({scenario.Value})",
                        Count = scenario.Value
                    });
                }

                nodes.Add(node);
            }

            // Add simple types (Bus, LVCB, Fuse, etc.)
            foreach (var kvp in simpleTypes)
            {
                var dataTypeName = kvp.Key;
                var count = kvp.Value["(All)"];

                nodes.Add(new DataTypeNodeViewModel
                {
                    DataTypeName = dataTypeName,
                    DisplayName = $"{GetFriendlyName(dataTypeName)} ({count})",
                    Count = count
                });
            }

            return nodes;
        }

        /// <summary>
        /// Converts internal data type names to user-friendly display names.
        /// </summary>
        private string GetFriendlyName(string dataTypeName)
        {
            return dataTypeName switch
            {
                "ArcFlash" => "Arc Flash",
                "ShortCircuit" => "Short Circuit",
                "LVCB" => "Breakers",
                "HVBreaker" => "HV Breakers",
                "Transformer2W" => "2-Winding Transformers",
                "Transformer3W" => "3-Winding Transformers",
                "TransmissionLine" => "Transmission Lines",
                "CLReactor" => "Current Limiting Reactors",
                "ZigzagTransformer" => "Zigzag Transformers",
                _ => dataTypeName // Use as-is for others (Bus, Fuse, Cable, etc.)
            };
        }

        /// <summary>
        /// Builds statistics table rows comparing New and Old data.
        /// </summary>
        /// <param name="newData">New DataSet.</param>
        /// <param name="oldData">Old DataSet.</param>
        /// <returns>Observable collection of table rows with hierarchical scenario support.</returns>
        private ObservableCollection<DataStatisticsRowViewModel> BuildStatisticsRows(DataSet? newData, DataSet? oldData)
        {
            var rows = new ObservableCollection<DataStatisticsRowViewModel>();

            // Get statistics from both datasets
            var newStats = newData?.GetStatisticsByScenario() ?? new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, int>>();
            var oldStats = oldData?.GetStatisticsByScenario() ?? new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, int>>();

            // Collect all unique data type names from both datasets
            var allDataTypes = newStats.Keys.Union(oldStats.Keys).OrderBy(k => k);

            // Separate scenario-based types from simple types
            var scenarioTypes = allDataTypes.Where(dt => 
                (newStats.ContainsKey(dt) && !newStats[dt].ContainsKey("(All)")) ||
                (oldStats.ContainsKey(dt) && !oldStats[dt].ContainsKey("(All)")))
                .OrderBy(dt => dt);

            var simpleTypes = allDataTypes.Where(dt =>
                (newStats.ContainsKey(dt) && newStats[dt].ContainsKey("(All)")) ||
                (oldStats.ContainsKey(dt) && oldStats[dt].ContainsKey("(All)")))
                .OrderBy(dt => dt);

            // Add scenario-based types first (Arc Flash, Short Circuit)
            foreach (var dataTypeName in scenarioTypes)
            {
                var newScenarios = newStats.ContainsKey(dataTypeName) ? newStats[dataTypeName] : new System.Collections.Generic.Dictionary<string, int>();
                var oldScenarios = oldStats.ContainsKey(dataTypeName) ? oldStats[dataTypeName] : new System.Collections.Generic.Dictionary<string, int>();

                var newTotal = newScenarios.Values.Sum();
                var oldTotal = oldScenarios.Values.Sum();

                var row = new DataStatisticsRowViewModel
                {
                    DataTypeName = dataTypeName,
                    DisplayName = GetFriendlyName(dataTypeName),
                    NewCount = newTotal,
                    OldCount = oldTotal,
                    IsScenariosUniform = newData?.IsScenariosUniform(dataTypeName) ?? true
                };

                // Add child rows for each scenario
                var allScenarios = newScenarios.Keys.Union(oldScenarios.Keys).OrderBy(s => s);
                var scenarioList = allScenarios.ToList();
                
                for (int i = 0; i < scenarioList.Count; i++)
                {
                    var scenario = scenarioList[i];
                    var newCount = newScenarios.ContainsKey(scenario) ? newScenarios[scenario] : 0;
                    var oldCount = oldScenarios.ContainsKey(scenario) ? oldScenarios[scenario] : 0;

                    row.Children.Add(new DataStatisticsRowViewModel
                    {
                        DataTypeName = dataTypeName,
                        ScenarioName = scenario,
                        DisplayName = scenario,
                        NewCount = newCount,
                        OldCount = oldCount,
                        IsFirstChild = i == 0,
                        IsLastChild = i == scenarioList.Count - 1
                    });
                }

                rows.Add(row);
            }

            // Add simple types (Bus, Breakers, Fuses, etc.)
            foreach (var dataTypeName in simpleTypes)
            {
                var newCount = newStats.ContainsKey(dataTypeName) && newStats[dataTypeName].ContainsKey("(All)") 
                    ? newStats[dataTypeName]["(All)"] 
                    : 0;
                var oldCount = oldStats.ContainsKey(dataTypeName) && oldStats[dataTypeName].ContainsKey("(All)") 
                    ? oldStats[dataTypeName]["(All)"] 
                    : 0;

                rows.Add(new DataStatisticsRowViewModel
                {
                    DataTypeName = dataTypeName,
                    DisplayName = GetFriendlyName(dataTypeName),
                    NewCount = newCount,
                    OldCount = oldCount
                });
            }

            return rows;
        }

        #endregion
    }
}
