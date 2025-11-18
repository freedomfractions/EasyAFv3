using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EasyAF.Export
{
    /// <summary>
    /// Helper to access embedded Word template resources (.docx or .dotx). Files placed under the project folder
    /// 'Output Template Files' are embedded (EmbeddedResource) and can be materialized to disk or opened as streams.
    /// </summary>
    public static class TemplateResources
    {
        private static readonly Assembly ThisAssembly = typeof(TemplateResources).Assembly;
        private static readonly string[] ResourceNames = ThisAssembly.GetManifestResourceNames();

        /// <summary>
        /// Enumerate embedded resource names that appear to be Word templates (ends with .docx or .dotx)
        /// </summary>
        public static IEnumerable<string> ListTemplateResourceNames() =>
            ResourceNames.Where(r => r.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) || r.EndsWith(".dotx", StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Reads an embedded template resource into a MemoryStream. The caller owns the returned stream.
        /// Accepts either a short file name (Breaker.dotx / Report.docx) or full manifest name.
        /// </summary>
        public static MemoryStream OpenTemplate(string name)
        {
            var ext = Path.GetExtension(name);
            string requiredExt = string.Equals(ext, ".dotx", StringComparison.OrdinalIgnoreCase) ? ".dotx" : ".docx";
            var full = ResolveResourceName(name, requiredExt);
            using var s = ThisAssembly.GetManifestResourceStream(full) ?? throw new FileNotFoundException($"Embedded resource '{name}' not found (resolved '{full}').");
            var ms = new MemoryStream();
            s.CopyTo(ms);
            ms.Position = 0;
            return ms;
        }

        /// <summary>
        /// Extracts an embedded template (docx or dotx) to a destination file path. Overwrites if overwrite=true.
        /// </summary>
        public static string ExtractTemplateTo(string name, string destinationPath, bool overwrite = false)
        {
            var ext = Path.GetExtension(name);
            string requiredExt = string.Equals(ext, ".dotx", StringComparison.OrdinalIgnoreCase) ? ".dotx" : ".docx";
            var full = ResolveResourceName(name, requiredExt);
            if (File.Exists(destinationPath) && !overwrite)
                throw new IOException($"Destination file already exists: {destinationPath}");
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(destinationPath))!);
            using var s = ThisAssembly.GetManifestResourceStream(full) ?? throw new FileNotFoundException($"Embedded resource '{name}' not found (resolved '{full}').");
            using var fs = File.Create(destinationPath);
            s.CopyTo(fs);
            return destinationPath;
        }

        private static string ResolveResourceName(string name, string requiredExtension)
        {
            // If caller passed a full manifest name and it exists, return it directly.
            if (ResourceNames.Contains(name)) return name;

            var shortName = name;
            // If name already ends with .docx or .dotx leave as-is; otherwise append required extension
            if (!(shortName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) || shortName.EndsWith(".dotx", StringComparison.OrdinalIgnoreCase)))
                shortName += requiredExtension;

            var matches = ResourceNames.Where(r => r.EndsWith(shortName, StringComparison.OrdinalIgnoreCase)).ToList();
            if (matches.Count == 0)
                throw new FileNotFoundException($"No embedded resource ending with '{shortName}' found. Available: {string.Join(", ", ResourceNames)}");
            if (matches.Count > 1)
                throw new InvalidOperationException($"Multiple embedded resources match '{shortName}': {string.Join(", ", matches)}. Specify the full manifest name.");
            return matches[0];
        }
    }
}
