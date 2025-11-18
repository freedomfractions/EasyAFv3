using System;
using System.IO;
using System.Threading;

namespace EasyAF.Import
{
    using EasyAF.Data.Models;
    
    /// <summary>
    /// Orchestrates the import of data from external files (CSV, Excel) into EasyAF DataSet structures.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Purpose:</strong> ImportManager is the entry point for all data import operations.
    /// It handles file type detection, importer creation, file accessibility verification, 
    /// and error handling throughout the import pipeline.
    /// </para>
    /// <para>
    /// <strong>Import Pipeline:</strong>
    /// </para>
    /// <list type="number">
    /// <item><description>Verify file accessibility (with retry logic for locked files)</description></item>
    /// <item><description>Detect file type based on extension (.csv, .xls, .xlsx)</description></item>
    /// <item><description>Create appropriate importer (CsvImporter or ExcelImporter)</description></item>
    /// <item><description>Execute import using provided MappingConfig</description></item>
    /// <item><description>Populate target DataSet with imported data</description></item>
    /// <item><description>Log results and handle errors</description></item>
    /// </list>
    /// <para>
    /// <strong>File Locking Handling:</strong> ImportManager includes automatic retry logic to handle
    /// files that may be temporarily locked (e.g., open in Excel). It will retry for up to 5 seconds
    /// before throwing an IOException.
    /// </para>
    /// <para>
    /// <strong>Logging:</strong> All import operations are logged using the provided ILogger.
    /// If no logger is provided, a FileLogger writing to "import.log" is used by default.
    /// </para>
    /// <para>
    /// <strong>Thread Safety:</strong> ImportManager instances are NOT thread-safe. Create separate
    /// instances for concurrent import operations.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para><strong>Example 1: Basic import with CSV file</strong></para>
    /// <code>
    /// // Load mapping configuration
    /// var mappingConfig = MappingConfig.Load("mappings/bus-import.ezmap");
    /// 
    /// // Create target dataset
    /// var dataSet = new DataSet();
    /// 
    /// // Create import manager
    /// var importManager = new ImportManager();
    /// 
    /// // Execute import
    /// importManager.Import("data/buses.csv", mappingConfig, dataSet);
    /// 
    /// // Access imported data
    /// foreach (var bus in dataSet.BusEntries.Values)
    /// {
    ///     Console.WriteLine($"Imported bus: {bus.Id} - {bus.Name}");
    /// }
    /// </code>
    /// 
    /// <para><strong>Example 2: Import with options and custom logger</strong></para>
    /// <code>
    /// // Create custom logger
    /// var logger = new FileLogger("my-import.log", LogMode.Verbose);
    /// 
    /// // Create import manager with logger
    /// var importManager = new ImportManager(logger);
    /// 
    /// // Configure import options
    /// var options = new ImportOptions
    /// {
    ///     SkipBlankRows = true,
    ///     TrimWhitespace = true
    /// };
    /// 
    /// // Execute import with options
    /// try
    /// {
    ///     importManager.Import("data/equipment.xlsx", mappingConfig, dataSet, options);
    ///     Console.WriteLine("Import successful!");
    /// }
    /// catch (IOException ex)
    /// {
    ///     Console.WriteLine($"File is locked: {ex.Message}");
    /// }
    /// catch (NotSupportedException ex)
    /// {
    ///     Console.WriteLine($"Unsupported file type: {ex.Message}");
    /// }
    /// </code>
    /// 
    /// <para><strong>Example 3: Handling file locking (Excel open)</strong></para>
    /// <code>
    /// var importManager = new ImportManager();
    /// 
    /// try
    /// {
    ///     // This will retry for up to 5 seconds if file is locked
    ///     importManager.Import("data/locked-file.xlsx", mappingConfig, dataSet);
    /// }
    /// catch (IOException ex)
    /// {
    ///     // File still locked after retry timeout
    ///     Console.WriteLine("Please close the file in Excel and try again.");
    ///     Console.WriteLine(ex.Message);
    /// }
    /// </code>
    /// </example>
    public class ImportManager
    {
        private readonly ILogger _logger;
        
        // File accessibility retry configuration
        private readonly TimeSpan _fileWaitTimeout = TimeSpan.FromSeconds(5);
        private readonly TimeSpan _fileRetryDelay = TimeSpan.FromMilliseconds(250);

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportManager"/> class.
        /// </summary>
        /// <param name="logger">
        /// Optional logger for import operations. If null, a default FileLogger writing to "import.log" is used.
        /// </param>
        /// <remarks>
        /// <para>
        /// The logger receives messages at various levels:
        /// - Verbose: Detailed step-by-step progress
        /// - Info: Major milestones (import started, completed)
        /// - Error: Errors and exceptions
        /// </para>
        /// </remarks>
        public ImportManager(ILogger? logger = null) 
        { 
            _logger = logger ?? new FileLogger("import.log", LogMode.Standard); 
        }

        /// <summary>
        /// Imports data from a file into the specified DataSet using the provided mapping configuration.
        /// </summary>
        /// <param name="filePath">
        /// Path to the file to import. Supported formats: .csv, .xls, .xlsx
        /// </param>
        /// <param name="mappingConfig">
        /// Mapping configuration defining how file columns map to DataSet properties.
        /// Use an <see cref="ImmutableMappingConfig"/> for thread safety.
        /// </param>
        /// <param name="targetDataSet">
        /// The DataSet to populate with imported data. Existing data is preserved; 
        /// new entries are added or updated based on keys.
        /// </param>
        /// <param name="options">
        /// Optional import options controlling parsing behavior. If null, default options are used.
        /// </param>
        /// <exception cref="IOException">
        /// Thrown when the file cannot be accessed (locked, missing, or inaccessible after retry timeout).
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Thrown when the file extension is not supported (.csv, .xls, .xlsx only).
        /// </exception>
        /// <exception cref="InvalidDataException">
        /// Thrown when the file content is invalid or cannot be parsed according to the mapping configuration.
        /// </exception>
        /// <remarks>
        /// <para>
        /// <strong>Workflow:</strong>
        /// </para>
        /// <list type="number">
        /// <item><description>Verify file is readable (retry up to 5 seconds if locked)</description></item>
        /// <item><description>Detect file type and create appropriate importer</description></item>
        /// <item><description>Execute importer-specific parsing and mapping logic</description></item>
        /// <item><description>Populate targetDataSet with parsed data</description></item>
        /// </list>
        /// <para>
        /// <strong>File Locking:</strong> If the file is locked (e.g., open in Excel), ImportManager
        /// will retry opening it every 250ms for up to 5 seconds before throwing an IOException.
        /// </para>
        /// <para>
        /// <strong>Data Merging:</strong> Imported data is merged into the target DataSet:
        /// - New entries are added
        /// - Existing entries (matching by Id/key) may be updated depending on importer behavior
        /// - No data is removed from the target DataSet
        /// </para>
        /// <para>
        /// <strong>Logging:</strong> The import process is logged at multiple levels:
        /// - Verbose: File preparation, accessibility checks
        /// - Info: Import completion summary
        /// - Error: I/O errors, parsing failures, exceptions
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var manager = new ImportManager();
        /// var config = MappingConfig.Load("mapping.ezmap").ToImmutable();
        /// var dataSet = new DataSet();
        /// 
        /// try
        /// {
        ///     manager.Import("data.csv", config, dataSet);
        ///     Console.WriteLine($"Imported {dataSet.BusEntries.Count} buses");
        /// }
        /// catch (IOException ex)
        /// {
        ///     Console.WriteLine($"File error: {ex.Message}");
        /// }
        /// catch (NotSupportedException ex)
        /// {
        ///     Console.WriteLine($"Invalid file type: {ex.Message}");
        /// }
        /// </code>
        /// </example>
        public void Import(string filePath, IMappingConfig mappingConfig, DataSet targetDataSet, ImportOptions? options = null)
        {
            options ??= new ImportOptions();
            _logger.Verbose(nameof(ImportManager), $"Preparing to import file '{Path.GetFileName(filePath)}'");
            
            // Verify file accessibility (with retry for locked files)
            EnsureReadable(filePath);
            _logger.Verbose(nameof(ImportManager), $"File is readable: '{Path.GetFileName(filePath)}'");

            // Create appropriate importer based on file extension
            IImporter importer = CreateImporter(filePath);
            
            try
            {
                // Execute import via importer
                importer.Import(filePath, mappingConfig, targetDataSet, options);
                _logger.Info(nameof(ImportManager), $"Import complete: '{Path.GetFileName(filePath)}'");
            }
            catch (IOException ioEx)
            {
                _logger.Error(nameof(ImportManager), $"I/O error importing '{Path.GetFileName(filePath)}': {ioEx.Message}");
                throw new IOException($"I/O error importing '{Path.GetFileName(filePath)}': {ioEx.Message}", ioEx);
            }
            catch (Exception ex)
            {
                _logger.Error(nameof(ImportManager), $"Unexpected error importing '{Path.GetFileName(filePath)}': {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Creates the appropriate importer based on file extension.
        /// </summary>
        /// <param name="filePath">Path to the file being imported.</param>
        /// <returns>An <see cref="IImporter"/> instance configured for the file type.</returns>
        /// <exception cref="NotSupportedException">
        /// Thrown when the file extension is not recognized. Supported: .csv, .xls, .xlsx
        /// </exception>
        /// <remarks>
        /// <para>
        /// <strong>Supported File Types:</strong>
        /// </para>
        /// <list type="bullet">
        /// <item><description><strong>.csv</strong> - Comma-separated values (uses <see cref="CsvImporter"/>)</description></item>
        /// <item><description><strong>.xls</strong> - Excel 97-2003 format (uses <see cref="ExcelImporter"/>)</description></item>
        /// <item><description><strong>.xlsx</strong> - Excel 2007+ format (uses <see cref="ExcelImporter"/>)</description></item>
        /// </list>
        /// <para>
        /// Extension matching is case-insensitive (.CSV, .Csv, .csv all work).
        /// </para>
        /// </remarks>
        private IImporter CreateImporter(string filePath)
        {
            var extension = Path.GetExtension(filePath)?.ToLowerInvariant();
            return extension switch
            {
                ".csv" => new CsvImporter(_logger),
                ".xls" or ".xlsx" => new ExcelImporter(_logger),
                _ => throw new NotSupportedException($"Unsupported file type: {extension}. Supported types: .csv, .xls, .xlsx")
            };
        }

        /// <summary>
        /// Ensures the file is readable, retrying if the file is temporarily locked.
        /// </summary>
        /// <param name="filePath">Path to the file to verify.</param>
        /// <exception cref="IOException">
        /// Thrown if the file cannot be opened for reading after the retry timeout (5 seconds).
        /// </exception>
        /// <remarks>
        /// <para>
        /// <strong>Retry Logic:</strong> This method handles common scenarios where files may be
        /// temporarily locked, such as:
        /// - Excel has the file open for editing
        /// - Another process is reading/writing the file
        /// - Antivirus software is scanning the file
        /// </para>
        /// <para>
        /// The method retries every 250ms for up to 5 seconds. This provides a good balance between
        /// responsiveness and allowing time for locks to be released.
        /// </para>
        /// <para>
        /// <strong>Common Error Messages:</strong>
        /// </para>
        /// <list type="bullet">
        /// <item><description>File locked by Excel: Close Excel and retry</description></item>
        /// <item><description>File not found: Verify the file path</description></item>
        /// <item><description>Access denied: Check file permissions</description></item>
        /// </list>
        /// </remarks>
        private void EnsureReadable(string filePath)
        {
            var start = DateTime.UtcNow;
            while (true)
            {
                if (CanOpenForRead(filePath)) return;
                
                if (DateTime.UtcNow - start > _fileWaitTimeout)
                {
                    throw new IOException(
                        $"File '{filePath}' appears to be locked or inaccessible after waiting {_fileWaitTimeout.TotalSeconds:F1}s. " +
                        $"Close any application (e.g., Excel) using it and retry.");
                }
                
                Thread.Sleep(_fileRetryDelay);
            }
        }

        /// <summary>
        /// Attempts to open the file for reading to verify accessibility.
        /// </summary>
        /// <param name="path">Path to the file to test.</param>
        /// <returns>True if the file can be opened for reading; false otherwise.</returns>
        /// <remarks>
        /// Opens the file with FileShare.ReadWrite to allow the file to be locked by other processes
        /// for writing while we verify read access. This is less restrictive than FileShare.Read
        /// and reduces false positives for locked files.
        /// </remarks>
        private bool CanOpenForRead(string path)
        {
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                return fs.Length >= 0; // Access Length property to ensure handle is valid
            }
            catch (IOException)
            {
                return false; // File is locked or inaccessible
            }
            catch (UnauthorizedAccessException)
            {
                return false; // Insufficient permissions
            }
        }
    }
}
