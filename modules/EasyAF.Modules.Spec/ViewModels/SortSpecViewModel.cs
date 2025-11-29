using System;
using EasyAF.Engine;
using Prism.Mvvm;
using Serilog;

namespace EasyAF.Modules.Spec.ViewModels
{
    /// <summary>
    /// ViewModel wrapper for SortSpec DTO.
    /// Provides UI-friendly properties for sorting configuration.
    /// </summary>
    public class SortSpecViewModel : BindableBase
    {
        private readonly SortSpec _sortSpec;
        private readonly Action _onChanged;
        private readonly Func<int, string> _getColumnHeader;
        private int _orderIndex;

        public SortSpecViewModel(SortSpec sortSpec, Action onChanged, Func<int, string> getColumnHeader, int orderIndex = 0)
        {
            _sortSpec = sortSpec ?? throw new ArgumentNullException(nameof(sortSpec));
            _onChanged = onChanged ?? throw new ArgumentNullException(nameof(onChanged));
            _getColumnHeader = getColumnHeader ?? throw new ArgumentNullException(nameof(getColumnHeader));
            _orderIndex = orderIndex;
        }

        /// <summary>
        /// Gets the display order index (1-based).
        /// </summary>
        public int OrderIndex
        {
            get => _orderIndex;
            set => SetProperty(ref _orderIndex, value);
        }

        /// <summary>
        /// Gets or sets the column number to sort by (1-based).
        /// </summary>
        public int Column
        {
            get => _sortSpec.Column;
            set
            {
                if (_sortSpec.Column != value)
                {
                    _sortSpec.Column = value;
                    _onChanged();
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(ColumnHeader));
                    RaisePropertyChanged(nameof(Summary));
                    Log.Debug("SortSpec Column changed to: {Column}", value);
                }
            }
        }

        /// <summary>
        /// Gets the column header name for the current column number.
        /// </summary>
        public string ColumnHeader => _getColumnHeader(Column);

        /// <summary>
        /// Gets or sets the sort direction (asc/desc).
        /// </summary>
        public string Direction
        {
            get => _sortSpec.Direction ?? "asc";
            set
            {
                if (_sortSpec.Direction != value)
                {
                    _sortSpec.Direction = value;
                    _onChanged();
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(Summary));
                    RaisePropertyChanged(nameof(DirectionDisplay));
                    Log.Debug("SortSpec Direction changed to: {Direction}", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether this is a numeric sort.
        /// </summary>
        public bool Numeric
        {
            get => _sortSpec.Numeric;
            set
            {
                if (_sortSpec.Numeric != value)
                {
                    _sortSpec.Numeric = value;
                    _onChanged();
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(Summary));
                    Log.Debug("SortSpec Numeric changed to: {Numeric}", value);
                }
            }
        }

        /// <summary>
        /// Gets the human-readable direction display text.
        /// </summary>
        public string DirectionDisplay => Direction == "desc" ? "Descending ?" : "Ascending ?";

        /// <summary>
        /// Gets a summary display of this sort (e.g., "Column 2 (Ascending)").
        /// </summary>
        public string Summary
        {
            get
            {
                var dir = Direction == "desc" ? "Descending" : "Ascending";
                var type = Numeric ? " [Numeric]" : "";
                return $"Column {Column} ({dir}){type}";
            }
        }

        /// <summary>
        /// Gets the underlying SortSpec DTO.
        /// </summary>
        public SortSpec SortSpec => _sortSpec;
        
        /// <summary>
        /// Manually raises PropertyChanged for a specific property name.
        /// Used when column headers change externally.
        /// </summary>
        public void RaisePropertyChanged(string propertyName)
        {
            base.RaisePropertyChanged(propertyName);
        }
    }
}
