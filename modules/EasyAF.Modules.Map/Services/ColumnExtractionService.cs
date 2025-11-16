using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using EasyAF.Modules.Map.Models;
using Serilog;

namespace EasyAF.Modules.Map.Services
{
    /// <summary>
    /// Extracts column information from CSV and Excel files.
    /// </summary>
    /// <remarks>
    /// This service reads the headers from data files without loading the entire dataset,
    /// making it efficient for discovering available columns for mapping. Supports CSV,
    /// XLS, and XLSX formats.
    /// </remarks>
    public class ColumnExtractionService
    {
        /// <summary>
        /// Extracts columns from a data file (CSV or Excel).
        /// </summary>
        /// <param name="filePath">Path to the file to extract columns from.</param>
        /// <returns>
        /// Dictionary where key is the table/sheet name and value is the list of columns.
        /// For CSV files, key is typically "Sheet1". For Excel files, key is the actual sheet name.
        /// </returns>
        /// <exception cref="FileNotFoundException">If the file does not exist.</exception>
        /// <exception cref="NotSupportedException">If the file format is not supported.</exception>
        /// <exception cref="IOException">If the file cannot be read.</exception>
        public Dictionary<string, List<ColumnInfo>> ExtractColumns(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Log.Error("File not found: {FilePath}", filePath);
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            
            Log.Information("Extracting columns from {Extension} file: {FileName}", 
                extension, Path.GetFileName(filePath));

            try
            {
                return extension switch
                {
                    ".csv" => ExtractFromCsv(filePath),
                    ".xls" or ".xlsx" => ExtractFromExcel(filePath),
                    _ => throw new NotSupportedException(
                        $"File type '{extension}' is not supported. Supported formats: .csv, .xls, .xlsx")
                };
            }
            catch (Exception ex) when (ex is not NotSupportedException)
            {
                Log.Error(ex, "Failed to extract columns from {FilePath}", filePath);
                throw new IOException($"Failed to read file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Extracts columns from a CSV file.
        /// </summary>
        private Dictionary<string, List<ColumnInfo>> ExtractFromCsv(string filePath)
        {
            var result = new Dictionary<string, List<ColumnInfo>>();
            var columns = new List<ColumnInfo>();

            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    TrimOptions = TrimOptions.Trim,
                    BadDataFound = null // Ignore bad data during header read
                };

                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, config);

                // Read just the header
                csv.Read();
                csv.ReadHeader();

                if (csv.HeaderRecord == null)
                {
                    Log.Warning("CSV file has no header row: {FilePath}", filePath);
                    return result;
                }

                // Count non-empty rows for sample value count
                int rowCount = 0;
                while (csv.Read() && rowCount < 100) // Read up to 100 rows for sampling
                {
                    rowCount++;
                }

                // Create column info for each header
                for (int i = 0; i < csv.HeaderRecord.Length; i++)
                {
                    var header = csv.HeaderRecord[i];
                    columns.Add(new ColumnInfo
                    {
                        ColumnName = header,
                        ColumnIndex = i,
                        SourceTable = "Sheet1", // CSV files don't have sheet names
                        SampleValueCount = rowCount
                    });
                }

                result["Sheet1"] = columns;
                Log.Debug("Extracted {Count} columns from CSV", columns.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error reading CSV file: {FilePath}", filePath);
                throw;
            }

            return result;
        }

        /// <summary>
        /// Extracts columns from an Excel file (all sheets).
        /// </summary>
        private Dictionary<string, List<ColumnInfo>> ExtractFromExcel(string filePath)
        {
            var result = new Dictionary<string, List<ColumnInfo>>();

            try
            {
                using var workbook = new XLWorkbook(filePath);

                foreach (var worksheet in workbook.Worksheets)
                {
                    var columns = new List<ColumnInfo>();
                    
                    // Check if worksheet has data
                    if (worksheet.FirstRowUsed() == null)
                    {
                        Log.Debug("Skipping empty worksheet: {SheetName}", worksheet.Name);
                        continue;
                    }

                    var firstRow = worksheet.FirstRowUsed();
                    var lastColumn = firstRow.LastCellUsed()?.Address.ColumnNumber ?? 0;

                    if (lastColumn == 0)
                    {
                        Log.Debug("Worksheet has no columns: {SheetName}", worksheet.Name);
                        continue;
                    }

                    // Get row count for sample value count
                    var rowCount = Math.Min(worksheet.RowsUsed().Count() - 1, 100); // Exclude header, max 100

                    // Extract column headers from first row
                    for (int col = 1; col <= lastColumn; col++)
                    {
                        var cell = firstRow.Cell(col);
                        var columnName = cell.GetString();

                        // Skip empty headers
                        if (string.IsNullOrWhiteSpace(columnName))
                        {
                            columnName = $"Column{col}"; // Generate name for unnamed columns
                        }

                        columns.Add(new ColumnInfo
                        {
                            ColumnName = columnName.Trim(),
                            ColumnIndex = col - 1, // Zero-based index
                            SourceTable = worksheet.Name,
                            SampleValueCount = rowCount
                        });
                    }

                    result[worksheet.Name] = columns;
                    Log.Debug("Extracted {Count} columns from sheet '{SheetName}'", 
                        columns.Count, worksheet.Name);
                }

                if (result.Count == 0)
                {
                    Log.Warning("Excel file contains no readable sheets: {FilePath}", filePath);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error reading Excel file: {FilePath}", filePath);
                throw;
            }

            return result;
        }

        /// <summary>
        /// Gets a preview of sample data from a file.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        /// <param name="tableName">Name of the table/sheet to preview.</param>
        /// <param name="maxRows">Maximum number of rows to return (default 5).</param>
        /// <returns>DataTable containing the preview data.</returns>
        public DataTable GetSampleData(string filePath, string tableName, int maxRows = 5)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            try
            {
                return extension switch
                {
                    ".csv" => GetCsvSampleData(filePath, maxRows),
                    ".xls" or ".xlsx" => GetExcelSampleData(filePath, tableName, maxRows),
                    _ => throw new NotSupportedException($"File type '{extension}' not supported")
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to get sample data from {FilePath}", filePath);
                throw;
            }
        }

        private DataTable GetCsvSampleData(string filePath, int maxRows)
        {
            var table = new DataTable();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);

            csv.Read();
            csv.ReadHeader();

            if (csv.HeaderRecord != null)
            {
                foreach (var header in csv.HeaderRecord)
                {
                    table.Columns.Add(header);
                }
            }

            int rowsRead = 0;
            while (csv.Read() && rowsRead < maxRows)
            {
                var row = table.NewRow();
                for (int i = 0; i < csv.HeaderRecord?.Length; i++)
                {
                    row[i] = csv.GetField(i) ?? string.Empty;
                }
                table.Rows.Add(row);
                rowsRead++;
            }

            return table;
        }

        private DataTable GetExcelSampleData(string filePath, string sheetName, int maxRows)
        {
            var table = new DataTable();

            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheets.FirstOrDefault(w => w.Name == sheetName);

            if (worksheet == null)
            {
                throw new ArgumentException($"Sheet '{sheetName}' not found in workbook");
            }

            var firstRow = worksheet.FirstRowUsed();
            if (firstRow == null) return table;

            var lastColumn = firstRow.LastCellUsed()?.Address.ColumnNumber ?? 0;

            // Add columns
            for (int col = 1; col <= lastColumn; col++)
            {
                var columnName = firstRow.Cell(col).GetString();
                if (string.IsNullOrWhiteSpace(columnName))
                    columnName = $"Column{col}";
                table.Columns.Add(columnName);
            }

            // Add rows
            int rowsRead = 0;
            var currentRow = firstRow.RowBelow();
            while (currentRow != null && rowsRead < maxRows)
            {
                if (currentRow.IsEmpty())
                {
                    currentRow = currentRow.RowBelow();
                    continue;
                }

                var row = table.NewRow();
                for (int col = 1; col <= lastColumn; col++)
                {
                    row[col - 1] = currentRow.Cell(col).GetString();
                }
                table.Rows.Add(row);
                rowsRead++;
                currentRow = currentRow.RowBelow();
            }

            return table;
        }
    }
}
