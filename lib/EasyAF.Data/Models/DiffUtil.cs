using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;

namespace EasyAF.Data.Models
{
    /// <summary>
    /// Provides reflection-based utilities for comparing Plain Old CLR Objects (POCOs) and detecting property-level changes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Purpose:</strong> DiffUtil enables automatic comparison of data model instances by examining
    /// their public properties via reflection. It produces a list of <see cref="PropertyChange"/> objects
    /// describing what changed between two versions of an object.
    /// </para>
    /// <para>
    /// <strong>Comparison Strategy:</strong>
    /// </para>
    /// <list type="number">
    /// <item><description>Enumerate all public, readable instance properties</description></item>
    /// <item><description>Skip properties marked with <see cref="ObsoleteAttribute"/></description></item>
    /// <item><description>Compare property values between old and new objects</description></item>
    /// <item><description>Attempt numeric comparison for values that look like numbers (with tolerance)</description></item>
    /// <item><description>Fall back to string comparison if numeric parsing fails</description></item>
    /// <item><description>Generate PropertyChange entries for differences</description></item>
    /// </list>
    /// <para>
    /// <strong>Numeric Comparison:</strong> Values are automatically normalized and compared as floating-point
    /// numbers when possible. This handles cases like "10.0 kA" vs "10 kA" or "8.50" vs "8.5" as equal.
    /// A relative tolerance of 1e-6 is used to handle floating-point precision issues.
    /// </para>
    /// <para>
    /// <strong>Unit Stripping:</strong> Common electrical units (kA, A, cal/cm², etc.) are automatically
    /// stripped before numeric comparison to avoid false differences.
    /// </para>
    /// <para>
    /// <strong>Limitations:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Only compares top-level properties; does not recurse into nested objects</description></item>
    /// <item><description>Collections/arrays are compared by ToString() (usually reference identity)</description></item>
    /// <item><description>For complex objects with nested structures, implement custom Diff() methods (see <see cref="LVCB"/> and <see cref="TripUnit"/>)</description></item>
    /// <item><description>Uses reflection; performance may be a concern for very large datasets</description></item>
    /// </list>
    /// <para>
    /// <strong>Thread Safety:</strong> All methods are thread-safe (stateless static class).
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Reflection overhead is acceptable for typical diff operations
    /// (100-1000 objects). For larger datasets or real-time scenarios, consider:
    /// - Implementing custom comparison logic
    /// - Using compiled expressions (see ExpressionCompiler pattern)
    /// - Running diff operations on background threads
    /// </para>
    /// </remarks>
    /// <example>
    /// <para><strong>Example 1: Basic object comparison</strong></para>
    /// <code>
    /// var oldBus = new Bus { Id = "BUS-001", Name = "Main", Voltage = "480V" };
    /// var newBus = new Bus { Id = "BUS-001", Name = "Main Switchgear", Voltage = "480V" };
    /// 
    /// var changes = DiffUtil.DiffObjects(oldBus, newBus);
    /// 
    /// // Changes will contain:
    /// // - PropertyChange { PropertyPath = "Name", OldValue = "Main", NewValue = "Main Switchgear", ChangeType = Modified }
    /// // (Voltage unchanged, so not included)
    /// 
    /// foreach (var change in changes)
    /// {
    ///     Console.WriteLine($"{change.PropertyPath}: {change.OldValue} -> {change.NewValue}");
    /// }
    /// </code>
    /// 
    /// <para><strong>Example 2: Handling null objects (additions/removals)</strong></para>
    /// <code>
    /// var oldFuse = new Fuse { Id = "F-001", Rating = "100A" };
    /// 
    /// // Compare existing object to null (represents removal)
    /// var removalChanges = DiffUtil.DiffObjects(oldFuse, null);
    /// // All properties marked as Removed
    /// 
    /// // Compare null to new object (represents addition)
    /// var newFuse = new Fuse { Id = "F-002", Rating = "200A" };
    /// var additionChanges = DiffUtil.DiffObjects&lt;Fuse&gt;(null, newFuse);
    /// // All properties marked as Added
    /// </code>
    /// 
    /// <para><strong>Example 3: Numeric comparison with units</strong></para>
    /// <code>
    /// var old = new ShortCircuit { DutyKA = "10.0 kA" };
    /// var @new = new ShortCircuit { DutyKA = "10 kA" };
    /// 
    /// var changes = DiffUtil.DiffObjects(old, @new);
    /// 
    /// // No changes detected - "10.0 kA" and "10 kA" are numerically equal
    /// // Units are stripped, values parsed as 10.0 == 10.0
    /// Console.WriteLine($"Changes: {changes.Count}"); // 0
    /// </code>
    /// 
    /// <para><strong>Example 4: Using prefix for nested property paths</strong></para>
    /// <code>
    /// var oldTrip = new TripUnit { LongTimePickup = "800" };
    /// var newTrip = new TripUnit { LongTimePickup = "1000" };
    /// 
    /// // Add prefix to indicate nested path
    /// var changes = DiffUtil.DiffObjects(oldTrip, newTrip, "TripUnit");
    /// 
    /// // PropertyChange will have PropertyPath = "TripUnit.LongTimePickup"
    /// // This is used by DataSet.Diff() for LVCB.TripUnit changes
    /// </code>
    /// </example>
    public static class DiffUtil
    {
        /// <summary>
        /// Compares two objects of the same type using public readable properties, producing a list of detected changes.
        /// </summary>
        /// <typeparam name="T">The type of objects to compare. Must be a reference type (class).</typeparam>
        /// <param name="oldObj">
        /// The original/old version of the object. If null, all properties of <paramref name="newObj"/> are treated as additions.
        /// </param>
        /// <param name="newObj">
        /// The updated/new version of the object. If null, all properties of <paramref name="oldObj"/> are treated as removals.
        /// </param>
        /// <param name="prefix">
        /// Optional prefix to prepend to property paths in the results (e.g., "TripUnit" produces "TripUnit.PropertyName").
        /// Useful for nested object comparisons.
        /// </param>
        /// <returns>
        /// A list of <see cref="PropertyChange"/> objects describing the differences.
        /// Empty list if objects are identical or both null.
        /// </returns>
        public static List<PropertyChange> DiffObjects<T>(T? oldObj, T? newObj, string prefix = "") where T : class
        {
            var changes = new List<PropertyChange>();
            
            if (oldObj == null && newObj == null) 
                return changes;

            if (oldObj == null)
            {
                foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (!prop.CanRead || prop.GetIndexParameters().Length > 0) 
                        continue;
                    if (Attribute.IsDefined(prop, typeof(ObsoleteAttribute))) 
                        continue;
                    var newVal = prop.GetValue(newObj)?.ToString();
                    changes.Add(new PropertyChange
                    {
                        PropertyPath = Join(prefix, prop.Name),
                        OldValue = null,
                        NewValue = newVal,
                        ChangeType = ChangeType.Added
                    });
                }
                return changes;
            }

            if (newObj == null)
            {
                foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (!prop.CanRead || prop.GetIndexParameters().Length > 0) 
                        continue;
                    if (Attribute.IsDefined(prop, typeof(ObsoleteAttribute))) 
                        continue;
                    var oldVal = prop.GetValue(oldObj)?.ToString();
                    changes.Add(new PropertyChange
                    {
                        PropertyPath = Join(prefix, prop.Name),
                        OldValue = oldVal,
                        NewValue = null,
                        ChangeType = ChangeType.Removed
                    });
                }
                return changes;
            }

            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanRead || prop.GetIndexParameters().Length > 0) 
                    continue;
                if (Attribute.IsDefined(prop, typeof(ObsoleteAttribute))) 
                    continue;

                var oldVal = prop.GetValue(oldObj);
                var newVal = prop.GetValue(newObj);

                var oldStr = oldVal?.ToString();
                var newStr = newVal?.ToString();

                if (string.Equals(oldStr, newStr, StringComparison.Ordinal)) 
                    continue;

                if (TryCompareAsDouble(oldStr, newStr, out bool equal))
                {
                    if (equal) 
                        continue;
                }

                changes.Add(new PropertyChange
                {
                    PropertyPath = Join(prefix, prop.Name),
                    OldValue = oldStr,
                    NewValue = newStr,
                    ChangeType = ChangeType.Modified
                });
            }

            return changes;
        }

        private static string Join(string prefix, string propName)
        {
            return string.IsNullOrEmpty(prefix) ? propName : prefix + "." + propName;
        }

        private static bool TryCompareAsDouble(string? s1, string? s2, out bool equal)
        {
            equal = false;
            
            if (string.IsNullOrWhiteSpace(s1) && string.IsNullOrWhiteSpace(s2))
            {
                equal = true;
                return true;
            }

            if (double.TryParse(NormalizeNumberString(s1), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var d1) &&
                double.TryParse(NormalizeNumberString(s2), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var d2))
            {
                var tol = 1e-6;
                var maxMagnitude = Math.Max(1.0, Math.Max(Math.Abs(d1), Math.Abs(d2)));
                
                if (Math.Abs(d1 - d2) <= tol * maxMagnitude)
                {
                    equal = true;
                }
                return true;
            }

            return false;
        }

        private static string? NormalizeNumberString(string? s)
        {
            if (s == null) 
                return null;
                
            var cleaned = s.Trim();
            
            cleaned = cleaned
                .Replace("cal/cm^2", "", StringComparison.OrdinalIgnoreCase)
                .Replace("cal/cm2", "", StringComparison.OrdinalIgnoreCase)
                .Replace("kA", "", StringComparison.OrdinalIgnoreCase)
                .Replace("A", "", StringComparison.OrdinalIgnoreCase);
                
            return cleaned.Trim();
        }
    }
}
