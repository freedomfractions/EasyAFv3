namespace EasyAF.Data.Models
{
    /// <summary>
    /// Defines the type of import pipeline used for the project.
    /// Changing this value requires purging all dataset entries to prevent invalid data.
    /// </summary>
    public enum ProjectType
    {
        /// <summary>
        /// Standard EasyPower model import pipeline.
        /// </summary>
        Standard = 0,

        /// <summary>
        /// Composite model import pipeline with different data structure.
        /// </summary>
        Composite = 1
    }
}
