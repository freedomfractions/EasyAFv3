using System;
using System.Collections.Generic;
using System.Linq;
using EasyAF.Data; // for TableFormattingOptions
using EasyAF.Data.Models;

namespace EasyAF.Engine
{
    public enum TableMode
    {
        New,
        Diff
    }

    public class ColumnDefinition
    {
        public string Header { get; set; }
        public string? Name { get; set; }
        public Func<ProjectContext, object?, string>? Renderer { get; set; }
        public bool MergeVertically { get; set; } = false;
        public List<CellConditionSpec>? Conditions { get; set; }
        // Expression metrics metadata retained
        public bool IsExpression { get; set; } = false;
        public string? ExpressionSource { get; set; }
        public string? ExpressionNumberFormat { get; set; }
        // (Removed prefetch optimization fields)
        public ColumnDefinition(string header) { Header = header; }
    }

    public class CellConditionSpec
    {
        public string PropertyPath { get; set; } = string.Empty;
        public string Operator { get; set; } = "eq";
        public string? Value { get; set; }
        public string? RightPropertyPath { get; set; }
        public bool Numeric { get; set; } = false;
        public bool IgnoreCase { get; set; } = true;
        public string Fill { get; set; } = "FFFF00";
        public string? TextColor { get; set; }
        public string Target { get; set; } = "Fill";
        public bool ApplyToRow { get; set; } = false;
        public bool MatchRendered { get; set; } = false;
    }

    public class RowConditionSpec
    {
        public string PropertyPath { get; set; } = string.Empty;
        public string Operator { get; set; } = "eq";
        public string? Value { get; set; }
        public string? RightPropertyPath { get; set; }
        public bool Numeric { get; set; } = false;
        public bool IgnoreCase { get; set; } = true;
        public string Fill { get; set; } = "FFC7CE";
        public string? TextColor { get; set; }
        public string Target { get; set; } = "Fill";
    }

    public class SortSpec
    {
        public int Column { get; set; }
        public string? Direction { get; set; }
        public bool Numeric { get; set; } = false;
    }

    public class FilterSpec
    {
        public string PropertyPath { get; set; } = string.Empty;
        public string Operator { get; set; } = "eq";
        public string? Value { get; set; }
        public bool Numeric { get; set; } = false;
        public bool IgnoreCase { get; set; } = false;
        public string? RightPropertyPath { get; set; }
    }

    public class FilterGroup
    {
        public string Logic { get; set; } = "AND";
        public List<FilterSpec> Filters { get; set; } = new();
    }

    public class TableDefinition
    {
        public string Id { get; set; }
        public string AltText { get; set; }
        public List<ColumnDefinition> Columns { get; set; } = new();
        public TableFormattingOptions? Formatting { get; set; }
        public List<SortSpec>? SortSpecs { get; set; }
        public List<FilterSpec>? FilterSpecs { get; set; }
        public List<FilterGroup>? FilterGroups { get; set; }
        /// <summary>
        /// Advanced filter logic expression (e.g., "(1 | 2) & 3").
        /// If null/empty/"AND"/"OR", uses simple logic. Otherwise evaluates via FilterLogicEvaluator.
        /// </summary>
        public string? FilterLogic { get; set; }
        public List<RowConditionSpec>? RowConditions { get; set; }
        public List<CellConditionSpec>? GlobalCellConditions { get; set; }
        public TableMode Mode { get; set; } = TableMode.New;
        public List<object>? LastSourceObjects { get; set; }
        public string? EmptyMessage { get; set; }
        public EmptyMessageFormattingSpec? EmptyFormatting { get; set; }
        public bool? HideIfNoDiff { get; set; } // new flag
        public TableDefinition(string id, string altText) { Id = id; AltText = altText; }
    }
}
