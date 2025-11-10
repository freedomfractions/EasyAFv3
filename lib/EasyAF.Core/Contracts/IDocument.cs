namespace EasyAF.Core.Contracts;

/// <summary>
/// Represents a document instance managed by a module.
/// </summary>
/// <remarks>
/// Documents track file state, modifications, and provide a common interface
/// for the shell to manage open documents regardless of their type.
/// </remarks>
public interface IDocument
{
    /// <summary>
    /// Gets the file path of the document.
    /// </summary>
    /// <value>
    /// Full path to the document file, or null if the document has never been saved.
    /// </value>
    string? FilePath { get; }

    /// <summary>
    /// Gets the display title for the document.
    /// </summary>
    /// <value>
    /// The title shown in tabs and window titles (e.g., filename or "Untitled").
    /// </value>
    string Title { get; }

    /// <summary>
    /// Gets a value indicating whether the document has unsaved changes.
    /// </summary>
    /// <value>
    /// True if the document has been modified since last save; otherwise, false.
    /// </value>
    bool IsDirty { get; }

    /// <summary>
    /// Gets the module that owns this document.
    /// </summary>
    /// <value>
    /// Reference to the IDocumentModule that created this document.
    /// </value>
    IDocumentModule OwnerModule { get; }

    /// <summary>
    /// Marks the document as having unsaved changes.
    /// </summary>
    void MarkDirty();

    /// <summary>
    /// Marks the document as saved (clears dirty flag).
    /// </summary>
    void MarkClean();
}
