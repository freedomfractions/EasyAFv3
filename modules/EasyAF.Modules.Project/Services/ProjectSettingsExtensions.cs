using EasyAF.Core.Contracts;
using EasyAF.Modules.Project.Models;
using Newtonsoft.Json;

namespace EasyAF.Modules.Project.Services
{
    /// <summary>
    /// Extension methods for ISettingsService to handle Project Module settings.
    /// </summary>
    public static class ProjectSettingsExtensions
    {
        private const string SettingsKey = "ProjectModule";

        /// <summary>
        /// Gets the Project Module settings.
        /// </summary>
        /// <param name="settingsService">The settings service.</param>
        /// <returns>Current settings or defaults if not configured.</returns>
        public static ProjectModuleSettings GetProjectModuleSettings(this ISettingsService settingsService)
        {
            var json = settingsService.GetSetting(SettingsKey, string.Empty);
            
            if (string.IsNullOrWhiteSpace(json))
                return new ProjectModuleSettings();

            try
            {
                return JsonConvert.DeserializeObject<ProjectModuleSettings>(json) ?? new ProjectModuleSettings();
            }
            catch
            {
                return new ProjectModuleSettings();
            }
        }

        /// <summary>
        /// Saves the Project Module settings.
        /// </summary>
        /// <param name="settingsService">The settings service.</param>
        /// <param name="settings">The settings to save.</param>
        public static void SetProjectModuleSettings(this ISettingsService settingsService, ProjectModuleSettings settings)
        {
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            settingsService.SetSetting(SettingsKey, json);
        }
    }
}
