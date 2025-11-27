using System;
using System.Collections.Generic;
using System.Linq;
using EasyAF.Modules.Map.Models;
using Serilog;
using MapPropertyInfo = EasyAF.Modules.Map.Models.PropertyInfo;

namespace EasyAF.Modules.Map.ViewModels
{
    /// <summary>
    /// Partial class containing auto-map intelligence algorithm.
    /// </summary>
    public partial class DataTypeMappingViewModel
    {
        #region Auto-Map Algorithm

        /// <summary>
        /// Executes the auto-map command using intelligent fuzzy matching.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Auto-Map Algorithm:
        /// 1. Get all unmapped properties (target properties with no mapping)
        /// 2. Get all unmapped columns (source columns with no mapping)
        /// 3. FOR EACH unmapped property:
        ///    a) STRATEGY 1: Match against PropertyName (fuzzy)
        ///    b) STRATEGY 2: Match against Description/friendly text (fuzzy)
        ///    c) If score >= 0.6, create mapping
        /// 4. SPECIAL ID HANDLING (runs AFTER regular matching):
        ///    a) If ID property is still unmapped, use first column (most reliable)
        ///    b) FALLBACK: Try explicit ID column names ("ID", "ID Name", etc.)
        /// 5. Update UI to reflect new mappings (no dialog - badges show confidence)
        /// </para>
        /// </remarks>
        private void ExecuteAutoMap()
        {
            try
            {
                if (SourceColumns.Count == 0)
                {
                    _dialogService.ShowMessage(
                        "No source columns available.\n\n" +
                        "Please select a table from the dropdown first.",
                        "Auto-Map");
                    Log.Information("Auto-Map: No source columns available for {DataType}", _dataType);
                    return;
                }

                Log.Information("Starting Auto-Map for {DataType} with {ColumnCount} columns and {PropertyCount} properties",
                    _dataType, SourceColumns.Count, TargetProperties.Count);

                var unmappedProperties = TargetProperties.Where(p => !p.IsMapped).ToList();
                var unmappedColumnNames = SourceColumns.Where(c => !c.IsMapped).Select(c => c.ColumnName).ToList();

                if (unmappedProperties.Count == 0)
                {
                    _dialogService.ShowMessage("All properties are already mapped!", "Auto-Map");
                    Log.Information("Auto-Map: All properties already mapped for {DataType}", _dataType);
                    return;
                }

                var successfulMappings = new List<(string Property, string Column, double Score, string Strategy)>();
                var lowConfidenceMappings = new List<(string Property, string Column, double Score)>();
                var noMatchProperties = new List<string>();

                const double confidenceThreshold = 0.6;
                var stylishKeywords = new[] { "style", "type", "category", "class", "kind", "mode" };

                // PHASE 1: Regular fuzzy matching for all properties
                foreach (var property in unmappedProperties)
                {
                    var matches = _fuzzyMatcher.FindBestMatches(
                        query: property.PropertyName,
                        candidates: unmappedColumnNames,
                        maxResults: 1,
                        minScore: 0.4,
                        caseSensitive: false);

                    // Also try matching against Description (friendly text) if available
                    if (!string.IsNullOrWhiteSpace(property.Description))
                    {
                        var descriptionMatches = _fuzzyMatcher.FindBestMatches(
                            query: property.Description,
                            candidates: unmappedColumnNames,
                            maxResults: 1,
                            minScore: 0.4,
                            caseSensitive: false);

                        if (descriptionMatches.Any() && 
                            (!matches.Any() || descriptionMatches[0].Score > matches[0].Score))
                        {
                            matches = descriptionMatches;
                            Log.Debug("Auto-Map: Using Description match for '{Property}' - score {Score:P0}",
                                property.PropertyName, descriptionMatches[0].Score);
                        }
                    }

                    if (!matches.Any())
                    {
                        noMatchProperties.Add(property.PropertyName);
                        Log.Debug("Auto-Map: No match found for property '{Property}'", property.PropertyName);
                        continue;
                    }

                    var bestMatch = matches[0];
                    
                    // Semantic filtering: Detect ID/name properties being matched to style/type columns
                    bool isLikelyIdProperty = property.IsRequired ||
                                              property.PropertyName.Equals("Id", StringComparison.OrdinalIgnoreCase) ||
                                              property.PropertyName.Equals(_dataType, StringComparison.OrdinalIgnoreCase) ||
                                              property.PropertyName.Equals(_dataType + "s", StringComparison.OrdinalIgnoreCase);

                    bool columnLooksLikeStyleOrType = stylishKeywords.Any(keyword => 
                        bestMatch.Target.Contains(keyword, StringComparison.OrdinalIgnoreCase));
                    
                    double effectiveThreshold = confidenceThreshold;
                    
                    if (isLikelyIdProperty && columnLooksLikeStyleOrType)
                    {
                        effectiveThreshold = 0.85;
                        Log.Debug("Auto-Map: Semantic filter triggered for '{Property}' ? '{Column}' (raising threshold to 85%)",
                            property.PropertyName, bestMatch.Target);
                    }

                    if (bestMatch.Score >= effectiveThreshold)
                    {
                        var sourceColumn = SourceColumns.FirstOrDefault(c => c.ColumnName == bestMatch.Target);
                        if (sourceColumn != null)
                        {
                            CreateMapping(property, sourceColumn, bestMatch.Score);
                            successfulMappings.Add((property.PropertyName, bestMatch.Target, bestMatch.Score, "Fuzzy Match"));
                            unmappedColumnNames.Remove(bestMatch.Target);

                            Log.Information("Auto-Map: Mapped '{Property}' ? '{Column}' (score: {Score:P0})",
                                property.PropertyName, bestMatch.Target, bestMatch.Score);
                        }
                    }
                    else
                    {
                        lowConfidenceMappings.Add((property.PropertyName, bestMatch.Target, bestMatch.Score));
                        Log.Debug("Auto-Map: Low confidence match for '{Property}' ? '{Column}' (score: {Score:P0})",
                            property.PropertyName, bestMatch.Target, bestMatch.Score);
                    }
                }

                // PHASE 2: Special ID property handling
                var idProperty = TargetProperties.FirstOrDefault(p => 
                    !p.IsMapped && 
                    (p.PropertyName.Equals("Id", StringComparison.OrdinalIgnoreCase) ||
                     p.PropertyName.Equals("ID", StringComparison.OrdinalIgnoreCase) ||
                     p.PropertyName.Equals("Identifier", StringComparison.OrdinalIgnoreCase) ||
                     p.IsRequired));

                if (idProperty != null)
                {
                    Log.Debug("Auto-Map: Found unmapped ID/Required property '{Property}' - attempting smart ID mapping", idProperty.PropertyName);
                    
                    // STRATEGY 1: Use first column (by ColumnIndex) if unmapped - MOST RELIABLE
                    var firstColumn = SourceColumns
                        .Where(c => !c.IsMapped)
                        .OrderBy(c => c.ColumnIndex)
                        .FirstOrDefault();
                    
                    if (firstColumn != null)
                    {
                        CreateMapping(idProperty, firstColumn, 1.0);
                        successfulMappings.Add((idProperty.PropertyName, firstColumn.ColumnName, 1.0, "First Column (ID)"));
                        unmappedColumnNames.Remove(firstColumn.ColumnName);
                        
                        Log.Information("Auto-Map: Mapped ID property '{Property}' ? '{Column}' (first column - highest confidence)", 
                            idProperty.PropertyName, firstColumn.ColumnName);
                    }
                    else
                    {
                        // STRATEGY 2: Try explicit ID column matches if first column is already mapped
                        var idColumnNames = new[] { "ID", "Id", "id", "ID Name", "Id Name", "Identifier", "UniqueID" };
                        var idColumn = SourceColumns.FirstOrDefault(c => 
                            !c.IsMapped && 
                            idColumnNames.Contains(c.ColumnName, StringComparer.OrdinalIgnoreCase));

                        if (idColumn != null)
                        {
                            CreateMapping(idProperty, idColumn, 0.95);
                            successfulMappings.Add((idProperty.PropertyName, idColumn.ColumnName, 0.95, "Explicit ID Column"));
                            unmappedColumnNames.Remove(idColumn.ColumnName);
                            
                            Log.Information("Auto-Map: Mapped ID property '{Property}' ? '{Column}' (explicit ID column)", 
                                idProperty.PropertyName, idColumn.ColumnName);
                        }
                        else
                        {
                            Log.Debug("Auto-Map: ID property '{Property}' remains unmapped", idProperty.PropertyName);
                        }
                    }
                }

                Log.Information("Auto-Map complete for {DataType}: {Success} mapped, {LowConf} low confidence, {NoMatch} no match",
                    _dataType, successfulMappings.Count, lowConfidenceMappings.Count, noMatchProperties.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during Auto-Map for {DataType}", _dataType);
                _dialogService.ShowMessage($"An error occurred during Auto-Map:\n\n{ex.Message}", "Auto-Map Error");
            }
        }

        #endregion
    }
}
