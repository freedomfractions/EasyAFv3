using System.IO;
using System.Reflection;
using EasyAF.Core.Contracts;
using Serilog;

namespace EasyAF.Shell.Services;

/// <summary>
/// Loads raw markdown content for help pages provided by modules.
/// </summary>
public interface IHelpContentLoader
{
    /// <summary>
    /// Loads markdown text for the specified descriptor.
    /// </summary>
    /// <param name="descriptor">Help page descriptor.</param>
    /// <returns>Markdown content string or an error placeholder.</returns>
    string LoadContent(HelpPageDescriptor descriptor);
}

public class HelpContentLoader : IHelpContentLoader
{
    public string LoadContent(HelpPageDescriptor descriptor)
    {
        if (descriptor == null) return string.Empty;

        // Attempt to locate embedded resource first (ResourcePath treated as resource name suffix)
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var resourceName = asm.GetManifestResourceNames()
                    .FirstOrDefault(r => r.EndsWith(descriptor.ResourcePath, StringComparison.OrdinalIgnoreCase));
                if (resourceName != null)
                {
                    using var stream = asm.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    using (var reader = new StreamReader(stream))
                    {
                        var text = reader.ReadToEnd();
                        Log.Debug("Loaded help resource {Resource} for page {Id}", resourceName, descriptor.Id);
                        return text;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error searching resources in assembly {Assembly}", asm.FullName);
            }
        }

        // Fallback: treat ResourcePath as relative file path on disk
        try
        {
            if (File.Exists(descriptor.ResourcePath))
            {
                var text = File.ReadAllText(descriptor.ResourcePath);
                Log.Debug("Loaded help file {File} for page {Id}", descriptor.ResourcePath, descriptor.Id);
                return text;
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error reading help file {File}", descriptor.ResourcePath);
        }

        return $"# {descriptor.Title}\n\nHelp content not found for '{descriptor.ResourcePath}'.";
    }
}
