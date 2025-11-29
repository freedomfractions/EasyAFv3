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
        /// Default: Arial
        /// </summary>
        public string? EmptyFontName
        {
            get => _table.EmptyFormatting?.FontName ?? "Arial";
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
        /// Default: 11
        /// </summary>
        public string EmptyFontSize
        {
            get
            {
                var fontSize = _table.EmptyFormatting?.FontSize;
                return fontSize.HasValue ? fontSize.Value.ToString("F1") : "11.0";
            }
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
        /// Default: false
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
        /// Default: false
        /// </summary>
        private bool _emptyItalic;
        public bool EmptyItalic
        {
            get => _emptyItalic;
            set
            {
                if (SetProperty(ref _emptyItalic, value))
                {
                    _document.MarkDirty();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets whether the empty message is underlined (placeholder - not yet in Engine spec).
        /// Default: false
        /// </summary>
        private bool _emptyUnderline;
        public bool EmptyUnderline
        {
            get => _emptyUnderline;
            set
            {
                if (SetProperty(ref _emptyUnderline, value))
                {
                    _document.MarkDirty();
                }
            }
        }

        /// <summary>
        /// Gets or sets the horizontal alignment for empty message.
        /// Default: center
        /// </summary>
        public string? EmptyHorizontalAlignment
        {
            get => _table.EmptyFormatting?.HorizontalAlignment ?? "center";
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
        /// Default: center (middle)
        /// </summary>
        public string? EmptyVerticalAlignment
        {
            get => _table.EmptyFormatting?.VerticalAlignment ?? "center";
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
        /// Default: FFFFFF (white)
        /// </summary>
        public string? EmptyFill
        {
            get => _table.EmptyFormatting?.Fill ?? "FFFFFF";
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
        /// Default: 000000 (black)
        /// </summary>
        public string? EmptyTextColor
        {
            get => _table.EmptyFormatting?.TextColor ?? "000000";
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

            // Reset to defaults instead of null
            _table.EmptyFormatting.FontName = "Arial";
            _table.EmptyFormatting.FontSize = 11.0;
            _table.EmptyFormatting.HorizontalAlignment = "center";
            _table.EmptyFormatting.VerticalAlignment = "center";
            _table.EmptyFormatting.Bold = false;
            _table.EmptyFormatting.Fill = "FFFFFF"; // White
            _table.EmptyFormatting.TextColor = "000000"; // Black

            // Reset italic and underline (local properties)
            EmptyItalic = false;
            EmptyUnderline = false;

            RaisePropertyChanged(nameof(EmptyFontName));
            RaisePropertyChanged(nameof(EmptyFontSize));
            RaisePropertyChanged(nameof(EmptyHorizontalAlignment));
            RaisePropertyChanged(nameof(EmptyVerticalAlignment));
            RaisePropertyChanged(nameof(EmptyBold));
            RaisePropertyChanged(nameof(EmptyFill));
            RaisePropertyChanged(nameof(EmptyTextColor));

            _document.MarkDirty();

            Log.Information("Reset empty message formatting to defaults for table {TableId}", _table.Id);
        }
        
        private void ExecuteSetEmptyAlignment(string? alignment)
        {
            if (string.IsNullOrWhiteSpace(alignment)) return;
            
            var parts = alignment.Split('-');
            if (parts.Length != 2) return;
            
            var vAlign = parts[0] switch
            {
                "top" => "top",
                "center" => "center",
                "bottom" => "bottom",
                _ => null
            };
            
            var hAlign = parts[1] switch
            {
                "left" => "left",
                "center" => "center",
                "right" => "right",
                _ => null
            };
            
            if (vAlign != null)
            {
                EmptyVerticalAlignment = vAlign;
            }
            
            if (hAlign != null)
            {
                EmptyHorizontalAlignment = hAlign;
            }
            
            Log.Debug("Set empty message alignment: V={VAlign} H={HAlign}", vAlign, hAlign);
        }
        
        private void ExecuteChooseEmptyFont()
        {
            try
            {
                using var dialog = new System.Windows.Forms.FontDialog
                {
                    ShowColor = false,
                    ShowEffects = true // Enable Bold, Italic, Underline
                };
                
                // Set current font (with defaults)
                try
                {
                    var fontName = EmptyFontName ?? "Arial";
                    var fontSizeStr = EmptyFontSize ?? "11.0";
                    var fontSize = float.TryParse(fontSizeStr, out var size) ? size : 11f;
                    var style = System.Drawing.FontStyle.Regular;
                    if (EmptyBold) style |= System.Drawing.FontStyle.Bold;
                    if (EmptyItalic) style |= System.Drawing.FontStyle.Italic;
                    if (EmptyUnderline) style |= System.Drawing.FontStyle.Underline;
                    
                    dialog.Font = new System.Drawing.Font(fontName, fontSize, style);
                }
                catch
                {
                    // Use default Arial 11 if parsing fails
                    dialog.Font = new System.Drawing.Font("Arial", 11f);
                }
                
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    EmptyFontName = dialog.Font.Name;
                    EmptyFontSize = dialog.Font.Size.ToString("F1");
                    EmptyBold = dialog.Font.Bold;
                    EmptyItalic = dialog.Font.Italic;
                    EmptyUnderline = dialog.Font.Underline;
                    
                    Log.Information("Selected font for empty message: {Font} {Size}pt Bold={Bold} Italic={Italic} Underline={Underline}", 
                        dialog.Font.Name, dialog.Font.Size, dialog.Font.Bold, dialog.Font.Italic, dialog.Font.Underline);
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
                
                // Set current color (with default white)
                try
                {
                    var fillColor = EmptyFill ?? "FFFFFF";
                    var color = System.Drawing.ColorTranslator.FromHtml("#" + fillColor);
                    dialog.Color = color;
                }
                catch
                {
                    // Use white if parsing fails
                    dialog.Color = System.Drawing.Color.White;
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
                
                // Set current color (with default black)
                try
                {
                    var textColor = EmptyTextColor ?? "000000";
                    var color = System.Drawing.ColorTranslator.FromHtml("#" + textColor);
                    dialog.Color = color;
                }
                catch
                {
                    // Use black if parsing fails
                    dialog.Color = System.Drawing.Color.Black;
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
