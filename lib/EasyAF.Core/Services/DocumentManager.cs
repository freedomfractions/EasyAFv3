using System.Collections.ObjectModel;
using System.IO;
using EasyAF.Core.Contracts;
using Serilog;
using System.Linq;

namespace EasyAF.Core.Services;

/// <summary>
/// Default implementation of the document manager handling open documents and state transitions.
/// </summary>
public class DocumentManager : IDocumentManager
{
    private readonly IModuleCatalog _catalog;
    private IDocument? _active;

    public DocumentManager(IModuleCatalog catalog)
    {
        _catalog = catalog;
        OpenDocuments = new ObservableCollection<IDocument>();
    }

    public ObservableCollection<IDocument> OpenDocuments { get; }

    public IDocument? ActiveDocument
    {
        get => _active;
        set
        {
            if (!ReferenceEquals(_active, value))
            {
                _active = value;
                ActiveDocumentChanged?.Invoke(this, _active);
            }
        }
    }

    public event EventHandler<IDocument?>? ActiveDocumentChanged;
    public event EventHandler<IDocument>? DocumentOpened;
    public event EventHandler<IDocument>? DocumentClosed;

    public IDocument OpenDocument(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path required", nameof(path));
        
        // Normalize the path for comparison
        var normalizedPath = Path.GetFullPath(path);
        
        // Check if document is already open
        var existingDoc = OpenDocuments.FirstOrDefault(d => 
            !string.IsNullOrEmpty(d.FilePath) && 
            string.Equals(Path.GetFullPath(d.FilePath), normalizedPath, StringComparison.OrdinalIgnoreCase));
        
        if (existingDoc != null)
        {
            // Document already open - just activate it
            ActiveDocument = existingDoc;
            Log.Information("Document already open, activated existing instance: {Path}", path);
            return existingDoc;
        }
        
        // Not open yet - proceed with opening
        var module = _catalog.FindModuleForFile(path);
        if (module is not IDocumentModule docModule)
            throw new InvalidOperationException($"No document module found for file: {path}");

        var doc = docModule.OpenDocument(path);
        OpenDocuments.Add(doc);
        ActiveDocument = doc;
        DocumentOpened?.Invoke(this, doc);
        Log.Information("Document opened: {Path}", path);
        return doc;
    }

    public IDocument CreateNewDocument(IDocumentModule module)
    {
        var doc = module.CreateNewDocument();
        OpenDocuments.Add(doc);
        ActiveDocument = doc;
        DocumentOpened?.Invoke(this, doc);
        Log.Information("New document created by module {Module}", module.ModuleName);
        return doc;
    }

    public bool SaveDocument(IDocument document, string? path = null)
    {
        if (document == null) return false;
        var module = document.OwnerModule;
        if (module is not IDocumentModule docModule) return false;
        try
        {
            docModule.SaveDocument(document, path ?? document.FilePath ?? throw new InvalidOperationException("No target path supplied"));
            document.MarkClean();
            Log.Information("Document saved: {Path}", path ?? document.FilePath);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save document {Title}", document.Title);
            return false;
        }
    }

    public bool CloseDocument(IDocument document, Func<IDocument, DocumentCloseDecision>? confirmClose = null)
    {
        if (document == null) return false;

        // Dirty check
        if (document.IsDirty && confirmClose != null)
        {
            var decision = confirmClose(document);
            if (decision == DocumentCloseDecision.Cancel) return false;
            if (decision == DocumentCloseDecision.Save)
            {
                if (!SaveDocument(document)) return false;
            }
        }

        var idx = OpenDocuments.IndexOf(document);
        if (idx >= 0)
        {
            OpenDocuments.RemoveAt(idx);
            if (ReferenceEquals(ActiveDocument, document))
            {
                ActiveDocument = OpenDocuments.Count > 0 ? OpenDocuments[Math.Min(idx, OpenDocuments.Count - 1)] : null;
            }
            DocumentClosed?.Invoke(this, document);
            Log.Information("Document closed: {Title}", document.Title);
            return true;
        }
        return false;
    }
}
