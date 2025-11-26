using System;
using System.Collections.Generic;
using System.Linq;
using EasyAF.Data.Models;
using Serilog;

namespace EasyAF.Modules.Project.ViewModels
{
    /// <summary>
    /// Partial class containing Import Helper methods.
    /// </summary>
    public partial class ProjectSummaryViewModel
    {
        #region Import Helpers

        /// <summary>
        /// Checks if importing the specified files will overwrite existing data.
        /// Pre-scans files to detect which data types they contain.
        /// </summary>
        /// <param name="filePaths">Files to be imported.</param>
        /// <param name="mappingConfig">Mapping configuration to use.</param>
        /// <param name="targetDataSet">Target dataset to check.</param>
        /// <returns>Tuple of (willOverwrite, affectedDataTypes)</returns>
        private (bool willOverwrite, List<string> affectedTypes) 
            WillImportOverwriteData(string[] filePaths, EasyAF.Import.MappingConfig mappingConfig, DataSet? targetDataSet)
        {
            var affectedTypes = new List<string>();
            
            if (targetDataSet == null || !HasDatasetEntriesInternal(targetDataSet))
                return (false, affectedTypes); // No existing data - no conflict

            try
            {
                // Create temporary dataset to see what data types the files contain
                var tempDataSet = new DataSet();
                var importManager = new EasyAF.Import.ImportManager();

                foreach (var filePath in filePaths)
                {
                    try
                    {
                        importManager.Import(filePath, mappingConfig, tempDataSet);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Error pre-scanning file {File} for conflict detection", System.IO.Path.GetFileName(filePath));
                        // Continue with other files - we'll catch import errors later
                    }
                }

                // Check which data types in tempDataSet already have data in targetDataSet
                if ((tempDataSet.BusEntries?.Count ?? 0) > 0 && (targetDataSet.BusEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"Bus ({targetDataSet.BusEntries!.Count} existing)");
                if ((tempDataSet.LVBreakerEntries?.Count ?? 0) > 0 && (targetDataSet.LVBreakerEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"LVBreaker ({targetDataSet.LVBreakerEntries!.Count} existing)");
                if ((tempDataSet.FuseEntries?.Count ?? 0) > 0 && (targetDataSet.FuseEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"Fuse ({targetDataSet.FuseEntries!.Count} existing)");
                if ((tempDataSet.CableEntries?.Count ?? 0) > 0 && (targetDataSet.CableEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"Cable ({targetDataSet.CableEntries!.Count} existing)");
                if ((tempDataSet.ArcFlashEntries?.Count ?? 0) > 0 && (targetDataSet.ArcFlashEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"ArcFlash ({targetDataSet.ArcFlashEntries!.Count} existing)");
                if ((tempDataSet.ShortCircuitEntries?.Count ?? 0) > 0 && (targetDataSet.ShortCircuitEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"ShortCircuit ({targetDataSet.ShortCircuitEntries!.Count} existing)");
                
                // Extended equipment types
                if ((tempDataSet.AFDEntries?.Count ?? 0) > 0 && (targetDataSet.AFDEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"AFD ({targetDataSet.AFDEntries!.Count} existing)");
                if ((tempDataSet.ATSEntries?.Count ?? 0) > 0 && (targetDataSet.ATSEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"ATS ({targetDataSet.ATSEntries!.Count} existing)");
                if ((tempDataSet.BatteryEntries?.Count ?? 0) > 0 && (targetDataSet.BatteryEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"Battery ({targetDataSet.BatteryEntries!.Count} existing)");
                if ((tempDataSet.BuswayEntries?.Count ?? 0) > 0 && (targetDataSet.BuswayEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"Busway ({targetDataSet.BuswayEntries!.Count} existing)");
                if ((tempDataSet.CapacitorEntries?.Count ?? 0) > 0 && (targetDataSet.CapacitorEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"Capacitor ({targetDataSet.CapacitorEntries!.Count} existing)");
                if ((tempDataSet.CLReactorEntries?.Count ?? 0) > 0 && (targetDataSet.CLReactorEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"CLReactor ({targetDataSet.CLReactorEntries!.Count} existing)");
                if ((tempDataSet.CTEntries?.Count ?? 0) > 0 && (targetDataSet.CTEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"CT ({targetDataSet.CTEntries!.Count} existing)");
                if ((tempDataSet.FilterEntries?.Count ?? 0) > 0 && (targetDataSet.FilterEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"Filter ({targetDataSet.FilterEntries!.Count} existing)");
                if ((tempDataSet.GeneratorEntries?.Count ?? 0) > 0 && (targetDataSet.GeneratorEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"Generator ({targetDataSet.GeneratorEntries!.Count} existing)");
                if ((tempDataSet.HVBreakerEntries?.Count ?? 0) > 0 && (targetDataSet.HVBreakerEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"HVBreaker ({targetDataSet.HVBreakerEntries!.Count} existing)");
                if ((tempDataSet.InverterEntries?.Count ?? 0) > 0 && (targetDataSet.InverterEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"Inverter ({targetDataSet.InverterEntries!.Count} existing)");
                if ((tempDataSet.LoadEntries?.Count ?? 0) > 0 && (targetDataSet.LoadEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"Load ({targetDataSet.LoadEntries!.Count} existing)");
                if ((tempDataSet.MCCEntries?.Count ?? 0) > 0 && (targetDataSet.MCCEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"MCC ({targetDataSet.MCCEntries!.Count} existing)");
                if ((tempDataSet.MeterEntries?.Count ?? 0) > 0 && (targetDataSet.MeterEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"Meter ({targetDataSet.MeterEntries!.Count} existing)");
                if ((tempDataSet.MotorEntries?.Count ?? 0) > 0 && (targetDataSet.MotorEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"Motor ({targetDataSet.MotorEntries!.Count} existing)");
                if ((tempDataSet.PanelEntries?.Count ?? 0) > 0 && (targetDataSet.PanelEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"Panel ({targetDataSet.PanelEntries!.Count} existing)");
                if ((tempDataSet.PhotovoltaicEntries?.Count ?? 0) > 0 && (targetDataSet.PhotovoltaicEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"Photovoltaic ({targetDataSet.PhotovoltaicEntries!.Count} existing)");
                if ((tempDataSet.POCEntries?.Count ?? 0) > 0 && (targetDataSet.POCEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"POC ({targetDataSet.POCEntries!.Count} existing)");
                if ((tempDataSet.RectifierEntries?.Count ?? 0) > 0 && (targetDataSet.RectifierEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"Rectifier ({targetDataSet.RectifierEntries!.Count} existing)");
                if ((tempDataSet.RelayEntries?.Count ?? 0) > 0 && (targetDataSet.RelayEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"Relay ({targetDataSet.RelayEntries!.Count} existing)");
                if ((tempDataSet.ShuntEntries?.Count ?? 0) > 0 && (targetDataSet.ShuntEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"Shunt ({targetDataSet.ShuntEntries!.Count} existing)");
                if ((tempDataSet.SwitchEntries?.Count ?? 0) > 0 && (targetDataSet.SwitchEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"Switch ({targetDataSet.SwitchEntries!.Count} existing)");
                if ((tempDataSet.Transformer2WEntries?.Count ?? 0) > 0 && (targetDataSet.Transformer2WEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"Transformer2W ({targetDataSet.Transformer2WEntries!.Count} existing)");
                if ((tempDataSet.Transformer3WEntries?.Count ?? 0) > 0 && (targetDataSet.Transformer3WEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"Transformer3W ({targetDataSet.Transformer3WEntries!.Count} existing)");
                if ((tempDataSet.TransmissionLineEntries?.Count ?? 0) > 0 && (targetDataSet.TransmissionLineEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"TransmissionLine ({targetDataSet.TransmissionLineEntries!.Count} existing)");
                if ((tempDataSet.UPSEntries?.Count ?? 0) > 0 && (targetDataSet.UPSEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"UPS ({targetDataSet.UPSEntries!.Count} existing)");
                if ((tempDataSet.UtilityEntries?.Count ?? 0) > 0 && (targetDataSet.UtilityEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"Utility ({targetDataSet.UtilityEntries!.Count} existing)");
                if ((tempDataSet.ZigzagTransformerEntries?.Count ?? 0) > 0 && (targetDataSet.ZigzagTransformerEntries?.Count ?? 0) > 0)
                    affectedTypes.Add($"ZigzagTransformer ({targetDataSet.ZigzagTransformerEntries!.Count} existing)");

                return (affectedTypes.Count > 0, affectedTypes);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during smart conflict detection - falling back to conservative warning");
                // Fall back to conservative behavior: warn if ANY data exists
                return (true, new List<string> { "Unknown (error during detection)" });
            }
        }

        /// <summary>
        /// Clears specific data types from a DataSet (Standard mode replacement behavior).
        /// </summary>
        /// <param name="dataSet">The dataset to clear data types from.</param>
        /// <param name="dataTypesToClear">List of data type names to clear (e.g., "Bus", "ArcFlash").</param>
        private void ClearDataTypes(DataSet dataSet, List<string> dataTypesToClear)
        {
            foreach (var dataType in dataTypesToClear)
            {
                // Extract just the type name (before the count in parentheses)
                var typeName = dataType.Split('(')[0].Trim();
                
                switch (typeName)
                {
                    case "Bus":
                        var busCount = dataSet.BusEntries?.Count ?? 0;
                        dataSet.BusEntries?.Clear();
                        Log.Information("Cleared {Count} Bus entries for replacement import", busCount);
                        break;
                    case "LVBreaker":
                        var lvbCount = dataSet.LVBreakerEntries?.Count ?? 0;
                        dataSet.LVBreakerEntries?.Clear();
                        Log.Information("Cleared {Count} LVBreaker entries for replacement import", lvbCount);
                        break;
                    case "Fuse":
                        var fuseCount = dataSet.FuseEntries?.Count ?? 0;
                        dataSet.FuseEntries?.Clear();
                        Log.Information("Cleared {Count} Fuse entries for replacement import", fuseCount);
                        break;
                    case "Cable":
                        var cableCount = dataSet.CableEntries?.Count ?? 0;
                        dataSet.CableEntries?.Clear();
                        Log.Information("Cleared {Count} Cable entries for replacement import", cableCount);
                        break;
                    case "ArcFlash":
                        var afCount = dataSet.ArcFlashEntries?.Count ?? 0;
                        dataSet.ArcFlashEntries?.Clear();
                        Log.Information("Cleared {Count} ArcFlash entries for replacement import", afCount);
                        break;
                    case "ShortCircuit":
                        var scCount = dataSet.ShortCircuitEntries?.Count ?? 0;
                        dataSet.ShortCircuitEntries?.Clear();
                        Log.Information("Cleared {Count} ShortCircuit entries for replacement import", scCount);
                        break;
                    case "AFD":
                        var afdCount = dataSet.AFDEntries?.Count ?? 0;
                        dataSet.AFDEntries?.Clear();
                        Log.Information("Cleared {Count} AFD entries for replacement import", afdCount);
                        break;
                    case "ATS":
                        var atsCount = dataSet.ATSEntries?.Count ?? 0;
                        dataSet.ATSEntries?.Clear();
                        Log.Information("Cleared {Count} ATS entries for replacement import", atsCount);
                        break;
                    case "Battery":
                        var battCount = dataSet.BatteryEntries?.Count ?? 0;
                        dataSet.BatteryEntries?.Clear();
                        Log.Information("Cleared {Count} Battery entries for replacement import", battCount);
                        break;
                    case "Busway":
                        var bwCount = dataSet.BuswayEntries?.Count ?? 0;
                        dataSet.BuswayEntries?.Clear();
                        Log.Information("Cleared {Count} Busway entries for replacement import", bwCount);
                        break;
                    case "Capacitor":
                        var capCount = dataSet.CapacitorEntries?.Count ?? 0;
                        dataSet.CapacitorEntries?.Clear();
                        Log.Information("Cleared {Count} Capacitor entries for replacement import", capCount);
                        break;
                    case "CLReactor":
                        var clrCount = dataSet.CLReactorEntries?.Count ?? 0;
                        dataSet.CLReactorEntries?.Clear();
                        Log.Information("Cleared {Count} CLReactor entries for replacement import", clrCount);
                        break;
                    case "CT":
                        var ctCount = dataSet.CTEntries?.Count ?? 0;
                        dataSet.CTEntries?.Clear();
                        Log.Information("Cleared {Count} CT entries for replacement import", ctCount);
                        break;
                    case "Filter":
                        var filtCount = dataSet.FilterEntries?.Count ?? 0;
                        dataSet.FilterEntries?.Clear();
                        Log.Information("Cleared {Count} Filter entries for replacement import", filtCount);
                        break;
                    case "Generator":
                        var genCount = dataSet.GeneratorEntries?.Count ?? 0;
                        dataSet.GeneratorEntries?.Clear();
                        Log.Information("Cleared {Count} Generator entries for replacement import", genCount);
                        break;
                    case "HVBreaker":
                        var hvbCount = dataSet.HVBreakerEntries?.Count ?? 0;
                        dataSet.HVBreakerEntries?.Clear();
                        Log.Information("Cleared {Count} HVBreaker entries for replacement import", hvbCount);
                        break;
                    case "Inverter":
                        var invCount = dataSet.InverterEntries?.Count ?? 0;
                        dataSet.InverterEntries?.Clear();
                        Log.Information("Cleared {Count} Inverter entries for replacement import", invCount);
                        break;
                    case "Load":
                        var loadCount = dataSet.LoadEntries?.Count ?? 0;
                        dataSet.LoadEntries?.Clear();
                        Log.Information("Cleared {Count} Load entries for replacement import", loadCount);
                        break;
                    case "MCC":
                        var mccCount = dataSet.MCCEntries?.Count ?? 0;
                        dataSet.MCCEntries?.Clear();
                        Log.Information("Cleared {Count} MCC entries for replacement import", mccCount);
                        break;
                    case "Meter":
                        var mtrCount = dataSet.MeterEntries?.Count ?? 0;
                        dataSet.MeterEntries?.Clear();
                        Log.Information("Cleared {Count} Meter entries for replacement import", mtrCount);
                        break;
                    case "Motor":
                        var motCount = dataSet.MotorEntries?.Count ?? 0;
                        dataSet.MotorEntries?.Clear();
                        Log.Information("Cleared {Count} Motor entries for replacement import", motCount);
                        break;
                    case "Panel":
                        var panCount = dataSet.PanelEntries?.Count ?? 0;
                        dataSet.PanelEntries?.Clear();
                        Log.Information("Cleared {Count} Panel entries for replacement import", panCount);
                        break;
                    case "Photovoltaic":
                        var pvCount = dataSet.PhotovoltaicEntries?.Count ?? 0;
                        dataSet.PhotovoltaicEntries?.Clear();
                        Log.Information("Cleared {Count} Photovoltaic entries for replacement import", pvCount);
                        break;
                    case "POC":
                        var pocCount = dataSet.POCEntries?.Count ?? 0;
                        dataSet.POCEntries?.Clear();
                        Log.Information("Cleared {Count} POC entries for replacement import", pocCount);
                        break;
                    case "Rectifier":
                        var rectCount = dataSet.RectifierEntries?.Count ?? 0;
                        dataSet.RectifierEntries?.Clear();
                        Log.Information("Cleared {Count} Rectifier entries for replacement import", rectCount);
                        break;
                    case "Relay":
                        var relCount = dataSet.RelayEntries?.Count ?? 0;
                        dataSet.RelayEntries?.Clear();
                        Log.Information("Cleared {Count} Relay entries for replacement import", relCount);
                        break;
                    case "Shunt":
                        var shCount = dataSet.ShuntEntries?.Count ?? 0;
                        dataSet.ShuntEntries?.Clear();
                        Log.Information("Cleared {Count} Shunt entries for replacement import", shCount);
                        break;
                    case "Switch":
                        var swCount = dataSet.SwitchEntries?.Count ?? 0;
                        dataSet.SwitchEntries?.Clear();
                        Log.Information("Cleared {Count} Switch entries for replacement import", swCount);
                        break;
                    case "Transformer2W":
                        var t2wCount = dataSet.Transformer2WEntries?.Count ?? 0;
                        dataSet.Transformer2WEntries?.Clear();
                        Log.Information("Cleared {Count} Transformer2W entries for replacement import", t2wCount);
                        break;
                    case "Transformer3W":
                        var t3wCount = dataSet.Transformer3WEntries?.Count ?? 0;
                        dataSet.Transformer3WEntries?.Clear();
                        Log.Information("Cleared {Count} Transformer3W entries for replacement import", t3wCount);
                        break;
                    case "TransmissionLine":
                        var tlCount = dataSet.TransmissionLineEntries?.Count ?? 0;
                        dataSet.TransmissionLineEntries?.Clear();
                        Log.Information("Cleared {Count} TransmissionLine entries for replacement import", tlCount);
                        break;
                    case "UPS":
                        var upsCount = dataSet.UPSEntries?.Count ?? 0;
                        dataSet.UPSEntries?.Clear();
                        Log.Information("Cleared {Count} UPS entries for replacement import", upsCount);
                        break;
                    case "Utility":
                        var utilCount = dataSet.UtilityEntries?.Count ?? 0;
                        dataSet.UtilityEntries?.Clear();
                        Log.Information("Cleared {Count} Utility entries for replacement import", utilCount);
                        break;
                    case "ZigzagTransformer":
                        var zzCount = dataSet.ZigzagTransformerEntries?.Count ?? 0;
                        dataSet.ZigzagTransformerEntries?.Clear();
                        Log.Information("Cleared {Count} ZigzagTransformer entries for replacement import", zzCount);
                        break;
                }
            }
        }

        #endregion
    }
}
