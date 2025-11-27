using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EasyAF.Modules.Map.Converters
{
    /// <summary>
    /// Converts a confidence score to Visibility (visible if score exists and >= threshold, collapsed otherwise).
    /// </summary>
    /// <remarks>
    /// Used to show/hide confidence badges based on whether a confidence score is available.
    /// </remarks>
    public class ConfidenceToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a confidence score to Visibility.
        /// </summary>
        /// <param name="value">The confidence score (double?).</param>
        /// <param name="targetType">The target type (Visibility).</param>
        /// <param name="parameter">Optional threshold (default 0.6).</param>
        /// <param name="culture">Culture info (unused).</param>
        /// <returns>Visible if confidence >= threshold, Collapsed otherwise.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double confidence)
                return Visibility.Collapsed; // No confidence = no badge

            // Parse threshold from parameter (default 0.6)
            double threshold = 0.6;
            if (parameter is string paramStr && double.TryParse(paramStr, out var parsed))
            {
                threshold = parsed;
            }

            return confidence >= threshold ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Not implemented (one-way binding only).
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConfidenceToVisibilityConverter is one-way only");
        }
    }
}
