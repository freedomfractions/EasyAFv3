using EasyAF.Core.Contracts;

namespace EasyAF.Shell.Extensions;

/// <summary>
/// Extension methods for module metadata.
/// </summary>
public static class ModuleExtensions
{
    /// <summary>
    /// Gets a user-friendly description for a document module.
    /// </summary>
    public static string GetModuleDescription(this EasyAF.Core.Contracts.IModule module)
    {
        // Convention: Check if module has a Description property via reflection (optional)
        var descProp = module.GetType().GetProperty("Description");
        if (descProp != null && descProp.PropertyType == typeof(string))
        {
            var desc = descProp.GetValue(module) as string;
            if (!string.IsNullOrWhiteSpace(desc))
                return desc;
        }
        
        // Fallback to sensible defaults based on module name
        return module.ModuleName switch
        {
            "Project Editor" => "Create and manage EasyAF projects with import/export capabilities",
            "Map Editor" => "Design data mapping configurations for CSV and Excel imports",
            "Spec Editor" => "Define custom report specifications and table layouts",
            _ => $"Create new {module.ModuleName} documents"
        };
    }
}
