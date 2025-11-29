using System;
using System.Collections.Generic;

namespace EasyAF.Engine
{
    /// <summary>
    /// Describes a single column inside a table specification. A column can source its value
    /// from one or more property paths, a literal, or a composite <see cref="Format"/> string.
    /// Optionally an <see cref="Expression"/> can be evaluated for computed numeric values.
    /// </summary>
    public class ColumnSpec
    {
        /// <summary>Header text as it appears in the template table.</summary>
        public string Header { get; set; } = string.Empty;
        /// <summary>Optional internal name (not currently required).</summary>
        public string? Name { get; set; }
        /// <summary>One or more property paths (relative to the row object) whose values will be rendered or joined.</summary>
        public string[]? PropertyPaths { get; set; }
        /// <summary>Literal text to render (overrides <see cref="PropertyPaths"/> and <see cref="Format"/> when supplied).</summary>
        public string? Literal { get; set; }
        /// <summary>Composite format string with tokens like {TripUnit.Ltpu}. Ignored if <see cref="Literal"/> is set.</summary>
        public string? Format { get; set; }
        /// <summary>Separator used when joining multiple <see cref="PropertyPaths"/> (defaults to newline).</summary>
        public string? JoinWith { get; set; } = "\n";
        /// <summary>If true, identical vertically adjacent cells will be merged (row-spanning effect).</summary>
        public bool MergeVertically { get; set; } = false;
        /// <summary>Desired width percentage (0-100). When any column specifies a width, unspecified columns divide remaining space.</summary>
        public double? WidthPercent { get; set; }
        /// <summary>Optional numeric expression using property tokens (e.g. <c>({RatingKA}-{DutyKA})/{RatingKA}*100</c>).</summary>
        public string? Expression { get; set; }
        /// <summary>Optional .NET numeric format string applied when <see cref="Expression"/> (or numeric value) is used.</summary>
        public string? NumberFormat { get; set; }
        /// <summary>Conditional formatting rules applied to the produced cell value.</summary>
        public CellConditionSpec[]? Conditions { get; set; }
        /// <summary>Per-column override for font name.</summary>
        public string? FontName { get; set; }
        /// <summary>Per-column override for font size (points).</summary>
        public double? FontSize { get; set; } // points
        /// <summary>Per-column horizontal alignment override (e.g. left, center, right).</summary>
        public string? HorizontalAlignment { get; set; }
        /// <summary>Per-column vertical alignment override (e.g. top, center, bottom).</summary>
        public string? VerticalAlignment { get; set; }
    }

    /// <summary>Represents a logical group of simple filters (future use; currently unused).</summary>
    public class FilterGroupSpec
    {
        /// <summary>Logical operator combining filters: AND / OR.</summary>
        public string Logic { get; set; } = "AND";
        /// <summary>Contained filters.</summary>
        public FilterSpec[] Filters { get; set; } = Array.Empty<FilterSpec>();
    }

    /// <summary>Global formatting defaults for a table.</summary>
    public class TableFormattingSpec
    {
        public string? FontName { get; set; }
        public double? FontSize { get; set; } // points
        public string? HorizontalAlignment { get; set; }
        public string? VerticalAlignment { get; set; }
        /// <summary>Header row fill color (hex RGB w/o #). Applies when present.</summary>
        public string? HeaderFill { get; set; }
        /// <summary>Alternate (band) row fill color (hex RGB w/o #).</summary>
        public string? AlternateRowFill { get; set; }
        /// <summary>When true, duplicate lines within a single cell are removed during rendering.</summary>
        public bool? RemoveDuplicateLines { get; set; }
        /// <summary>When true, duplicate detection is case-insensitive and trims whitespace.</summary>
        public bool? RemoveDuplicateLinesStrict { get; set; }
        /// <summary>
        /// When true (default), the rendered table is set to100% page width. When false, the template’s original width is preserved.
        /// </summary>
        public bool? FitToWindow { get; set; }
        /// <summary>Optional list of1-based column indexes that define merge break boundaries.</summary>
        public int[]? MergeBreakColumns { get; set; }
        /// <summary>Optional list of column headers (case-insensitive) that define merge break boundaries.</summary>
        public string[]? MergeBreakColumnHeaders { get; set; }
    }

    /// <summary>Formatting to apply to an empty table placeholder message.</summary>
    public class EmptyMessageFormattingSpec
    {
        public string? FontName { get; set; }
        public double? FontSize { get; set; } // points
        public string? HorizontalAlignment { get; set; }
        public string? VerticalAlignment { get; set; }
        public bool? Bold { get; set; }
        /// <summary>Cell background color (hex RGB w/o #).</summary>
        public string? Fill { get; set; }
        /// <summary>Text color (hex RGB w/o #).</summary>
        public string? TextColor { get; set; }
    }

    /// <summary>Specification for a single logical table rendered into the DOCX template.</summary>
    public class TableSpec
    {
        /// <summary>Unique identifier (used internally; not shown to users).</summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>Alternate text / description matching the table placeholder AltText in the template.</summary>
        public string AltText { get; set; } = string.Empty;
        /// <summary>If false rows are marked with cantSplit to avoid page breaks within a row.</summary>
        public bool? AllowRowBreakAcrossPages { get; set; }
        /// <summary>Optional set of sort directives applied in order.</summary>
        public SortSpec[]? SortSpecs { get; set; }
        /// <summary>Simple independent filters (logical AND by default in engine).</summary>
        public FilterSpec[]? FilterSpecs { get; set; }
        /// <summary>Advanced grouped filters (future use).</summary>
        public FilterGroupSpec[]? FilterGroups { get; set; }
        /// <summary>
        /// Advanced filter logic expression (e.g., "(1 | 2) & 3" or "!1 & 2").
        /// If null, empty, "AND", or "OR", uses simple logic.
        /// If a complex expression, it's parsed and evaluated using FilterLogicEvaluator.
        /// Numbers represent 1-based filter indices from FilterSpecs array.
        /// Operators: & (AND), | (OR), ! (NOT), ( ) (grouping).
        /// </summary>
        public string? FilterLogic { get; set; }
        /// <summary>Row-level conditional formatting rules.</summary>
        public RowConditionSpec[]? RowConditions { get; set; }
        /// <summary>Global cell conditions applied after column-specific conditions.</summary>
        public CellConditionSpec[]? GlobalCellConditions { get; set; }
        /// <summary>Ordered list of column specifications.</summary>
        public ColumnSpec[] Columns { get; set; } = Array.Empty<ColumnSpec>();
        /// <summary>Mode indicator: "new" or "diff" (affects diff-aware rendering).</summary>
        public string? Mode { get; set; }
        /// <summary>Default formatting for the table body (may be overridden per column).</summary>
        public TableFormattingSpec? Formatting { get; set; }
        /// <summary>Message inserted when the table yields zero data rows.</summary>
        public string? EmptyMessage { get; set; }
        /// <summary>Formatting applied exclusively to the empty message placeholder row.</summary>
        public EmptyMessageFormattingSpec? EmptyFormatting { get; set; }
        /// <summary>If true (and Mode="diff") the table is rendered as empty when no cell contains a diff marker.</summary>
        public bool? HideIfNoDiff { get; set; }
    }

    /// <summary>Maps a project-level property into a Word core or custom document property.</summary>
    public class PropertyMappingSpec
    {
        /// <summary>Name of the project property (in Project.Properties or a legacy scalar property).</summary>
        public string ProjectProperty { get; set; } = string.Empty;
        /// <summary>Target document property name (custom or built-in depending on <see cref="BuiltIn"/>).</summary>
        public string DocProperty { get; set; } = string.Empty;
        /// <summary>If true attempt built-in (core package) property first, else always create/update custom property.</summary>
        public bool BuiltIn { get; set; } = false;
        /// <summary>If true and the project property is missing/blank an error will be logged (and mapping skipped).</summary>
        public bool Required { get; set; } = false;
        /// <summary>Optional default value applied when the project property is missing or blank and not Required.</summary>
        public string? DefaultValue { get; set; }
    }

    /// <summary>
    /// Root object of a report spec file. Allows bundling of <see cref="PropertyMappings"/> with table specs.
    /// </summary>
    public class SpecFileRoot
    {
        /// <summary>Semantic version or simple incrementing version for the spec (e.g. 1.0.0).</summary>
        public string? SpecVersion { get; set; }
        /// <summary>Optional checksum (e.g. SHA256 hex) of the canonical Tables array for integrity validation.</summary>
        public string? SpecChecksum { get; set; }
        /// <summary>Table specifications (each referencing a placeholder table in the template by AltText).</summary>
        public TableSpec[] Tables { get; set; } = Array.Empty<TableSpec>();
        /// <summary>Optional property mapping definitions for project -> DOCX properties.</summary>
        public PropertyMappingSpec[]? PropertyMappings { get; set; }
        // TODO: Add future MapCompatibility metadata if required.
    }

    // Note: SortSpec / FilterSpec / RowConditionSpec / CellConditionSpec live in TableDefinition.cs
}
