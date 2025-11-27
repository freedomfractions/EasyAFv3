using System;
using System.IO;
using Serilog;

namespace EasyAF.Core.Services
{
    /// <summary>
    /// Extension methods for safely accessing cross-module settings with fallback logic.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These extensions allow modules to safely read settings owned by other modules
    /// without creating hard dependencies. If a module isn't loaded or settings are
    /// corrupted, graceful fallbacks are used.
    /// </para>
    /// <para>
    /// <strong>Public Setting Keys (Cross-Module Contracts):</strong>
    /// - "Directories.Maps" ? Map module-owned, publicly readable
    /// - "Directories.Specs" ? Spec module-owned (future), publicly readable
    /// </para>
    /// <para>
    /// <strong>Module Ownership:</strong>
    /// - Only the owning module should WRITE these keys
    /// - Other modules can READ with fallback logic
    /// - Breaking changes in owner module won't crash consumers
    /// </para>
    /// </remarks>
    public static class CrossModuleSettingsExtensions
    {
        /// <summary>
        /// Gets the Maps directory with fallback to default location.
        /// </summary>
        /// <param name="settings">The settings service.</param>
        /// <returns>
        /// Path to Maps directory. Falls back to Documents\EasyAF\Maps if:
        /// - Map module not loaded
        /// - Setting key doesn't exist
        /// - Configured path doesn't exist
        /// - Any error occurs reading the setting
        /// </returns>
        /// <remarks>
        /// <para>
        /// <strong>Ownership:</strong> Map module owns "Directories.Maps" key.
        /// </para>
        /// <para>
        /// <strong>Fallback Strategy:</strong>
        /// 1. Try to read from "Directories.Maps" setting
        /// 2. If path exists, use it
        /// 3. If not found/invalid, use Documents\EasyAF\Maps
        /// 4. Log warning if fallback used
        /// </para>
        /// <para>
        /// <strong>Non-Breaking:</strong> Works even if Map module not loaded.
        /// </para>
        /// </remarks>
        public static string GetMapsDirectory(this Contracts.ISettingsService settings)
        {
            var defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "EasyAF",
                "Maps");

            try
            {
                var configured = settings.GetSetting("Directories.Maps", defaultPath);
                
                if (!string.IsNullOrEmpty(configured) && Directory.Exists(configured))
                {
                    Log.Debug("Using Maps directory from settings: {Path}", configured);
                    return configured;
                }
                
                // Configured path doesn't exist - use fallback
                if (!string.IsNullOrEmpty(configured))
                {
                    Log.Warning("Configured Maps directory not found: {Path} - using fallback: {Fallback}", 
                        configured, defaultPath);
                }
                
                return defaultPath;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to read Maps directory setting - using fallback: {Fallback}", defaultPath);
                return defaultPath;
            }
        }

        /// <summary>
        /// Gets the Specs directory with fallback to default location.
        /// </summary>
        /// <param name="settings">The settings service.</param>
        /// <returns>
        /// Path to Specs directory. Falls back to Documents\EasyAF\Specs if:
        /// - Spec module not loaded (future)
        /// - Setting key doesn't exist
        /// - Configured path doesn't exist
        /// - Any error occurs reading the setting
        /// </returns>
        /// <remarks>
        /// <para>
        /// <strong>Ownership:</strong> Spec module (future) owns "Directories.Specs" key.
        /// </para>
        /// <para>
        /// <strong>Fallback Strategy:</strong>
        /// 1. Try to read from "Directories.Specs" setting
        /// 2. If path exists, use it
        /// 3. If not found/invalid, use Documents\EasyAF\Specs
        /// 4. Log warning if fallback used
        /// </para>
        /// <para>
        /// <strong>Future-Ready:</strong> Prepared for Spec module arrival.
        /// </para>
        /// </remarks>
        public static string GetSpecsDirectory(this Contracts.ISettingsService settings)
        {
            var defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "EasyAF",
                "Specs");

            try
            {
                var configured = settings.GetSetting("Directories.Specs", defaultPath);
                
                if (!string.IsNullOrEmpty(configured) && Directory.Exists(configured))
                {
                    Log.Debug("Using Specs directory from settings: {Path}", configured);
                    return configured;
                }
                
                // Configured path doesn't exist - use fallback
                if (!string.IsNullOrEmpty(configured))
                {
                    Log.Warning("Configured Specs directory not found: {Path} - using fallback: {Fallback}", 
                        configured, defaultPath);
                }
                
                return defaultPath;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to read Specs directory setting - using fallback: {Fallback}", defaultPath);
                return defaultPath;
            }
        }

        /// <summary>
        /// Ensures a directory exists, creating it if necessary.
        /// </summary>
        /// <param name="path">Directory path to ensure exists.</param>
        /// <returns>The path (for fluent chaining).</returns>
        /// <remarks>
        /// Helper method to ensure fallback directories exist before use.
        /// </remarks>
        public static string EnsureDirectoryExists(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    Log.Debug("Created directory: {Path}", path);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to create directory: {Path}", path);
            }
            
            return path;
        }
    }
}
