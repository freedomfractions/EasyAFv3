using System.Collections.Generic;

namespace EasyAF.Engine
{
    /// <summary>
    /// Result of evaluating a TableDefinition into a value matrix (no rendering concerns).
    /// Includes optional per-cell formatting metadata prepared by the evaluator.
    /// </summary>
    public class TableEvalResult
    {
        public string TableId { get; set; } = string.Empty;
        public List<string[]> Rows { get; set; } = new();
        public List<object> SourceObjects { get; set; } = new();
        public List<List<string?>>? CellFills { get; set; }
        public List<List<string?>>? CellTextColors { get; set; }
        public int[]? MergeVerticalColumnIndexes { get; set; }
    }
}
