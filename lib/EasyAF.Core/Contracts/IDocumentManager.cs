using System.Collections.ObjectModel;

namespace EasyAF.Core.Contracts;

/// <summary>
/// Decision returned by a close confirmation for a dirty document.
/// </summary>
public enum DocumentCloseDecision
{
    Save,
    Discard,
    Cancel
}

/// <summary>
/// Manages open documents and active document switching.
/// </summary>
public interface IDocumentManager
{
    /// <summary>
    /// Gets the collection of open documents in display order.
    /// </summary>
    ObservableCollection<IDocument> OpenDocuments { get; }

    /// <summary>
    /// Gets or sets the active document.
    /// </summary>
    IDocument? ActiveDocument { get; set; }

    /// <summary>
    /// Opens a document from the specified path using an appropriate module.
    /// </summary>
    IDocument OpenDocument(string path);

    /// <summary>
    /// Creates a new document using the specified module.
    /// </summary>
    IDocument CreateNewDocument(IDocumentModule module);

    /// <summary>
    /// Saves the document. If <paramref name="path"/> is null, uses the document's existing path.
    /// </summary>
    bool SaveDocument(IDocument document, string? path = null);

    /// <summary>
    /// Attempts to close a document. If the document is dirty, an optional confirmation callback can be provided.
    /// </summary>
    /// <param name="document">Document to close.</param>
    /// <param name="confirmClose">Callback to confirm closing a dirty document.</param>
    /// <returns>True if closed; false if canceled or failed.</returns>
    bool CloseDocument(IDocument document, Func<IDocument, DocumentCloseDecision>? confirmClose = null);

    /// <summary>
    /// Occurs when the active document has changed.
    /// </summary>
    event EventHandler<IDocument?>? ActiveDocumentChanged;

    /// <summary>
    /// Occurs when a document is opened.
    /// </summary>
    event EventHandler<IDocument>? DocumentOpened;

    /// <summary>
    /// Occurs when a document is closed.
    /// </summary>
    event EventHandler<IDocument>? DocumentClosed;
}
