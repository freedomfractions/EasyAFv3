using System;
using System.Collections.Generic;
using System.Linq;
using EasyAF.Modules.Map.Models;
using EasyAF.Core.Contracts;
using EasyAF.Core.Services; // NEW: For DataTypeSettingsExtensions
using Serilog;

namespace EasyAF.Modules.Map.Services
{
    /// <summary>
    /// Service for validating mapping completeness and quality.
    /// </summary>
    /// <remarks>
    /// <para>
    /// CROSS-MODULE EDIT: 2025-01-16 Required Property Validation
    /// Created for: Validate that all required properties are mapped before saving
    /// Related modules: Map (PropertyDiscoveryService, MapDocument, MapModule)
    /// Rollback instructions: Delete this file entirely
    /// </para>
    /// <para>
    /// This service provides validation logic for mapping configurations:
    /// - Required property validation (ensures critical properties are mapped)
    /// - Future: Data type compatibility validation
    /// - Future: Circular reference detection
    /// - Future: Missing column warnings
    /// </para>
    /// </remarks>
    public class MappingValidator
    {
        private readonly IPropertyDiscoveryService _propertyDiscovery;
        private readonly ISettingsService _settingsService;

        public MappingValidator(IPropertyDiscoveryService propertyDiscovery, ISettingsService settingsService)
        {
            _propertyDiscovery = propertyDiscovery ?? throw new ArgumentNullException(nameof(propertyDiscovery));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        }

        /// <summary>
        /// Validates that all required properties are mapped across all ENABLED data types.
        /// </summary>
        /// <param name="document">The map document to validate.</param>
        /// <returns>Validation result with details of any unmapped required properties.</returns>
        /// <remarks>
        /// CROSS-MODULE EDIT: 2025-01-16 Required Field Validation Filter Fix
        /// Modified for: Skip disabled data types during validation (e.g., Cable)
        /// Related modules: Core (ISettingsService), Map (MapSettingsExtensions)
        /// Rollback instructions: Remove IsDataTypeEnabled check and settingsService dependency
        /// 
        /// BUG FIX: Previously validated ALL data types regardless of enabled/disabled state.
        /// This caused errors like "Cable.Id required" even when Cable was disabled in settings.
        /// Now only validates data types that are actually enabled and visible to the user.
        /// </remarks>
        public ValidationResult ValidateRequiredMappings(MapDocument document)
        {
            var unmappedRequired = new Dictionary<string, List<string>>();

            try
            {
                // Get all available data types
                var dataTypes = _propertyDiscovery.GetAvailableDataTypes();

                foreach (var dataType in dataTypes)
                {
                    // SKIP disabled data types - they don't need validation
                    // If Cable is disabled in settings, we don't care if its required fields are mapped
                    if (!_settingsService.IsDataTypeEnabled(dataType))
                    {
                        Log.Debug("Skipping validation for disabled data type: {DataType}", dataType);
                        continue;
                    }

                    // Get all properties for this type (including required flag)
                    var allProperties = _propertyDiscovery.GetPropertiesForType(dataType);
                    var requiredProperties = allProperties
                        .Where(p => p.IsRequired)
                        .Select(p => p.PropertyName)
                        .ToList();

                    if (!requiredProperties.Any())
                    {
                        Log.Debug("No required properties for {DataType}", dataType);
                        continue;
                    }

                    // Get mapped property names for this type
                    var mappedPropertyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    if (document.MappingsByDataType.TryGetValue(dataType, out var mappings))
                    {
                        foreach (var mapping in mappings)
                        {
                            mappedPropertyNames.Add(mapping.PropertyName);
                        }
                    }

                    // Find required properties that are NOT mapped
                    var unmapped = requiredProperties
                        .Where(req => !mappedPropertyNames.Contains(req))
                        .ToList();

                    if (unmapped.Any())
                    {
                        unmappedRequired[dataType] = unmapped;
                        Log.Warning("Found {Count} unmapped required properties for {DataType}: {Properties}",
                            unmapped.Count, dataType, string.Join(", ", unmapped));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during required property validation");
                return new ValidationResult
                {
                    IsValid = false,
                    UnmappedRequired = unmappedRequired,
                    ErrorMessage = $"Validation error: {ex.Message}"
                };
            }

            var isValid = !unmappedRequired.Any();
            
            Log.Information("Required property validation complete: {Result} ({Count} data types with issues)",
                isValid ? "PASS" : "FAIL", unmappedRequired.Count);

            return new ValidationResult
            {
                IsValid = isValid,
                UnmappedRequired = unmappedRequired
            };
        }

        /// <summary>
        /// Gets a human-readable summary of validation failures.
        /// </summary>
        /// <param name="result">The validation result to summarize.</param>
        /// <returns>Formatted string describing the validation failures.</returns>
        public string GetValidationSummary(ValidationResult result)
        {
            if (result.IsValid)
            {
                return "All required properties are mapped. ?";
            }

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                return $"Validation error: {result.ErrorMessage}";
            }

            var summary = new System.Text.StringBuilder();
            summary.AppendLine("Required properties are not mapped:\n");

            foreach (var dataTypeMissing in result.UnmappedRequired.OrderBy(kvp => kvp.Key))
            {
                var dataType = dataTypeMissing.Key;
                var unmapped = dataTypeMissing.Value;

                summary.AppendLine($"{dataType}:");
                foreach (var propertyName in unmapped.OrderBy(p => p))
                {
                    summary.AppendLine($"  • {propertyName} *");
                }
                summary.AppendLine();
            }

            var totalUnmapped = result.UnmappedRequired.Sum(kvp => kvp.Value.Count);
            summary.AppendLine($"Total: {totalUnmapped} required property(ies) unmapped.");

            return summary.ToString();
        }
    }

    /// <summary>
    /// Result of a mapping validation operation.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Gets or sets whether the validation passed.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of unmapped required properties by data type.
        /// Key = data type name, Value = list of unmapped required property names.
        /// </summary>
        public Dictionary<string, List<string>> UnmappedRequired { get; set; } = new();

        /// <summary>
        /// Gets or sets an error message if validation itself failed.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}
