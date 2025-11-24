using System;

namespace EasyAF.Modules.Project.Models
{
    /// <summary>
    /// Represents a mapping file item in the dropdown.
    /// </summary>
    public class MappingFileItem
    {
        /// <summary>
        /// Gets or sets the display name shown in the dropdown.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the full file path to the mapping file.
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// Gets or sets whether this is the "Browse..." special item.
        /// </summary>
        public bool IsBrowseItem { get; set; }

        /// <summary>
        /// Gets or sets whether this is the custom file item (last browsed file).
        /// </summary>
        public bool IsCustomFileItem { get; set; }

        /// <summary>
        /// Gets or sets whether this mapping file is valid.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the software version from the map metadata.
        /// </summary>
        public string? SoftwareVersion { get; set; }

        /// <summary>
        /// Gets or sets the map name from the map metadata.
        /// </summary>
        public string? MapName { get; set; }

        /// <summary>
        /// Gets the formatted display string (SoftwareVersion - MapName) or just DisplayName.
        /// </summary>
        public string FormattedDisplayName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(SoftwareVersion) && !string.IsNullOrWhiteSpace(MapName))
                {
                    return $"{SoftwareVersion} - {MapName}";
                }
                return DisplayName;
            }
        }

        /// <summary>
        /// Creates the special "Browse..." item.
        /// </summary>
        public static MappingFileItem CreateBrowseItem()
        {
            return new MappingFileItem
            {
                DisplayName = "Browse...",
                FilePath = null,
                IsBrowseItem = true,
                IsCustomFileItem = false,
                IsValid = true
            };
        }

        /// <summary>
        /// Creates a "(None)" item for when no mapping is selected.
        /// </summary>
        public static MappingFileItem CreateNoneItem()
        {
            return new MappingFileItem
            {
                DisplayName = "(None)",
                FilePath = null,
                IsBrowseItem = false,
                IsCustomFileItem = false,
                IsValid = true
            };
        }

        /// <summary>
        /// Creates a "Custom File" item for the last browsed custom file.
        /// </summary>
        public static MappingFileItem CreateCustomFileItem(string filePath, string fileName, string? softwareVersion = null, string? mapName = null)
        {
            return new MappingFileItem
            {
                DisplayName = fileName,
                FilePath = filePath,
                IsBrowseItem = false,
                IsCustomFileItem = true,
                IsValid = true,
                SoftwareVersion = softwareVersion,
                MapName = mapName
            };
        }
    }
}
