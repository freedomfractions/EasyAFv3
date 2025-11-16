namespace EasyAF.Import
{
    using EasyAF.Data.Models;

    /// <summary>
    /// Defines an interface for importing data from a file into a DataSet using a mapping configuration.
    /// </summary>
    public interface IImporter
    {
        /// <summary>
        /// Imports data from the specified file into the given DataSet using the provided mapping configuration.
        /// </summary>
        /// <param name="filePath">The path to the data file (CSV or Excel).</param>
        /// <param name="mappingConfig">The mapping configuration to use.</param>
        /// <param name="targetDataSet">The DataSet to populate.</param>
        /// <param name="options">The import options to apply.</param>
        void Import(string filePath, IMappingConfig mappingConfig, DataSet targetDataSet, ImportOptions? options = null);
    }
}
