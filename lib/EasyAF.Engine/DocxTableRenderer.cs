using EasyAF.Data;

namespace EasyAF.Engine
{
    /// <summary>
    /// Renders table data into DOCX format using DocxTemplate.
    /// </summary>
    public class DocxTableRenderer : IDocTableRenderer
    {
        /// <summary>
        /// Renders evaluated table data into a DOCX template.
        /// </summary>
        /// <param name="tpl">The DOCX template to render into.</param>
        /// <param name="def">The table definition with formatting and column specs.</param>
        /// <param name="eval">The evaluated table data (rows, fills, colors, etc.).</param>
        public void Render(DocxTemplate tpl, TableDefinition def, TableEvalResult eval)
        {
            // Build formatting options from table definition
            var opts = new TableFormattingOptions
            {
                FitToWindow = true,
                RepeatHeaderRow = true,
                AlternateRowShading = true,
                HeaderBold = true,
                AddBorders = true,
                HeaderFillColor = "FFFFFF",
                LightWeight = false,
                AllowRowBreakAcrossPages = false,
                FontName = def.Formatting?.FontName,
                HorizontalAlignment = def.Formatting?.HorizontalAlignment,
                VerticalAlignment = def.Formatting?.VerticalAlignment,
                RemoveDuplicateLines = true,
                RemoveDuplicateLinesStrict = true,
                CellFills = eval.CellFills,
                CellTextColors = eval.CellTextColors
            };

            // Merge columns setup
            if (eval.MergeVerticalColumnIndexes != null && eval.MergeVerticalColumnIndexes.Length > 0)
            {
                opts.MergeBreakColumnIndexes = eval.MergeVerticalColumnIndexes.ToList();
            }

            // Render table
            tpl.ReplaceTableByAltText(def.AltText, eval.Rows, eval.MergeVerticalColumnIndexes, null, opts);
        }
    }
}
