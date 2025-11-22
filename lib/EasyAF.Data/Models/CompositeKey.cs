using System;
using System.Linq;

namespace EasyAF.Data.Models
{
    /// <summary>
    /// Represents a generic composite key with N string components.
    /// Used for DataSet dictionaries to support variable-length keys discovered via reflection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Purpose:</strong> This class enables truly generic composite keys discovered via 
    /// [Required] attributes on model classes, eliminating hardcoded tuple sizes and property names.
    /// </para>
    /// <para>
    /// <strong>Design Philosophy:</strong>
    /// - Model classes define their key structure via [Required] attributes (source of truth)
    /// - CompositeKeyHelper discovers key components via reflection (discovery layer)
    /// - CompositeKey stores N components in ordered array (generic storage)
    /// - DataSet dictionaries use CompositeKey for all entry types (uniform interface)
    /// </para>
    /// <para>
    /// <strong>Key Features:</strong>
    /// - Supports 1 to N components (no hardcoded limits)
    /// - Proper equality semantics for dictionary usage
    /// - Efficient hash code generation
    /// - Immutable after construction
    /// - Order-preserving (component order matters)
    /// </para>
    /// <para>
    /// <strong>Example Usage:</strong>
    /// </para>
    /// <code>
    /// // 1-part key (LVBreaker, Bus, etc.)
    /// var key1 = new CompositeKey("LVCB-101");
    /// 
    /// // 2-part key (ArcFlash: BusName, Scenario)
    /// var key2 = new CompositeKey("BUS-1", "Main-Max");
    /// 
    /// // 3-part key (ShortCircuit: BusName, EquipmentName, Scenario)
    /// var key3 = new CompositeKey("BUS-1", "CB-101", "Main-Max");
    /// 
    /// // All can be used as dictionary keys
    /// Dictionary&lt;CompositeKey, ArcFlash&gt; arcFlash;
    /// Dictionary&lt;CompositeKey, ShortCircuit&gt; shortCircuit;
    /// Dictionary&lt;CompositeKey, LVBreaker&gt; breakers;
    /// </code>
    /// <para>
    /// <strong>Performance Characteristics:</strong>
    /// - Dictionary lookup: O(1) average case
    /// - Equality check: O(N) where N = component count (typically 1-3)
    /// - Hash code: Cached, computed once during construction
    /// - Memory: Fixed overhead + N string references
    /// </para>
    /// </remarks>
    public sealed class CompositeKey : IEquatable<CompositeKey>
    {
        /// <summary>
        /// The string components that make up this composite key, in order.
        /// Component order is significant and must match the discovery order
        /// from [Required] attributes (alphabetically sorted by property name).
        /// </summary>
        /// <remarks>
        /// This array is exposed as read-only. The underlying array is private
        /// and cannot be modified after construction, ensuring immutability.
        /// </remarks>
        public string[] Components { get; }

        /// <summary>
        /// Cached hash code computed during construction.
        /// Immutability allows safe caching for performance.
        /// </summary>
        private readonly int _hashCode;

        /// <summary>
        /// Initializes a new instance of CompositeKey with the specified components.
        /// </summary>
        /// <param name="components">
        /// The key components in order. Must contain at least one non-null, non-whitespace value.
        /// Component order is preserved and significant for equality comparisons.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if components is null, empty, or contains any null/whitespace values.
        /// </exception>
        /// <remarks>
        /// <para>
        /// <strong>Validation Rules:</strong>
        /// - At least one component required
        /// - No component can be null or whitespace
        /// - Components are stored as-is (no trimming or normalization)
        /// </para>
        /// <para>
        /// <strong>Example:</strong>
        /// </para>
        /// <code>
        /// // Valid
        /// new CompositeKey("BUS-1");                          // 1-part
        /// new CompositeKey("BUS-1", "Main-Max");              // 2-part
        /// new CompositeKey("BUS-1", "CB-101", "Main-Max");    // 3-part
        /// 
        /// // Invalid - throws ArgumentException
        /// new CompositeKey();                                 // No components
        /// new CompositeKey(null);                             // Null
        /// new CompositeKey("BUS-1", null);                    // Contains null
        /// new CompositeKey("BUS-1", "  ");                    // Contains whitespace
        /// </code>
        /// </remarks>
        public CompositeKey(params string[] components)
        {
            // Validate input
            if (components == null || components.Length == 0)
                throw new ArgumentException(
                    "Composite key must have at least one component. " +
                    "Check that all [Required] properties have values before creating the key.",
                    nameof(components));

            // Validate no null/whitespace components
            for (int i = 0; i < components.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(components[i]))
                {
                    var componentList = string.Join(", ", components.Select((c, idx) => idx + ":" + (c ?? "<null>")));
                    throw new ArgumentException(
                        $"Composite key component at index {i} is null or whitespace. " +
                        $"All [Required] properties must have values. " +
                        $"Key components: [{componentList}]",
                        nameof(components));
                }
            }

            // Store components (defensive copy to ensure immutability)
            Components = (string[])components.Clone();

            // Pre-compute and cache hash code (object is immutable)
            _hashCode = ComputeHashCode();
        }

        /// <summary>
        /// Determines whether this composite key equals another composite key.
        /// </summary>
        /// <param name="other">The other composite key to compare.</param>
        /// <returns>
        /// True if both keys have the same number of components and all components
        /// are equal using ordinal string comparison; otherwise false.
        /// </returns>
        /// <remarks>
        /// <para>
        /// <strong>Equality Rules:</strong>
        /// - Null keys are not equal to non-null keys
        /// - Component count must match
        /// - All components must match using StringComparison.Ordinal (case-sensitive, culture-invariant)
        /// - Component order is significant: ("A", "B") ? ("B", "A")
        /// </para>
        /// <para>
        /// <strong>Performance:</strong>
        /// - Short-circuits on reference equality (O(1))
        /// - Short-circuits on length mismatch (O(1))
        /// - Full comparison is O(N) where N = component count
        /// </para>
        /// </remarks>
        public bool Equals(CompositeKey? other)
        {
            // Null check
            if (other is null)
                return false;

            // Reference equality (same instance)
            if (ReferenceEquals(this, other))
                return true;

            // Hash code early exit (performance optimization)
            // If hash codes differ, keys cannot be equal
            if (_hashCode != other._hashCode)
                return false;

            // Component count must match
            if (Components.Length != other.Components.Length)
                return false;

            // Compare each component using ordinal comparison
            // Ordinal = case-sensitive, culture-invariant, fastest
            for (int i = 0; i < Components.Length; i++)
            {
                if (!string.Equals(Components[i], other.Components[i], StringComparison.Ordinal))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether this composite key equals another object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>
        /// True if obj is a CompositeKey and equals this key; otherwise false.
        /// </returns>
        public override bool Equals(object? obj) => Equals(obj as CompositeKey);

        /// <summary>
        /// Gets the hash code for this composite key.
        /// </summary>
        /// <returns>
        /// A hash code value combining all component hash codes.
        /// Identical keys produce identical hash codes.
        /// </returns>
        /// <remarks>
        /// <para>
        /// <strong>Implementation:</strong>
        /// Uses .NET's HashCode struct for efficient, high-quality hash combination.
        /// Hash code is computed once during construction and cached for performance.
        /// </para>
        /// <para>
        /// <strong>Properties:</strong>
        /// - Deterministic: Same components ? same hash code
        /// - Collision-resistant: Different components ? likely different hash codes
        /// - Efficient: O(N) computation, O(1) retrieval (cached)
        /// - Culture-invariant: Uses ordinal string comparison
        /// </para>
        /// </remarks>
        public override int GetHashCode() => _hashCode;

        /// <summary>
        /// Computes the hash code by combining all component hash codes.
        /// Called once during construction; result is cached in _hashCode field.
        /// </summary>
        private int ComputeHashCode()
        {
            var hash = new HashCode();
            foreach (var component in Components)
            {
                // Use ordinal comparer for culture-invariant, case-sensitive hashing
                hash.Add(component, StringComparer.Ordinal);
            }
            return hash.ToHashCode();
        }

        /// <summary>
        /// Returns a string representation of this composite key.
        /// </summary>
        /// <returns>
        /// A string in the format "(component1, component2, ...)" for debugging and logging.
        /// </returns>
        /// <remarks>
        /// <para>
        /// <strong>Examples:</strong>
        /// </para>
        /// <code>
        /// new CompositeKey("BUS-1").ToString()                    // "(BUS-1)"
        /// new CompositeKey("BUS-1", "Main-Max").ToString()        // "(BUS-1, Main-Max)"
        /// new CompositeKey("BUS-1", "CB-101", "Main-Max").ToString()  // "(BUS-1, CB-101, Main-Max)"
        /// </code>
        /// <para>
        /// This format matches tuple syntax for familiarity and is useful for
        /// logging, debugging, and error messages.
        /// </para>
        /// </remarks>
        public override string ToString() => $"({string.Join(", ", Components)})";

        /// <summary>
        /// Equality operator for natural comparison syntax.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if both keys are equal or both are null; otherwise false.</returns>
        public static bool operator ==(CompositeKey? left, CompositeKey? right)
        {
            if (left is null)
                return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator for natural comparison syntax.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if keys are not equal; otherwise false.</returns>
        public static bool operator !=(CompositeKey? left, CompositeKey? right) => !(left == right);

        /// <summary>
        /// Creates a CompositeKey from a 2-component tuple for backward compatibility.
        /// </summary>
        /// <param name="item1">First component.</param>
        /// <param name="item2">Second component.</param>
        /// <returns>A CompositeKey with two components.</returns>
        /// <remarks>
        /// Helper method to ease migration from tuple-based keys.
        /// Prefer using the constructor directly for new code.
        /// </remarks>
        public static CompositeKey FromTuple(string item1, string item2) => new(item1, item2);

        /// <summary>
        /// Creates a CompositeKey from a 3-component tuple for backward compatibility.
        /// </summary>
        /// <param name="item1">First component.</param>
        /// <param name="item2">Second component.</param>
        /// <param name="item3">Third component.</param>
        /// <returns>A CompositeKey with three components.</returns>
        /// <remarks>
        /// Helper method to ease migration from tuple-based keys.
        /// Prefer using the constructor directly for new code.
        /// </remarks>
        public static CompositeKey FromTuple(string item1, string item2, string item3) => new(item1, item2, item3);

        /// <summary>
        /// Gets a single-component key value (for keys with exactly one component).
        /// </summary>
        /// <returns>The single component value.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the key has more than one component.
        /// </exception>
        /// <remarks>
        /// Convenience method for accessing single-component keys without array indexing.
        /// Useful for data types with simple string keys (LVBreaker, Bus, etc.).
        /// </remarks>
        public string AsSingleValue()
        {
            if (Components.Length != 1)
                throw new InvalidOperationException(
                    $"Cannot get single value from composite key with {Components.Length} components. " +
                    $"Key: {this}");
            return Components[0];
        }

        /// <summary>
        /// Gets a description of this key's structure for debugging.
        /// </summary>
        /// <returns>A string like "3-part key: (BUS-1, CB-101, Main-Max)"</returns>
        public string GetDescription() => $"{Components.Length}-part key: {this}";
    }
}
