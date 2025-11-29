using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;

namespace EasyAF.Modules.Spec.Behaviors
{
    /// <summary>
    /// Adds watermark (ghost text) to a TextBox that appears when the text is empty.
    /// The watermark inherits the TextBox's formatting for live preview.
    /// </summary>
    public class WatermarkTextBoxBehavior : Behavior<TextBox>
    {
        public static readonly DependencyProperty WatermarkTextProperty =
            DependencyProperty.Register(
                nameof(WatermarkText),
                typeof(string),
                typeof(WatermarkTextBoxBehavior),
                new PropertyMetadata(string.Empty, OnWatermarkChanged));

        public string WatermarkText
        {
            get => (string)GetValue(WatermarkTextProperty);
            set => SetValue(WatermarkTextProperty, value);
        }

        private static void OnWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WatermarkTextBoxBehavior behavior && behavior.AssociatedObject != null)
            {
                behavior.UpdateWatermark();
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.TextChanged += OnTextChanged;
            AssociatedObject.GotFocus += OnGotFocus;
            AssociatedObject.LostFocus += OnLostFocus;
            AssociatedObject.Loaded += OnLoaded;
            
            // Try to update immediately if already loaded
            if (AssociatedObject.IsLoaded)
            {
                UpdateWatermark();
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.TextChanged -= OnTextChanged;
            AssociatedObject.GotFocus -= OnGotFocus;
            AssociatedObject.LostFocus -= OnLostFocus;
            AssociatedObject.Loaded -= OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Update watermark after the control is fully loaded
            UpdateWatermark();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateWatermark();
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            UpdateWatermark();
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            UpdateWatermark();
        }

        private void UpdateWatermark()
        {
            if (AssociatedObject == null) return;

            // Defer to next render pass to ensure adorner layer is ready
            Dispatcher.BeginInvoke(new System.Action(() =>
            {
                if (AssociatedObject == null) return;

                var adornerLayer = AdornerLayer.GetAdornerLayer(AssociatedObject);
                if (adornerLayer == null) return;

                // Remove existing watermark adorner
                var adorners = adornerLayer.GetAdorners(AssociatedObject);
                if (adorners != null)
                {
                    foreach (var adorner in adorners)
                    {
                        if (adorner is WatermarkAdorner)
                        {
                            adornerLayer.Remove(adorner);
                        }
                    }
                }

                // Add watermark if text is empty and not focused
                if (string.IsNullOrEmpty(AssociatedObject.Text) && !AssociatedObject.IsFocused)
                {
                    adornerLayer.Add(new WatermarkAdorner(AssociatedObject, WatermarkText));
                }
            }), System.Windows.Threading.DispatcherPriority.Render);
        }
    }

    /// <summary>
    /// Adorner that displays watermark text in a TextBox, inheriting the TextBox's formatting.
    /// </summary>
    internal class WatermarkAdorner : Adorner
    {
        private readonly TextBox _textBox;
        private readonly string _watermarkText;

        public WatermarkAdorner(TextBox textBox, string watermarkText) : base(textBox)
        {
            _textBox = textBox;
            _watermarkText = watermarkText;
            IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (string.IsNullOrEmpty(_watermarkText)) return;

            // Create formatted text that inherits from the TextBox
            var formattedText = new FormattedText(
                _watermarkText,
                System.Globalization.CultureInfo.CurrentCulture,
                _textBox.FlowDirection,
                new Typeface(_textBox.FontFamily, _textBox.FontStyle, _textBox.FontWeight, _textBox.FontStretch),
                _textBox.FontSize,
                CreateWatermarkBrush(),
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            formattedText.TextAlignment = _textBox.TextAlignment;

            // Calculate position based on TextBox padding and alignment
            var padding = _textBox.Padding;
            var availableWidth = _textBox.ActualWidth - padding.Left - padding.Right;
            var availableHeight = _textBox.ActualHeight - padding.Top - padding.Bottom;

            double x = padding.Left;
            double y = padding.Top;

            // Horizontal alignment is handled by FormattedText.TextAlignment
            // Vertical alignment
            if (_textBox.VerticalContentAlignment == VerticalAlignment.Center)
            {
                y = (_textBox.ActualHeight - formattedText.Height) / 2.0;
            }
            else if (_textBox.VerticalContentAlignment == VerticalAlignment.Bottom)
            {
                y = _textBox.ActualHeight - formattedText.Height - padding.Bottom;
            }

            // Set max width for text wrapping
            formattedText.MaxTextWidth = availableWidth;

            drawingContext.DrawText(formattedText, new Point(x, y));
        }

        private Brush CreateWatermarkBrush()
        {
            // Use the TextBox's foreground with full opacity (matching text color exactly)
            if (_textBox.Foreground is SolidColorBrush solidBrush)
            {
                var color = solidBrush.Color;
                // Use full opacity but ensure alpha is at least somewhat transparent for "ghost" effect
                // If the user wants 100% opacity, we'll use 100%, but typically we want a subtle difference
                return new SolidColorBrush(Color.FromArgb(
                    Math.Max((byte)1, color.A), // Keep original alpha (full opacity if text is full opacity)
                    color.R, 
                    color.G, 
                    color.B));
            }
            // Fallback to gray
            return new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
        }
    }
}
