using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Xaml.Behaviors;
using Serilog;

namespace EasyAF.Modules.Project.Behaviors
{
    /// <summary>
    /// Attached behavior that enables drag-and-drop import for data files.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This behavior provides visual feedback and handles file drop events
    /// for importing CSV/Excel files into New or Old datasets.
    /// </para>
    /// <para>
    /// <strong>Visual Feedback:</strong>
    /// - Glow effect on drag-over using theme colors (NewDataGlowBrush or OldDataGlowBrush)
    /// - Only accepts .csv, .xls, .xlsx files
    /// - Changes cursor to indicate valid/invalid drop target
    /// </para>
    /// <para>
    /// <strong>Usage:</strong>
    /// Attach to the Border element surrounding the data column in the statistics table.
    /// </para>
    /// </remarks>
    public class ImportDropBehavior : Behavior<Border>
    {
        private Brush? _originalBorderBrush;
        private Thickness _originalBorderThickness;
        private Border? _targetBorder; // The actual background border to glow
        private bool _isGlowing; // Track if we're currently showing glow
        private System.Windows.Threading.DispatcherTimer? _hideGlowTimer; // Debounce timer

        /// <summary>
        /// Identifies whether this drop zone is for New Data (true) or Old Data (false).
        /// </summary>
        public static readonly DependencyProperty IsNewDataProperty =
            DependencyProperty.Register(
                nameof(IsNewData),
                typeof(bool),
                typeof(ImportDropBehavior),
                new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets whether this drop zone imports to New Data (true) or Old Data (false).
        /// </summary>
        public bool IsNewData
        {
            get => (bool)GetValue(IsNewDataProperty);
            set => SetValue(IsNewDataProperty, value);
        }

        /// <summary>
        /// Identifies the ViewModel that handles the import operation.
        /// </summary>
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(ViewModels.ProjectSummaryViewModel),
                typeof(ImportDropBehavior),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the ProjectSummaryViewModel that will handle the import.
        /// </summary>
        public ViewModels.ProjectSummaryViewModel? ViewModel
        {
            get => (ViewModels.ProjectSummaryViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        /// <summary>
        /// Attaches drag-drop event handlers to the Border.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.AllowDrop = true;
            AssociatedObject.DragEnter += OnDragEnter;
            AssociatedObject.DragOver += OnDragOver;
            AssociatedObject.DragLeave += OnDragLeave;
            AssociatedObject.Drop += OnDrop;

            // Find the target background border to glow
            FindTargetBorder();

            Log.Debug("ImportDropBehavior attached to {ElementName} (IsNewData={IsNewData})", 
                AssociatedObject.Name ?? "unnamed Border", IsNewData);
        }

        /// <summary>
        /// Detaches event handlers when behavior is removed.
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.DragEnter -= OnDragEnter;
            AssociatedObject.DragOver -= OnDragOver;
            AssociatedObject.DragLeave -= OnDragLeave;
            AssociatedObject.Drop -= OnDrop;
            
            // Force reset to clean state
            ResetGlowState();
        }

        /// <summary>
        /// Finds the background border element to apply glow effects to.
        /// </summary>
        private void FindTargetBorder()
        {
            // Walk up the visual tree to find the parent Grid (StatisticsTableContainer)
            var parent = VisualTreeHelper.GetParent(AssociatedObject);
            while (parent != null && !(parent is Grid grid && grid.Name == "StatisticsTableContainer"))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            if (parent is Grid container)
            {
                // Find the background border by name
                var targetName = IsNewData ? "NewDataBackground" : "OldDataBackground";
                _targetBorder = FindChildByName<Border>(container, targetName);

                if (_targetBorder == null)
                {
                    Log.Warning("Could not find target border: {Name}", targetName);
                }
                else
                {
                    Log.Debug("Found target border: {Name}", targetName);
                }
            }
        }

        /// <summary>
        /// Finds a child element by name in the visual tree.
        /// </summary>
        private T? FindChildByName<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            if (parent == null) return null;

            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                
                if (child is T element && element.Name == name)
                    return element;

                var result = FindChildByName<T>(child, name);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Handles drag enter - shows glow if file is valid.
        /// </summary>
        private void OnDragEnter(object sender, DragEventArgs e)
        {
            // Cancel any pending hide
            _hideGlowTimer?.Stop();
            
            if (CanImportFile(e.Data))
            {
                ShowGlow();
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;

                Log.Verbose("Drag enter on {Zone} drop zone - valid file", IsNewData ? "New Data" : "Old Data");
            }
            else
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;

                Log.Verbose("Drag enter on {Zone} drop zone - invalid file", IsNewData ? "New Data" : "Old Data");
            }
        }

        /// <summary>
        /// Handles drag over - maintains cursor feedback.
        /// </summary>
        private void OnDragOver(object sender, DragEventArgs e)
        {
            // Cancel any pending hide (we're still dragging over)
            _hideGlowTimer?.Stop();
            
            if (CanImportFile(e.Data))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        /// <summary>
        /// Handles drag leave - hides glow after a short delay.
        /// </summary>
        private void OnDragLeave(object sender, DragEventArgs e)
        {
            // Don't hide immediately - use a timer to debounce
            // This prevents flickering when mouse moves between child elements
            if (_hideGlowTimer == null)
            {
                _hideGlowTimer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(100)
                };
                _hideGlowTimer.Tick += (s, args) =>
                {
                    _hideGlowTimer?.Stop();
                    HideGlow();
                };
            }
            
            _hideGlowTimer.Start();
            e.Handled = true;

            Log.Verbose("Drag leave on {Zone} drop zone (debounced)", IsNewData ? "New Data" : "Old Data");
        }

        /// <summary>
        /// Handles file drop - triggers import.
        /// </summary>
        private void OnDrop(object sender, DragEventArgs e)
        {
            // Cancel timer and hide immediately
            _hideGlowTimer?.Stop();
            HideGlow();

            if (!CanImportFile(e.Data))
            {
                e.Handled = true;
                return;
            }

            try
            {
                var files = GetDroppedFiles(e.Data);
                if (files == null || files.Length == 0)
                {
                    Log.Warning("Drop event had no valid files");
                    e.Handled = true;
                    return;
                }

                Log.Information("Files dropped on {Zone} drop zone: {Files}", 
                    IsNewData ? "New Data" : "Old Data", 
                    string.Join(", ", files.Select(Path.GetFileName)));

                // Trigger import via ViewModel
                if (ViewModel != null)
                {
                    ViewModel.ExecuteDropImport(files, IsNewData);
                }
                else
                {
                    Log.Error("ViewModel is null - cannot execute drop import");
                }

                e.Handled = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing drop on {Zone} drop zone", IsNewData ? "New Data" : "Old Data");
                e.Handled = true;
            }
        }

        /// <summary>
        /// Determines if the drag data contains importable files.
        /// </summary>
        private bool CanImportFile(IDataObject data)
        {
            if (!data.GetDataPresent(DataFormats.FileDrop))
                return false;

            var files = GetDroppedFiles(data);
            if (files == null || files.Length == 0)
                return false;

            // Check if all files have valid extensions
            var validExtensions = new[] { ".csv", ".xls", ".xlsx" };
            return files.All(file =>
            {
                var ext = Path.GetExtension(file).ToLowerInvariant();
                return validExtensions.Contains(ext);
            });
        }

        /// <summary>
        /// Extracts file paths from drag data.
        /// </summary>
        private string[]? GetDroppedFiles(IDataObject data)
        {
            if (!data.GetDataPresent(DataFormats.FileDrop))
                return null;

            return data.GetData(DataFormats.FileDrop) as string[];
        }

        /// <summary>
        /// Shows the glow effect using theme-appropriate color.
        /// </summary>
        private void ShowGlow()
        {
            if (_targetBorder == null)
            {
                Log.Warning("Cannot show glow - target border not found");
                return;
            }

            // Cancel any pending hide timer
            _hideGlowTimer?.Stop();

            // If already glowing, don't restart animation
            if (_isGlowing)
                return;

            _isGlowing = true;

            // Save original state ONLY on the very first glow
            if (_originalBorderBrush == null)
            {
                _originalBorderBrush = _targetBorder.BorderBrush;
                _originalBorderThickness = _targetBorder.BorderThickness;
                Log.Verbose("Saved original border state: Brush={Brush}, Thickness={Thickness}", 
                    _originalBorderBrush, _originalBorderThickness);
            }

            // Get glow brush from theme
            var glowBrushKey = IsNewData ? "NewDataGlowBrush" : "OldDataGlowBrush";
            var glowBrush = Application.Current.TryFindResource(glowBrushKey) as Brush;

            if (glowBrush != null)
            {
                // Stop any running animations
                _targetBorder.BeginAnimation(Border.OpacityProperty, null);

                // Apply glow brush and thicker border
                _targetBorder.BorderBrush = glowBrush;
                _targetBorder.BorderThickness = new Thickness(3);
                
                // Start from 0 opacity and fade in to 1.0 over 500ms
                var fadeIn = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                _targetBorder.BeginAnimation(Border.OpacityProperty, fadeIn);
            }
            else
            {
                Log.Warning("Could not find glow brush resource: {Key}", glowBrushKey);
            }
        }

        /// <summary>
        /// Hides the glow effect and restores original appearance.
        /// </summary>
        private void HideGlow()
        {
            if (_targetBorder == null || !_isGlowing)
                return; // Not currently glowing

            _isGlowing = false;

            // Stop any running animations on opacity
            _targetBorder.BeginAnimation(Border.OpacityProperty, null);
            
            // Ensure we're at full opacity
            _targetBorder.Opacity = 1.0;

            // Restore original thickness immediately
            _targetBorder.BorderThickness = _originalBorderThickness;

            // Animate the border color from glow back to original over 1 second
            if (_originalBorderBrush is SolidColorBrush originalSolid)
            {
                var glowBrushKey = IsNewData ? "NewDataGlowBrush" : "OldDataGlowBrush";
                var glowBrush = Application.Current.TryFindResource(glowBrushKey) as SolidColorBrush;

                if (glowBrush != null && _targetBorder.BorderBrush is SolidColorBrush)
                {
                    // Create a new SolidColorBrush for animation
                    var animatedBrush = new SolidColorBrush(glowBrush.Color);
                    _targetBorder.BorderBrush = animatedBrush;

                    // Animate color from glow to original
                    var colorAnimation = new ColorAnimation
                    {
                        From = glowBrush.Color,
                        To = originalSolid.Color,
                        Duration = TimeSpan.FromMilliseconds(1000),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                    };

                    colorAnimation.Completed += (s, e) =>
                    {
                        // Set back to the original brush reference after animation
                        if (_targetBorder != null && _originalBorderBrush != null)
                        {
                            _targetBorder.BorderBrush = _originalBorderBrush;
                        }
                    };

                    animatedBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
                }
                else
                {
                    // Fallback: just restore immediately
                    _targetBorder.BorderBrush = _originalBorderBrush;
                }
            }
            else
            {
                // Not a solid color brush - just restore immediately
                if (_originalBorderBrush != null)
                {
                    _targetBorder.BorderBrush = _originalBorderBrush;
                }
            }
        }

        /// <summary>
        /// Called when the fade-out animation completes.
        /// </summary>
        private void OnFadeOutCompleted(object? sender, EventArgs e)
        {
            // No longer needed - keeping for compatibility
        }

        /// <summary>
        /// Forces a reset of the glow state (for cleanup or error recovery).
        /// </summary>
        private void ResetGlowState()
        {
            _hideGlowTimer?.Stop();
            
            if (_targetBorder != null && _originalBorderBrush != null)
            {
                _targetBorder.BeginAnimation(Border.OpacityProperty, null);
                _targetBorder.BorderBrush = _originalBorderBrush;
                _targetBorder.BorderThickness = _originalBorderThickness;
                _targetBorder.Opacity = 1.0;
            }
            
            _isGlowing = false;
            // DON'T clear _originalBorderBrush - keep it for the lifetime of the behavior
        }
    }
}
