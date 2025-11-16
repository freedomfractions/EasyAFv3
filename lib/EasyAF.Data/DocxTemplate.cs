using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Table = DocumentFormat.OpenXml.Wordprocessing.Table;

namespace EasyAF.Data
{
    public class TableFormattingOptions
    {
        public bool FitToWindow { get; set; } = true;
        public bool RepeatHeaderRow { get; set; } = true;
        public bool AlternateRowShading { get; set; } = true;
        public string? HeaderStyle { get; set; }
        public int FontSizeHalfPoints { get; set; } = 24; // 12pt
        public bool HeaderBold { get; set; } = true;
        public bool AddBorders { get; set; } = true;
        public string HeaderFillColor { get; set; } = "FFFFFF"; // default white per-table
        public bool LightWeight { get; set; } = false;
        public bool AllowRowBreakAcrossPages { get; set; } = false;
        public List<double>? ColumnWidthPercents { get; set; }
        public List<List<string?>>? CellFills { get; set; }
        public List<List<string?>>? CellTextColors { get; set; }
        public string? FontName { get; set; }
        public string? HorizontalAlignment { get; set; }
        public string? VerticalAlignment { get; set; }
        // New: per-column override lists (null or value per column)
        public List<string?>? ColumnFontNames { get; set; }
        public List<int?>? ColumnFontSizeHalfPoints { get; set; }
        public List<string?>? ColumnHorizontalAlignments { get; set; }
        public List<string?>? ColumnVerticalAlignments { get; set; }
        // New: configurable alternate row fill color (banding)
        public string? AlternateRowFillColor { get; set; } = "C1E4F5"; // default light blue
        // New: remove duplicate lines within a single cell (e.g., repeated Style/Type)
        public bool RemoveDuplicateLines { get; set; } = true;
        // When strict, comparison is case-insensitive and uses trimmed values
        public bool RemoveDuplicateLinesStrict { get; set; } = true;
        // New: Optional merge group break rules: when any of these columns changes, vertical merges cannot span across the boundary
        public List<int>? MergeBreakColumnIndexes { get; set; }
        public List<string>? MergeBreakColumnNames { get; set; }
    }

    public class DocxTemplate : IDisposable
    {
        private readonly WordprocessingDocument _doc;
        private readonly MainDocumentPart _mainPart;
        private readonly string _filePath;

        private DocxTemplate(WordprocessingDocument doc, string filePath)
        {
            _doc = doc;
            _mainPart = doc.MainDocumentPart!;
            _filePath = filePath;
        }

        public static DocxTemplate OpenReadWrite(string path)
        {
            var doc = WordprocessingDocument.Open(path, true);
            return new DocxTemplate(doc, path);
        }

        public static void CreateBasicTemplate(string path, string tableAltText, IEnumerable<string> headers)
        {
            using var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
            var main = doc.AddMainDocumentPart();
            main.Document = new Document(new Body());

            var tbl = new Table();
            var props = new TableProperties();
            props.Append(new TableCaption { Val = tableAltText });
            props.Append(new TableDescription { Val = tableAltText });
            tbl.Append(props);

            var headerRow = new TableRow();
            foreach (var h in headers)
            {
                var tc = new TableCell(new Paragraph(new Run(new Text(h ?? string.Empty) { Space = SpaceProcessingModeValues.Preserve })));
                headerRow.Append(tc);
            }
            var trPr = new TableRowProperties(); trPr.Append(new TableHeader()); headerRow.PrependChild(trPr);
            tbl.Append(headerRow);
            tbl.Append(new TableRow(new TableCell(new Paragraph(new Run(new Text(string.Empty))))));
            main.Document.Body!.Append(tbl);
            main.Document.Save();
        }

        public static void CreateMultiTableTemplate(string path, IEnumerable<(string AltText, IEnumerable<string> Headers)> tables)
        {
            using var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
            var main = doc.AddMainDocumentPart();
            main.Document = new Document(new Body());
            var body = main.Document.Body!;

            foreach (var (alt, headers) in tables)
            {
                var tbl = new Table();
                var props = new TableProperties();
                props.Append(new TableCaption { Val = alt });
                props.Append(new TableDescription { Val = alt });
                tbl.Append(props);

                var headerRow = new TableRow();
                foreach (var h in headers)
                {
                    var tc = new TableCell(new Paragraph(new Run(new Text(h ?? string.Empty) { Space = SpaceProcessingModeValues.Preserve })));
                    headerRow.Append(tc);
                }
                var trPr = new TableRowProperties(); trPr.Append(new TableHeader()); headerRow.PrependChild(trPr);
                tbl.Append(headerRow);

                // placeholder empty data row so replacement logic finds the table structure
                var emptyRow = new TableRow();
                int colCount = headers.Count();
                for (int i = 0; i < colCount; i++)
                {
                    emptyRow.Append(new TableCell(new Paragraph(new Run(new Text(string.Empty)))));
                }

                tbl.Append(emptyRow);

                body.Append(tbl);
                // add separator paragraph for readability
                body.Append(new Paragraph(new Run(new Break())));
            }
            main.Document.Save();
        }

        // IEnumerable overload -> materialize
        public void ReplaceTableByAltText(string altText, IEnumerable<string[]> rows, TableFormattingOptions? options = null)
            => ReplaceTableByAltText(altText, rows.ToArray(), null, null, options);

        // Merge by header names overload
        public void ReplaceTableByAltText(string altText, IReadOnlyList<string[]> rows, IEnumerable<string> mergeColumnNames, TableFormattingOptions? options = null)
            => ReplaceTableByAltText(altText, rows, null, mergeColumnNames, options);

        /// <summary>
        /// DOM table replacement with vertical merge support.
        /// </summary>
        public void ReplaceTableByAltText(string altText,
            IReadOnlyList<string[]> rows,
            IEnumerable<int>? mergeColumnIndexes = null,
            IEnumerable<string>? mergeColumnNames = null,
            TableFormattingOptions? options = null)
        {
            options ??= new TableFormattingOptions();
            var existing = FindTableByAltText(altText) ?? throw new InvalidOperationException($"Table '{altText}' not found");

            var headerTemplate = existing.Elements<TableRow>().FirstOrDefault();
            var headerTexts = headerTemplate?.Elements<TableCell>().Select(c => GetCellText(c).Trim()).ToList();

            var mergeIndices = ResolveMergeColumns(headerTexts, mergeColumnIndexes, mergeColumnNames);
            var mergeIdxList = mergeIndices.OrderBy(x => x).ToArray();
            // Resolve optional merge break columns from options
            HashSet<int>? breakCols = null;
            if (options.MergeBreakColumnIndexes != null && options.MergeBreakColumnIndexes.Count > 0)
                breakCols = new HashSet<int>(options.MergeBreakColumnIndexes);
            if (options.MergeBreakColumnNames != null && options.MergeBreakColumnNames.Count > 0 && headerTexts != null)
            {
                var map = headerTexts.Select((h, i) => new { h = h.Trim(), i })
                                     .ToDictionary(x => x.h, x => x.i, StringComparer.OrdinalIgnoreCase);
                breakCols ??= new HashSet<int>();
                foreach (var name in options.MergeBreakColumnNames)
                {
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    if (map.TryGetValue(name.Trim(), out var idx)) breakCols.Add(idx);
                }
            }

            int rowCount = rows?.Count ?? 0;
            var (isRestart, isContinuation) = ComputeVerticalMergeFlags(rows, mergeIdxList, breakCols);

            var newTbl = new Table();
            var origProps = existing.GetFirstChild<TableProperties>();
            if (origProps != null)
            {
                newTbl.Append((TableProperties)origProps.CloneNode(true));
            }
            else
            {
                newTbl.Append(new TableProperties());
            }
            // Build (or replace) TableGrid using width if provided
            if (options.ColumnWidthPercents != null && headerTexts != null && options.ColumnWidthPercents.Count == headerTexts.Count)
            {
                var grid = new TableGrid();
                foreach (var pct in options.ColumnWidthPercents)
                {
                    // Word expects width in twentieths of a point or DXA; we'll approximate using table width of 5000 twips (arbitrary) unless layout autofit.
                    // Better: use pct * 100 as dxa for simplicity.
                    var widthTwips = (int)Math.Round(pct * 50); // 100% -> 5000
                    grid.Append(new GridColumn() { Width = widthTwips.ToString() });
                }
                newTbl.Append(grid);
            }
            else
            {
                var origGrid = existing.GetFirstChild<TableGrid>() ?? existing.Elements<TableGrid>().FirstOrDefault();
                if (origGrid != null)
                {
                    newTbl.Append((TableGrid)origGrid.CloneNode(true));
                }
            }
            ApplyTableFormatting(newTbl.GetFirstChild<TableProperties>()!, options);

            if (headerTemplate != null)
            {
                var hdr = FormatHeader(headerTemplate, headerTexts, options);
                if (options.ColumnWidthPercents != null && options.ColumnWidthPercents.Count == hdr.Elements<TableCell>().Count())
                {
                    int ci = 0;
                    foreach (var tc in hdr.Elements<TableCell>())
                    {
                        var pct = options.ColumnWidthPercents[ci++];
                        var tcPr = tc.GetFirstChild<TableCellProperties>() ?? tc.PrependChild(new TableCellProperties());
                        // Table width approximate 5000 twips baseline
                        var w = (int)Math.Round(pct * 50);
                        tcPr.TableCellWidth = new TableCellWidth { Type = TableWidthUnitValues.Dxa, Width = w.ToString() };
                    }
                }
                // ensure header row does not split across pages if requested
                if (!options.AllowRowBreakAcrossPages)
                {
                    var trPr = hdr.GetFirstChild<TableRowProperties>() ?? new TableRowProperties();
                    if (!trPr.Elements<CantSplit>().Any())
                    {
                        trPr.PrependChild(new CantSplit());
                    }
                    hdr.TableRowProperties = trPr;
                }
                newTbl.Append(hdr);
            }

            for (int r = 0; r < rowCount; r++)
            {
                var data = rows[r];
                var tr = new TableRow();
                if (!options.AllowRowBreakAcrossPages)
                {
                    var trPr = new TableRowProperties();
                    trPr.Append(new CantSplit());
                    tr.Append(trPr);
                }
                bool shadeRow = options.AlternateRowShading && !options.LightWeight && (r % 2 == 1);
                for (int c = 0; c < data.Length; c++)
                {
                    var tc = new TableCell();
                    TableCellProperties? tcPr = null;
                    int pos = Array.IndexOf(mergeIdxList, c);
                    if (pos >= 0)
                    {
                        tcPr = tcPr ?? new TableCellProperties();
                        if (isRestart[r, pos])
                        {
                            tcPr.Append(new VerticalMerge { Val = MergedCellValues.Restart });
                        }
                        else if (isContinuation[r, pos])
                        {
                            tcPr.Append(new VerticalMerge());
                        }
                    }
                    string? explicitFill = null;
                    if (options.CellFills != null && r < options.CellFills.Count)
                    {
                        var rowFills = options.CellFills[r];
                        if (rowFills != null && c < rowFills.Count) explicitFill = rowFills[c];
                    }
                    if (explicitFill != null)
                    {
                        tcPr = tcPr ?? new TableCellProperties();
                        tcPr.Append(new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = explicitFill });
                    }
                    else if (shadeRow)
                    {
                        tcPr = tcPr ?? new TableCellProperties();
                        var alt = options.AlternateRowFillColor ?? "C1E4F5";
                        tcPr.Append(new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = alt });
                    }
                    if (options.ColumnWidthPercents != null && options.ColumnWidthPercents.Count == data.Length)
                    {
                        tcPr = tcPr ?? new TableCellProperties();
                        var w = (int)Math.Round(options.ColumnWidthPercents[c] * 50);
                        tcPr.TableCellWidth = new TableCellWidth { Type = TableWidthUnitValues.Dxa, Width = w.ToString() };
                    }
                    // Column-specific or global vertical alignment
                    var vAlign = GetColumnOverride(options.ColumnVerticalAlignments, c) ?? options.VerticalAlignment;
                    if (!string.IsNullOrWhiteSpace(vAlign))
                    {
                        var va = vAlign.Trim().ToLowerInvariant();
                        TableVerticalAlignmentValues? vVal = va switch
                        {
                            "top" => TableVerticalAlignmentValues.Top,
                            "middle" => TableVerticalAlignmentValues.Center,
                            "center" => TableVerticalAlignmentValues.Center,
                            "bottom" => TableVerticalAlignmentValues.Bottom,
                            _ => null
                        };
                        if (vVal.HasValue)
                        {
                            tcPr = tcPr ?? new TableCellProperties();
                            if (!tcPr.Elements<TableCellVerticalAlignment>().Any())
                            {
                                tcPr.Append(new TableCellVerticalAlignment { Val = vVal.Value });
                            }
                        }
                    }
                    if (tcPr != null && tcPr.HasChildren)
                    {
                        tc.Append(tcPr);
                    }

                    string cellValue = data[c] ?? string.Empty;
                    var p = new Paragraph();
                    var hAlign = GetColumnOverride(options.ColumnHorizontalAlignments, c) ?? options.HorizontalAlignment;
                    if (!string.IsNullOrWhiteSpace(hAlign))
                    {
                        var ha = hAlign.Trim().ToLowerInvariant();
                        JustificationValues? jv = ha switch
                        {
                            "left" => JustificationValues.Left,
                            "center" => JustificationValues.Center,
                            "right" => JustificationValues.Right,
                            "justify" => JustificationValues.Both,
                            _ => null
                        };
                        if (jv.HasValue)
                        {
                            var pPr = p.GetFirstChild<ParagraphProperties>() ?? p.PrependChild(new ParagraphProperties());
                            pPr.Justification = new Justification { Val = jv.Value };
                        }
                    }
                    var segments = cellValue.Split('\n');
                    // Remove duplicate lines if requested
                    if (options.RemoveDuplicateLines && segments.Length > 1)
                    {
                        var comparer = options.RemoveDuplicateLinesStrict ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
                        var seen = new HashSet<string>(comparer);
                        var filtered = new List<string>();
                        foreach (var s in segments)
                        {
                            var key = options.RemoveDuplicateLinesStrict ? (s ?? string.Empty).Trim() : s ?? string.Empty;
                            if (seen.Add(key)) filtered.Add(s);
                        }
                        segments = filtered.ToArray();
                    }
                    int fontSizeHp = GetColumnOverride(options.ColumnFontSizeHalfPoints, c) ?? options.FontSizeHalfPoints;
                    string cellFontSizeVal = fontSizeHp.ToString();
                    var fontName = GetColumnOverride(options.ColumnFontNames, c) ?? options.FontName;
                    for (int si = 0; si < segments.Length; si++)
                    {
                        if (si > 0)
                        {
                            p.Append(new Run(new Break()));
                        }
                        var runProps = new RunProperties();
                        runProps.Append(new FontSize { Val = cellFontSizeVal });
                        if (!string.IsNullOrWhiteSpace(fontName))
                        {
                            runProps.Append(new RunFonts { Ascii = fontName, HighAnsi = fontName, EastAsia = fontName });
                        }
                        if (options.CellTextColors != null && r < options.CellTextColors.Count)
                        {
                            var rowColors = options.CellTextColors[r];
                            if (rowColors != null && c < rowColors.Count)
                            {
                                var color = rowColors[c];
                                if (!string.IsNullOrWhiteSpace(color))
                                {
                                    runProps.Append(new Color { Val = color });
                                }
                            }
                        }
                        var text = new Text(segments[si]) { Space = SpaceProcessingModeValues.Preserve };
                        p.Append(new Run(runProps, text));
                    }
                    if (!p.HasChildren)
                    {
                        var runProps = new RunProperties();
                        runProps.Append(new FontSize { Val = cellFontSizeVal });
                        if (!string.IsNullOrWhiteSpace(fontName))
                        {
                            runProps.Append(new RunFonts { Ascii = fontName, HighAnsi = fontName, EastAsia = fontName });
                        }
                        p.Append(new Run(runProps, new Text(string.Empty)));
                    }
                    tc.Append(p);
                    tr.Append(tc);
                }
                newTbl.Append(tr);
            }

            existing.Parent!.InsertAfter(newTbl, existing);
            existing.Remove();
            _mainPart.Document.Save();
        }

        private TableRow FormatHeader(TableRow template, List<string>? headerTexts, TableFormattingOptions options)
        {
            var hdr = (TableRow)template.CloneNode(true);
            if (options.RepeatHeaderRow)
            {
                var trPr = hdr.GetFirstChild<TableRowProperties>() ?? new TableRowProperties();
                if (!trPr.Elements<TableHeader>().Any())
                {
                    trPr.Append(new TableHeader());
                }
                hdr.TableRowProperties = trPr;
            }
            bool applyFill = !options.LightWeight && !string.IsNullOrEmpty(options.HeaderFillColor);
            if (options.HeaderBold || applyFill || !string.IsNullOrWhiteSpace(options.FontName) || !string.IsNullOrWhiteSpace(options.HorizontalAlignment) || !string.IsNullOrWhiteSpace(options.VerticalAlignment) || (options.ColumnFontNames != null) || (options.ColumnFontSizeHalfPoints != null) || (options.ColumnHorizontalAlignments != null) || (options.ColumnVerticalAlignments != null))
            {
                int colIndex = 0;
                foreach (var tc in hdr.Elements<TableCell>())
                {
                    var tcPr = tc.GetFirstChild<TableCellProperties>() ?? tc.PrependChild(new TableCellProperties());
                    if (applyFill && !tcPr.Elements<Shading>().Any())
                    {
                        tcPr.Append(new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = options.HeaderFillColor });
                    }
                    // Vertical alignment override
                    var vAlign = GetColumnOverride(options.ColumnVerticalAlignments, colIndex) ?? options.VerticalAlignment;
                    if (!string.IsNullOrWhiteSpace(vAlign))
                    {
                        var va = vAlign.Trim().ToLowerInvariant();
                        TableVerticalAlignmentValues? vVal = va switch
                        {
                            "top" => TableVerticalAlignmentValues.Top,
                            "middle" => TableVerticalAlignmentValues.Center,
                            "center" => TableVerticalAlignmentValues.Center,
                            "bottom" => TableVerticalAlignmentValues.Bottom,
                            _ => null
                        };
                        if (vVal.HasValue && !tcPr.Elements<TableCellVerticalAlignment>().Any())
                        {
                            tcPr.Append(new TableCellVerticalAlignment { Val = vVal.Value });
                        }
                    }
                    var hAlign = GetColumnOverride(options.ColumnHorizontalAlignments, colIndex) ?? options.HorizontalAlignment;
                    if (!string.IsNullOrWhiteSpace(hAlign))
                    {
                        var ha = hAlign.Trim().ToLowerInvariant();
                        JustificationValues? jv = ha switch
                        {
                            "left" => JustificationValues.Left,
                            "center" => JustificationValues.Center,
                            "right" => JustificationValues.Right,
                            "justify" => JustificationValues.Both,
                            _ => null
                        };
                        if (jv.HasValue)
                        {
                            foreach (var p in tc.Elements<Paragraph>())
                            {
                                var pPr = p.GetFirstChild<ParagraphProperties>() ?? p.PrependChild(new ParagraphProperties());
                                pPr.Justification = new Justification { Val = jv.Value };
                            }
                        }
                    }
                    int fontSizeHp = GetColumnOverride(options.ColumnFontSizeHalfPoints, colIndex) ?? options.FontSizeHalfPoints;
                    string fontSizeVal = fontSizeHp.ToString();
                    var fontName = GetColumnOverride(options.ColumnFontNames, colIndex) ?? options.FontName;
                    foreach (var run in tc.Descendants<Run>())
                    {
                        var rPr = run.GetFirstChild<RunProperties>() ?? run.PrependChild(new RunProperties());
                        if (options.HeaderBold && !rPr.Elements<Bold>().Any())
                        {
                            rPr.Append(new Bold());
                        }
                        if (!rPr.Elements<FontSize>().Any())
                        {
                            rPr.Append(new FontSize { Val = fontSizeVal });
                        }
                        if (!string.IsNullOrWhiteSpace(fontName) && !rPr.Elements<RunFonts>().Any())
                        {
                            rPr.Append(new RunFonts { Ascii = fontName, HighAnsi = fontName, EastAsia = fontName });
                        }
                    }
                    colIndex++;
                }
            }
            return hdr;
        }

        private T? GetColumnOverride<T>(List<T?>? list, int index) where T : struct
        {
            if (list == null)
            {
                return null;
            }
            if (index < 0 || index >= list.Count)
            {
                return null;
            }
            return list[index];
        }
        private string? GetColumnOverride(List<string?>? list, int index)
        {
            if (list == null)
            {
                return null;
            }
            if (index < 0 || index >= list.Count)
            {
                return null;
            }
            return list[index];
        }

        private HashSet<int> ResolveMergeColumns(List<string>? headerTexts,
            IEnumerable<int>? mergeColumnIndexes,
            IEnumerable<string>? mergeColumnNames)
        {
            var mergeIndices = new HashSet<int>();
            if (mergeColumnIndexes != null)
            {
                foreach (var idx in mergeColumnIndexes) if (idx >= 0) mergeIndices.Add(idx);
            }
            if (mergeColumnNames != null)
            {
                if (headerTexts == null) throw new InvalidOperationException("No header row present to resolve mergeColumnNames.");
                var map = headerTexts.Select((h, i) => new { h = h.Trim(), i })
                                     .ToDictionary(x => x.h, x => x.i, StringComparer.OrdinalIgnoreCase);
                var missing = new List<string>();
                foreach (var name in mergeColumnNames)
                {
                    if (name == null) continue; var key = name.Trim();
                    if (map.TryGetValue(key, out var mi)) mergeIndices.Add(mi); else missing.Add(name);
                }
                if (missing.Count > 0)
                {
                    throw new KeyNotFoundException($"Merge column(s) not found: {string.Join(", ", missing)}. Available: {string.Join(", ", headerTexts)}");
                }
            }
            return mergeIndices;
        }

        private (bool[,] isRestart, bool[,] isContinuation) ComputeVerticalMergeFlags(IReadOnlyList<string[]> rows, int[] mergeIdxList, HashSet<int>? breakCols = null)
        {
            int rowCount = rows?.Count ??0;
            bool[,] isRestart = new bool[rowCount, Math.Max(1, mergeIdxList.Length)];
            bool[,] isContinuation = new bool[rowCount, Math.Max(1, mergeIdxList.Length)];
            if (mergeIdxList.Length ==0 || rowCount ==0) return (isRestart, isContinuation);
            for (int m =0; m < mergeIdxList.Length; m++)
            {
                int col = mergeIdxList[m];
                for (int r =0; r < rowCount; r++)
                {
                    var curr = GetSafe(rows[r], col);
                    var prev = r >0 ? GetSafe(rows[r -1], col) : null;
                    var next = r < rowCount -1 ? GetSafe(rows[r +1], col) : null;
                    bool boundaryBreak = false;
                    if (breakCols != null && r >0)
                    {
                        foreach (var bcol in breakCols)
                        {
                            var currGroup = GetSafe(rows[r], bcol);
                            var prevGroup = GetSafe(rows[r -1], bcol);
                            if (!string.Equals(currGroup, prevGroup, StringComparison.Ordinal)) { boundaryBreak = true; break; }
                        }
                    }
                    bool cont = !boundaryBreak && prev != null && curr == prev;
                    bool restart = !boundaryBreak && next != null && curr == next && (r ==0 || curr != prev || boundaryBreak);
                    isContinuation[r, m] = cont;
                    isRestart[r, m] = restart;
                }
            }
            return (isRestart, isContinuation);
        }

        private void ApplyTableFormatting(TableProperties props, TableFormattingOptions opt)
        {
            if (opt.FitToWindow)
            {
                // Force full page width (100%) instead of Auto so short/narrow content tables expand
                var tw = props.Elements<TableWidth>().FirstOrDefault();
                if (tw == null)
                {
                    props.Append(new TableWidth { Type = TableWidthUnitValues.Pct, Width = "5000" }); //5000 =>100%
                }
                else
                {
                    tw.Type = TableWidthUnitValues.Pct; tw.Width = "5000";
                }
                if (!props.Elements<TableLayout>().Any()) props.Append(new TableLayout { Type = TableLayoutValues.Autofit });
            }
            if (!opt.LightWeight && opt.AddBorders && !props.Elements<TableBorders>().Any())
            {
                props.Append(new TableBorders(
                    new TopBorder { Val = BorderValues.Single, Size =4 },
                    new LeftBorder { Val = BorderValues.Single, Size =4 },
                    new BottomBorder { Val = BorderValues.Single, Size =4 },
                    new RightBorder { Val = BorderValues.Single, Size =4 },
                    new InsideHorizontalBorder { Val = BorderValues.Single, Size =4 },
                    new InsideVerticalBorder { Val = BorderValues.Single, Size =4 }
                ));
            }
        }

        private string GetSafe(string[] row, int idx) => (row != null && idx >= 0 && idx < row.Length) ? (row[idx] ?? string.Empty) : string.Empty;
        private string GetCellText(TableCell tc) => string.Join(string.Empty, tc.Descendants<Text>().Select(t => t.Text));

        private Table? FindTableByAltText(string altText)
        {
            foreach (var t in _mainPart.Document.Descendants<Table>())
            {
                var props = t.GetFirstChild<TableProperties>();
                if (props == null) continue;
                var cap = props.GetFirstChild<TableCaption>();
                if (cap?.Val?.Value != null && string.Equals(cap.Val.Value, altText, StringComparison.OrdinalIgnoreCase)) return t;
                var desc = props.GetFirstChild<TableDescription>();
                if (desc?.Val?.Value != null && string.Equals(desc.Val.Value, altText, StringComparison.OrdinalIgnoreCase)) return t;
            }
            return null;
        }

        /// <summary>
        /// Enumerate tables in a DOCX returning index (1-based), caption, description, row count and column count (max cells in first row).
        /// </summary>
        public static IEnumerable<(int Index, string? Caption, string? Description, int Rows, int Cols)> ListTables(string path)
        {
            using var doc = WordprocessingDocument.Open(path, false);
            var body = doc.MainDocumentPart?.Document?.Body;
            if (body == null) yield break;
            int i = 0;
            foreach (var tbl in body.Descendants<Table>())
            {
                i++;
                var props = tbl.GetFirstChild<TableProperties>();
                var cap = props?.GetFirstChild<TableCaption>()?.Val?.Value;
                var desc = props?.GetFirstChild<TableDescription>()?.Val?.Value;
                int rows = tbl.Elements<TableRow>().Count();
                int cols = tbl.Elements<TableRow>().FirstOrDefault()?.Elements<TableCell>().Count() ?? 0;
                yield return (i, cap, desc, rows, cols);
            }
        }

        public void SaveAs(string path)
        {
            _mainPart.Document.Save();
            var dest = Path.GetFullPath(path);
            if (string.Equals(Path.GetFullPath(_filePath), dest, StringComparison.OrdinalIgnoreCase)) return;
            var clone = _doc.Clone(dest); clone.Dispose();
        }

        /// <summary>
        /// Replace a table (by alt text) with a single-cell table containing a message. No header row is preserved.
        /// The table is set to 100% width.
        /// </summary>
        public void ReplaceTableWithSingleCellMessage(string altText, string message, TableFormattingOptions? options = null, string? overrideFontName = null, int? overrideFontSizeHalfPoints = null, string? overrideHAlign = null, string? overrideVAlign = null, bool? overrideBold = null, string? overrideFill = null, string? overrideTextColor = null)
        {
            options ??= new TableFormattingOptions();
            var existing = FindTableByAltText(altText) ?? throw new InvalidOperationException($"Table '{altText}' not found");

            var newTbl = new Table();
            var props = new TableProperties();
            props.Append(new TableCaption { Val = altText });
            props.Append(new TableDescription { Val = altText });
            props.Append(new TableWidth { Type = TableWidthUnitValues.Pct, Width = "5000" });
            props.Append(new TableLayout { Type = TableLayoutValues.Autofit });
            if (options.AddBorders && !options.LightWeight)
            {
                props.Append(new TableBorders(
                    new TopBorder { Val = BorderValues.Single, Size = 4 },
                    new LeftBorder { Val = BorderValues.Single, Size = 4 },
                    new BottomBorder { Val = BorderValues.Single, Size = 4 },
                    new RightBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 }
                ));
            }
            newTbl.Append(props);
            var row = new TableRow();
            var cell = new TableCell();
            var tcPr = new TableCellProperties();
            tcPr.Append(new TableCellWidth { Type = TableWidthUnitValues.Pct, Width = "5000" });
            string? fill = overrideFill;
            if (fill == null && options.CellFills != null && options.CellFills.Count > 0)
            {
                var firstRow = options.CellFills[0];
                if (firstRow != null && firstRow.Count > 0) fill = firstRow[0];
            }
            if (!string.IsNullOrWhiteSpace(fill))
            {
                tcPr.Append(new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = fill });
            }
            var vAlignRaw = overrideVAlign ?? options.VerticalAlignment;
            if (!string.IsNullOrWhiteSpace(vAlignRaw))
            {
                var va = vAlignRaw.Trim().ToLowerInvariant();
                TableVerticalAlignmentValues? vVal = va switch
                {
                    "top" => TableVerticalAlignmentValues.Top,
                    "middle" => TableVerticalAlignmentValues.Center,
                    "center" => TableVerticalAlignmentValues.Center,
                    "bottom" => TableVerticalAlignmentValues.Bottom,
                    _ => null
                };
                if (vVal.HasValue) tcPr.Append(new TableCellVerticalAlignment { Val = vVal.Value });
            }
            cell.Append(tcPr);
            var p = new Paragraph();
            var hAlignRaw = overrideHAlign ?? options.HorizontalAlignment;
            if (!string.IsNullOrWhiteSpace(hAlignRaw))
            {
                var ha = hAlignRaw.Trim().ToLowerInvariant();
                JustificationValues? jv = ha switch
                {
                    "left" => JustificationValues.Left,
                    "center" => JustificationValues.Center,
                    "right" => JustificationValues.Right,
                    "justify" => JustificationValues.Both,
                    _ => null
                };
                if (jv.HasValue)
                {
                    var pPr = p.GetFirstChild<ParagraphProperties>() ?? p.PrependChild(new ParagraphProperties());
                    pPr.Justification = new Justification { Val = jv.Value };
                }
            }
            var runProps = new RunProperties();
            int fontHp = overrideFontSizeHalfPoints ?? (options.FontSizeHalfPoints > 0 ? options.FontSizeHalfPoints : 20);
            runProps.Append(new FontSize { Val = fontHp.ToString() });
            bool bold = overrideBold ?? options.HeaderBold;
            if (bold) runProps.Append(new Bold());
            var fontName = overrideFontName ?? options.FontName;
            if (!string.IsNullOrWhiteSpace(fontName))
            {
                runProps.Append(new RunFonts { Ascii = fontName, HighAnsi = fontName, EastAsia = fontName });
            }
            var textColor = overrideTextColor;
            if (textColor == null && options.CellTextColors != null && options.CellTextColors.Count > 0)
            {
                var firstRowColors = options.CellTextColors[0];
                if (firstRowColors != null && firstRowColors.Count > 0) textColor = firstRowColors[0];
            }
            if (!string.IsNullOrWhiteSpace(textColor))
            {
                runProps.Append(new Color { Val = textColor });
            }
            var run = new Run(runProps, new Text(message ?? string.Empty) { Space = SpaceProcessingModeValues.Preserve });
            p.Append(run);
            cell.Append(p);
            row.Append(cell);
            if (!options.AllowRowBreakAcrossPages)
            {
                var trPr = row.GetFirstChild<TableRowProperties>() ?? row.PrependChild(new TableRowProperties());
                trPr.Append(new CantSplit());
            }
            newTbl.Append(row);
            existing.Parent!.InsertAfter(newTbl, existing);
            existing.Remove();
            _mainPart.Document.Save();
        }

        /// <summary>
        /// Replace the header row of the table identified by altText with a new header built from provided headers.
        /// This is useful when the template's header row does not have discrete cells matching desired columns.
        /// </summary>
        public void SetHeaderRowByAltText(string altText, IEnumerable<string> headers)
        {
            var tbl = FindTableByAltText(altText);
            if (tbl == null) throw new InvalidOperationException($"Table '{altText}' not found");
            var firstRow = tbl.Elements<TableRow>().FirstOrDefault();
            // create new header row
            var hdr = new TableRow();
            var trPr = new TableRowProperties(); trPr.Append(new TableHeader()); hdr.Append(trPr);
            foreach (var h in headers)
            {
                var tc = new TableCell(new Paragraph(new Run(new Text(h ?? string.Empty) { Space = SpaceProcessingModeValues.Preserve })));
                hdr.Append(tc);
            }
            if (firstRow != null)
            {
                tbl.InsertBefore(hdr, firstRow);
                firstRow.Remove();
            }
            else
            {
                tbl.Append(hdr);
            }
            _mainPart.Document.Save();
        }

        public void Dispose() => _doc?.Dispose();
    }
}
