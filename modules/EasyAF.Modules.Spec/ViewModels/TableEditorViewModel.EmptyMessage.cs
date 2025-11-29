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
        public double? EmptyFontSize
        {
            get => _table.EmptyFormatting?.FontSize;
            set
            {
                EnsureEmptyFormatting();
                if (_table.EmptyFormatting!.FontSize != value)
                {
                    _table.EmptyFormatting.FontSize = value;
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

        #endregion

        #region Empty Message Initialization

        /// <summary>
        /// Initializes empty message-related commands.
        /// Call this from the constructor.
        /// </summary>
        private void InitializeEmptyMessage()
        {
            ClearEmptyFormattingCommand = new DelegateCommand(ExecuteClearEmptyFormatting);
        }

        #endregion

        #region Empty Message Command Implementations

        private void ExecuteClearEmptyFormatting()
        {
            _table.EmptyFormatting = null;
            
            // Raise property changed for all formatting properties
            RaisePropertyChanged(nameof(EmptyFontName));
            RaisePropertyChanged(nameof(EmptyFontSize));
            RaisePropertyChanged(nameof(EmptyBold));
            RaisePropertyChanged(nameof(EmptyHorizontalAlignment));
            RaisePropertyChanged(nameof(EmptyFill));
            RaisePropertyChanged(nameof(EmptyTextColor));
            
            _document.MarkDirty();
            
            Log.Information("Cleared empty message formatting for table {TableId}", _table.Id);
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
