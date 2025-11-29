using System;
using System.Windows.Input;
using Prism.Commands;
using Serilog;
using EasyAF.Engine;

namespace EasyAF.Modules.Spec.ViewModels
{
    /// <summary>
    /// Partial class containing empty message-related functionality for TableEditorViewModel.
    /// </summary>
    public partial class TableEditorViewModel
    {
        #region Empty Message Properties

        /// <summary>
        /// Gets or sets the empty message displayed when the table has no data.
        /// </summary>
        public string? EmptyMessage
        {
            get => _table.EmptyMessage;
            set
            {
                if (_table.EmptyMessage != value)
                {
                    _table.EmptyMessage = value;
                    RaisePropertyChanged();
                    _document.MarkDirty();
                    Log.Debug("EmptyMessage changed to: {Message}", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the empty message formatting panel is expanded.
        /// </summary>
        private bool _isEmptyFormattingExpanded;
        public bool IsEmptyFormattingExpanded
        {
            get => _isEmptyFormattingExpanded;
            set => SetProperty(ref _isEmptyFormattingExpanded, value);
        }

        #endregion

        #region Empty Formatting Properties

        /// <summary>
        /// Gets or sets the font name for empty message.
        /// </summary>
        public string? EmptyFontName
        {
            get => _table.EmptyFormatting?.FontName;
            set
            {
                EnsureEmptyFormatting();
                if (_table.EmptyFormatting!.FontName != value)
                {
                    _table.EmptyFormatting.FontName = value;
                    RaisePropertyChanged();
                    _document.MarkDirty();
                }
            }
        }

        /// <summary>
        /// Gets or sets the font size for empty message.
        /// </summary>
        public string EmptyFontSize
        {
            get => _table.EmptyFormatting?.FontSize?.ToString("F1") ?? string.Empty;
            set
            {
                EnsureEmptyFormatting();
                var newValue = double.TryParse(value, out var parsed) ? (double?)parsed : null;
                if (_table.EmptyFormatting!.FontSize != newValue)
                {
                    _table.EmptyFormatting.FontSize = newValue;
                    RaisePropertyChanged();
                    _document.MarkDirty();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the empty message is bold.
        /// </summary>
        public bool EmptyBold
        {
            get => _table.EmptyFormatting?.Bold ?? false;
            set
            {
                EnsureEmptyFormatting();
                if (_table.EmptyFormatting!.Bold != value)
                {
                    _table.EmptyFormatting.Bold = value;
                    RaisePropertyChanged();
                    _document.MarkDirty();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets whether the empty message is italic (placeholder - not yet in Engine spec).
        /// </summary>
        public bool EmptyItalic
        {
            get => false; // TODO: Add to EmptyMessageFormattingSpec when Engine updated
            set
            {
                // Placeholder for future implementation
                RaisePropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets whether the empty message is underlined (placeholder - not yet in Engine spec).
        /// </summary>
        public bool EmptyUnderline
        {
            get => false; // TODO: Add to EmptyMessageFormattingSpec when Engine updated
            set
            {
                // Placeholder for future implementation
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the horizontal alignment for empty message.
        /// </summary>
        public string? EmptyHorizontalAlignment
        {
            get => _table.EmptyFormatting?.HorizontalAlignment;
            set
            {
                EnsureEmptyFormatting();
                if (_table.EmptyFormatting!.HorizontalAlignment != value)
                {
                    _table.EmptyFormatting.HorizontalAlignment = value;
                    RaisePropertyChanged();
                    _document.MarkDirty();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the vertical alignment for empty message.
        /// </summary>
        public string? EmptyVerticalAlignment
        {
            get => _table.EmptyFormatting?.VerticalAlignment;
            set
            {
                EnsureEmptyFormatting();
                if (_table.EmptyFormatting!.VerticalAlignment != value)
                {
                    _table.EmptyFormatting.VerticalAlignment = value;
                    RaisePropertyChanged();
                    _document.MarkDirty();
                }
            }
        }

        /// <summary>
        /// Gets or sets the fill color for empty message (hex without #).
        /// </summary>
        public string? EmptyFill
        {
            get => _table.EmptyFormatting?.Fill;
            set
            {
                EnsureEmptyFormatting();
                // Remove # if present
                var cleanValue = value?.TrimStart('#');
                if (_table.EmptyFormatting!.Fill != cleanValue)
                {
                    _table.EmptyFormatting.Fill = cleanValue;
                    RaisePropertyChanged();
                    _document.MarkDirty();
                }
            }
        }

        /// <summary>
        /// Gets or sets the text color for empty message (hex without #).
        /// </summary>
        public string? EmptyTextColor
        {
            get => _table.EmptyFormatting?.TextColor;
            set
            {
                EnsureEmptyFormatting();
                // Remove # if present
                var cleanValue = value?.TrimStart('#');
                if (_table.EmptyFormatting!.TextColor != cleanValue)
                {
                    _table.EmptyFormatting.TextColor = cleanValue;
                    RaisePropertyChanged();
                    _document.MarkDirty();
                }
            }
        }

        #endregion

        #region Empty Message Commands

        /// <summary>
        /// Command to clear empty message formatting (reset to defaults).
        /// </summary>
        public ICommand ClearEmptyFormattingCommand { get; private set; } = null!;
        
        /// <summary>
        /// Command to choose font for empty message.
        /// </summary>
        public ICommand ChooseEmptyFontCommand { get; private set; } = null!;
        
        /// <summary>
        /// Command to choose fill color for empty message.
        /// </summary>
        public ICommand ChooseEmptyFillColorCommand { get; private set; } = null!;
        
        /// <summary>
        /// Command to choose text color for empty message.
        /// </summary>
        public ICommand ChooseEmptyTextColorCommand { get; private set; } = null!;

        #endregion

        #region Empty Message Initialization

        /// <summary>
        /// Initializes empty message-related commands.
        /// Call this from the constructor.
        /// </summary>
        private void InitializeEmptyMessage()
        {
            ClearEmptyFormattingCommand = new DelegateCommand(ExecuteClearEmptyFormatting);
            ChooseEmptyFontCommand = new DelegateCommand(ExecuteChooseEmptyFont);
            ChooseEmptyFillColorCommand = new DelegateCommand(ExecuteChooseEmptyFillColor);
            ChooseEmptyTextColorCommand = new DelegateCommand(ExecuteChooseEmptyTextColor);
        }

        #endregion

        #region Empty Message Command Implementations

        private void ExecuteClearEmptyFormatting()
        {
            if (_table.EmptyFormatting == null) return;

            _table.EmptyFormatting.FontName = null;
            _table.EmptyFormatting.FontSize = null;
            _table.EmptyFormatting.HorizontalAlignment = null;
            _table.EmptyFormatting.VerticalAlignment = null;
            _table.EmptyFormatting.Bold = null;
            _table.EmptyFormatting.Fill = null;
            _table.EmptyFormatting.TextColor = null;

            RaisePropertyChanged(nameof(EmptyFontName));
            RaisePropertyChanged(nameof(EmptyFontSize));
            RaisePropertyChanged(nameof(EmptyHorizontalAlignment));
            RaisePropertyChanged(nameof(EmptyVerticalAlignment));
            RaisePropertyChanged(nameof(EmptyBold));
            RaisePropertyChanged(nameof(EmptyFill));
            RaisePropertyChanged(nameof(EmptyTextColor));

            _document.MarkDirty();

            Log.Information("Cleared empty message formatting for table {TableId}", _table.Id);
        }
        
        private void ExecuteChooseEmptyFont()
        {
            try
            {
                using var dialog = new System.Windows.Forms.FontDialog
                {
                    ShowColor = false,
                    ShowEffects = false
                };
                
                // Set current font if available
                if (!string.IsNullOrWhiteSpace(EmptyFontName))
                {
                    try
                    {
                        var fontSizeStr = EmptyFontSize;
                        var fontSize = string.IsNullOrWhiteSpace(fontSizeStr) ? 10f : float.Parse(fontSizeStr);
                        dialog.Font = new System.Drawing.Font(EmptyFontName, fontSize);
                    }
                    catch
                    {
                        // Use default if parsing fails
                    }
                }
                
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    EmptyFontName = dialog.Font.Name;
                    EmptyFontSize = dialog.Font.Size.ToString("F1");
                    
                    Log.Information("Selected font for empty message: {Font} {Size}pt", 
                        dialog.Font.Name, dialog.Font.Size);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open font chooser");
                _dialogService.ShowError("Failed to open font chooser", ex.Message);
            }
        }
        
        private void ExecuteChooseEmptyFillColor()
        {
            try
            {
                using var dialog = new System.Windows.Forms.ColorDialog
                {
                    FullOpen = true,
                    AnyColor = true
                };
                
                // Set current color if available
                if (!string.IsNullOrWhiteSpace(EmptyFill))
                {
                    try
                    {
                        var color = System.Drawing.ColorTranslator.FromHtml("#" + EmptyFill);
                        dialog.Color = color;
                    }
                    catch
                    {
                        // Use default if parsing fails
                    }
                }
                
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // Convert to hex without #
                    EmptyFill = dialog.Color.R.ToString("X2") + 
                               dialog.Color.G.ToString("X2") + 
                               dialog.Color.B.ToString("X2");
                    
                    Log.Information("Selected fill color for empty message: #{Color}", EmptyFill);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open color chooser");
                _dialogService.ShowError("Failed to open color chooser", ex.Message);
            }
        }
        
        private void ExecuteChooseEmptyTextColor()
        {
            try
            {
                using var dialog = new System.Windows.Forms.ColorDialog
                {
                    FullOpen = true,
                    AnyColor = true
                };
                
                // Set current color if available
                if (!string.IsNullOrWhiteSpace(EmptyTextColor))
                {
                    try
                    {
                        var color = System.Drawing.ColorTranslator.FromHtml("#" + EmptyTextColor);
                        dialog.Color = color;
                    }
                    catch
                    {
                        // Use default if parsing fails
                    }
                }
                
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // Convert to hex without #
                    EmptyTextColor = dialog.Color.R.ToString("X2") + 
                                    dialog.Color.G.ToString("X2") + 
                                    dialog.Color.B.ToString("X2");
                    
                    Log.Information("Selected text color for empty message: #{Color}", EmptyTextColor);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open color chooser");
                _dialogService.ShowError("Failed to open color chooser", ex.Message);
            }
        }

        #endregion

        #region Empty Message Helper Methods

        /// <summary>
        /// Ensures EmptyFormatting object exists on the table spec.
        /// </summary>
        private void EnsureEmptyFormatting()
        {
            if (_table.EmptyFormatting == null)
            {
                _table.EmptyFormatting = new EmptyMessageFormattingSpec();
            }
        }

        #endregion
    }
}
