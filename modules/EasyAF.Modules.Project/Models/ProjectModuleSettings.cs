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
        
        /// <summary>
        /// Gets or sets the default Templates directory path.
        /// </summary>
        /// <remarks>
        /// CROSS-MODULE EDIT: 2025-01-27 Module Directory Settings Refactoring
        /// Modified for: Add Templates directory to Project module settings
        /// Related modules: Core (CrossModuleSettingsExtensions - future GetTemplatesDirectory())
        /// Rollback instructions: Remove this property
        /// 
        /// Default location for Word document templates used in report generation.
        /// If null or doesn't exist, defaults to Documents\EasyAF\Templates.
        /// </remarks>
        public string? TemplatesDirectory { get; set; }
        
        /// <summary>
        /// Gets or sets the default Output directory path.
        /// </summary>
        /// <remarks>
        /// CROSS-MODULE EDIT: 2025-01-27 Module Directory Settings Refactoring
        /// Modified for: Add Output directory to Project module settings
        /// Related modules: Core (CrossModuleSettingsExtensions - future GetOutputDirectory())
        /// Rollback instructions: Remove this property
        /// 
        /// Default location for generated reports and output files.
        /// If null or doesn't exist, defaults to Documents\EasyAF\Output.
        /// </remarks>
        public string? OutputDirectory { get; set; }
    }
}
