using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EasyAF.Modules.Map.Converters
{
    /// <summary>
    /// Converts a confidence score (0.0-1.0) to a color brush for badge display.
    /// </summary>
    /// <remarks>
    /// <para>
    /// CROSS-MODULE EDIT: 2025-01-26 Auto-Map Confidence Badges
    /// Modified for: Color-code confidence badges based on score thresholds
    /// Related modules: Map (DataTypeMappingView XAML)
    /// Rollback instructions: Remove this converter and associated XAML bindings
    /// </para>
    /// <para>
    /// Color Mapping:
    /// - null or &lt; 0.6: Transparent (no badge)
    /// - 0.6-0.89: Yellow/Orange (#FFA500) - Medium confidence
    /// - 0.9-1.0: Green (#28A745) - High confidence
    /// </para>
    /// </remarks>
    public class ConfidenceToColorConverter : IValueConverter
    {
        /// <summary>
        /// Converts a confidence score to a color brush.
        /// </summary>
        /// <param name="value">The confidence score (double? from 0.0 to 1.0).</param>
        /// <param name="targetType">The target type (Brush).</param>
        /// <param name="parameter">Optional parameter (unused).</param>
        /// <param name="culture">Culture info (unused).</param>
        /// <returns>A SolidColorBrush representing the confidence level.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double confidence)
                return Brushes.Transparent; // No confidence = no badge

            // High confidence: Green
            if (confidence >= 0.9)
                return new SolidColorBrush(Color.FromRgb(0x28, 0xA7, 0x45)); // Bootstrap success green

            // Medium/Low confidence: Orange/Yellow
            if (confidence >= 0.6)
                return new SolidColorBrush(Color.FromRgb(0xFF, 0xA5, 0x00)); // Orange

            // Below threshold: No badge
            return Brushes.Transparent;
        }

        /// <summary>
        /// Not implemented (one-way binding only).
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConfidenceToColorConverter is one-way only");
        }
    }
}
