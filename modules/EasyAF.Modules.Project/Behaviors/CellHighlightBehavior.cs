using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Xaml.Behaviors;
using Serilog;

namespace EasyAF.Modules.Project.Behaviors
{
    /// <summary>
    /// Attached behavior that animates a cell background color highlight.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This behavior provides smooth color-morphing transitions for cell highlights,
    /// using the same pattern as the drag-drop glow effect.
    /// </para>
    /// <para>
    /// <strong>Animation Sequence:</strong>
    /// 1. Normal background ? Fade to glow color (500ms)
    /// 2. Hold at glow color (configurable delay)
    /// 3. Glow color ? Fade back to normal (1000ms)
    /// </para>
    /// <para>
    /// <strong>Usage:</strong>
    /// Bind the `IsHighlighted` property to a ViewModel bool property.
    /// When it becomes true, the highlight animation plays automatically.
    /// </para>
    /// </remarks>
    public class CellHighlightBehavior : Behavior<Border>
    {
        private Brush? _originalBackground;
        private bool _isHighlighting;
        private System.Windows.Threading.DispatcherTimer? _holdTimer;

        /// <summary>
        /// Identifies the IsHighlighted dependency property.
        /// </summary>
        public static readonly DependencyProperty IsHighlightedProperty =
            DependencyProperty.Register(
                nameof(IsHighlighted),
                typeof(bool),
                typeof(CellHighlightBehavior),
                new PropertyMetadata(false, OnIsHighlightedChanged));

        /// <summary>
        /// Identifies the IsNewData dependency property.
        /// </summary>
        public static readonly DependencyProperty IsNewDataProperty =
            DependencyProperty.Register(
                nameof(IsNewData),
                typeof(bool),
                typeof(CellHighlightBehavior),
                new PropertyMetadata(true));

        /// <summary>
        /// Identifies the HoldDuration dependency property.
        /// </summary>
        public static readonly DependencyProperty HoldDurationProperty =
            DependencyProperty.Register(
                nameof(HoldDuration),
                typeof(TimeSpan),
                typeof(CellHighlightBehavior),
                new PropertyMetadata(TimeSpan.FromSeconds(2)));

        /// <summary>
        /// Gets or sets whether the cell should be highlighted.
        /// </summary>
        public bool IsHighlighted
        {
            get => (bool)GetValue(IsHighlightedProperty);
            set => SetValue(IsHighlightedProperty, value);
        }

        /// <summary>
        /// Gets or sets whether this is New Data (true) or Old Data (false).
        /// Determines which glow color to use.
        /// </summary>
        public bool IsNewData
        {
            get => (bool)GetValue(IsNewDataProperty);
            set => SetValue(IsNewDataProperty, value);
        }

        /// <summary>
        /// Gets or sets how long to hold the glow color before fading back.
        /// </summary>
        public TimeSpan HoldDuration
        {
            get => (TimeSpan)GetValue(HoldDurationProperty);
            set => SetValue(HoldDurationProperty, value);
        }

        /// <summary>
        /// Attaches the behavior to the Border element.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            // Save original background
            _originalBackground = AssociatedObject.Background;

            Log.Debug("CellHighlightBehavior attached to {ElementName} (IsNewData={IsNewData})",
                AssociatedObject.Name ?? "unnamed Border", IsNewData);
        }

        /// <summary>
        /// Detaches event handlers when behavior is removed.
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            // Clean up timer
            _holdTimer?.Stop();
            _holdTimer = null;

            // Restore original background
            ResetHighlight();
        }

        /// <summary>
        /// Called when IsHighlighted property changes.
        /// </summary>
        private static void OnIsHighlightedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CellHighlightBehavior behavior && (bool)e.NewValue)
            {
                behavior.StartHighlight();
            }
        }

        /// <summary>
        /// Starts the highlight animation sequence.
        /// </summary>
        private void StartHighlight()
        {
            if (_isHighlighting)
                return; // Already highlighting

            _isHighlighting = true;

            // Save original background if not already saved
            if (_originalBackground == null)
            {
                _originalBackground = AssociatedObject.Background;
            }

            // Get glow brush from theme
            var glowBrushKey = IsNewData ? "NewDataGlowBrush" : "OldDataGlowBrush";
            var glowBrush = Application.Current.TryFindResource(glowBrushKey) as SolidColorBrush;

            if (glowBrush != null && _originalBackground is SolidColorBrush originalSolid)
            {
                // Create animated brush
                var animatedBrush = new SolidColorBrush(originalSolid.Color);
                AssociatedObject.Background = animatedBrush;

                // Phase 1: Fade in to glow color (500ms)
                var fadeIn = new ColorAnimation
                {
                    From = originalSolid.Color,
                    To = glowBrush.Color,
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                fadeIn.Completed += (s, e) =>
                {
                    // Phase 2: Hold at glow color
                    _holdTimer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = HoldDuration
                    };
                    _holdTimer.Tick += (ts, te) =>
                    {
                        _holdTimer?.Stop();
                        FadeBackToNormal();
                    };
                    _holdTimer.Start();
                };

                animatedBrush.BeginAnimation(SolidColorBrush.ColorProperty, fadeIn);

                Log.Verbose("Started highlight animation for {Zone} cell", IsNewData ? "New Data" : "Old Data");
            }
            else
            {
                Log.Warning("Could not find glow brush resource: {Key}", glowBrushKey);
                _isHighlighting = false;
            }
        }

        /// <summary>
        /// Fades the background from glow color back to normal.
        /// </summary>
        private void FadeBackToNormal()
        {
            if (AssociatedObject.Background is SolidColorBrush animatedBrush && _originalBackground is SolidColorBrush originalSolid)
            {
                // Phase 3: Fade back to original color (1000ms)
                var fadeOut = new ColorAnimation
                {
                    From = animatedBrush.Color,
                    To = originalSolid.Color,
                    Duration = TimeSpan.FromMilliseconds(1000),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                };

                fadeOut.Completed += (s, e) =>
                {
                    // Restore original brush reference
                    if (_originalBackground != null)
                    {
                        AssociatedObject.Background = _originalBackground;
                    }

                    _isHighlighting = false;

                    // Reset the IsHighlighted property (will be detected by ViewModel)
                    IsHighlighted = false;

                    Log.Verbose("Completed highlight animation for {Zone} cell", IsNewData ? "New Data" : "Old Data");
                };

                animatedBrush.BeginAnimation(SolidColorBrush.ColorProperty, fadeOut);
            }
            else
            {
                // Fallback: just restore
                ResetHighlight();
            }
        }

        /// <summary>
        /// Immediately resets the highlight to normal state.
        /// </summary>
        private void ResetHighlight()
        {
            _holdTimer?.Stop();

            if (_originalBackground != null)
            {
                AssociatedObject.Background = _originalBackground;
            }

            _isHighlighting = false;
        }
    }
}
