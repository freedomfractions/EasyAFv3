namespace EasyAF.Core.Contracts;

/// <summary>
/// Describes a file type supported by a module for Open/Save dialogs and associations.
/// </summary>
/// <param name="Extension">File extension without dot (e.g., "ezmap").</param>
/// <param name="Description">Human-friendly description (e.g., "EasyAF Map Files").</param>
public readonly record struct FileTypeDefinition(string Extension, string Description);
