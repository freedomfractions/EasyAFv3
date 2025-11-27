using System.Collections.Generic;

namespace EasyAF.Modules.Project.Models
{
    /// <summary>
    /// Settings for the Project Editor module.
    /// </summary>
    /// <remarks>
    /// Stored in settings.json under "ProjectModule" key.
    /// </remarks>
    public class ProjectModuleSettings
    {
        /// <summary>
        /// Gets or sets the default import map file path.
        /// </summary>
        /// <remarks>
        /// This map will be pre-selected when creating a new project.
        /// If null or file doesn't exist, the most recent map from the Maps folder is used.
        /// </remarks>
        public string? DefaultImportMapPath { get; set; }
    }
}
