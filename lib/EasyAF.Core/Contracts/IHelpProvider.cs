using System.Collections.Generic;

namespace EasyAF.Core.Contracts;

/// <summary>
/// Optional interface a module can implement to provide help pages (Markdown) for aggregation by the shell.
/// </summary>
/// <remarks>
/// Backward-compatible: modules are not required to implement this. The shell will detect and aggregate pages when present.
/// </remarks>
public interface IHelpProvider
{
    /// <summary>
    /// Returns the collection of help page descriptors contributed by the module.
    /// </summary>
    /// <returns>Enumerable of <see cref="HelpPageDescriptor"/> items.</returns>
    IEnumerable<HelpPageDescriptor> GetHelpPages();
}

/// <summary>
/// Descriptor for a single help page provided by a module.
/// </summary>
/// <param name="Id">Stable unique id (e.g., "map.intro").</param>
/// <param name="Title">User-visible page title.</param>
/// <param name="Category">Logical category/group (e.g., "Getting Started", "Mapping").</param>
/// <param name="ResourcePath">Assembly resource path or relative file path to embedded Markdown.</param>
/// <param name="Keywords">Optional keywords for search weighting.</param>
public record HelpPageDescriptor(string Id, string Title, string Category, string ResourcePath, string[]? Keywords = null);
