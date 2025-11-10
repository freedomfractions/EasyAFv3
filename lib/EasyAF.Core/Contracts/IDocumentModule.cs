using Fluent;

namespace EasyAF.Core.Contracts;

/// <summary>
/// Interface for modules that manage document-based content.
/// </summary>
/// <remarks>
/// Document modules can create, open, save, and provide UI for specific file types.
/// The shell uses this interface to delegate file operations to the appropriate module.
/// </remarks>
public interface IDocumentModule : IModule
{
    /// <summary>
    /// Creates a new blank document.
    /// </summary>
    /// <returns>A new IDocument instance ready for editing.</returns>
    /// <remarks>
    /// The document should not have a file path until it is saved for the first time.
    /// </remarks>
    IDocument CreateNewDocument();

    /// <summary>
    /// Opens an existing document from a file.
    /// </summary>
    /// <param name="filePath">The full path to the file to open.</param>
    /// <returns>An IDocument instance with the loaded content.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the file format is invalid.</exception>
    /// <remarks>
    /// The module is responsible for deserializing the file and validating its content.
    /// </remarks>
    IDocument OpenDocument(string filePath);

    /// <summary>
    /// Saves a document to a file.
    /// </summary>
    /// <param name="document">The document to save.</param>
    /// <param name="filePath">The full path where the document should be saved.</param>
    /// <exception cref="InvalidOperationException">Thrown when the document cannot be saved.</exception>
    /// <remarks>
    /// The module should serialize the document content and handle any file I/O errors.
    /// After successful save, the document should be marked as clean.
    /// </remarks>
    void SaveDocument(IDocument document, string filePath);

    /// <summary>
    /// Gets the ribbon tabs to display when this document type is active.
    /// </summary>
    /// <param name="activeDocument">The currently active document of this module's type.</param>
    /// <returns>Array of RibbonTabItem controls to inject into the main ribbon.</returns>
    /// <remarks>
    /// The shell will dynamically add/remove these tabs based on the active document.
    /// Commands in the ribbon should be bound to the document's ViewModel.
    /// </remarks>
    RibbonTabItem[] GetRibbonTabs(IDocument activeDocument);

    /// <summary>
    /// Determines whether this module can handle the specified file.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <returns>True if this module can open the file; otherwise, false.</returns>
    /// <remarks>
    /// Typically checks the file extension against SupportedFileExtensions.
    /// May also peek at file content for format validation.
    /// </remarks>
    bool CanHandleFile(string filePath);
}
