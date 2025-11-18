Place DOCX template files (default report template, etc.) in this folder.

Folder name: Output File Templates

Any files here are:
1. Embedded as assembly resources (EasyAFExportLib)
2. Copied to the build output under the same relative path

Usage example (in code):

using var ms = TemplateResources.OpenTemplate("DefaultReport.docx");
TemplateResources.ExtractTemplateTo("DefaultReport.docx", Path.Combine(outputDir, "DefaultReport.docx"), overwrite:true);

Notes:
- File names must be unique (case-insensitive) across this folder tree; ambiguous short names will throw.
- Subfolders are supported; they are part of the manifest resource name.
- To list templates: var names = TemplateResources.ListDocxTemplateResourceNames();
