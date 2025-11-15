using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Data;

namespace EasyAF.Shell.Comparers;

/// <summary>
/// Custom comparer for sorting DateCategory groups in the Recent Files list.
/// Ensures "Pinned" is always first, followed by date groups in chronological order.
/// </summary>
public class DateCategoryComparer : IComparer
{
    private static readonly string[] CategoryOrder = 
    {
        "Pinned",
        "Today",
        "Yesterday",
        "This Week",
        "Last Week",
        "Older"
    };

    public int Compare(object? x, object? y)
    {
        if (x is CollectionViewGroup groupX && y is CollectionViewGroup groupY)
        {
            var nameX = groupX.Name?.ToString() ?? string.Empty;
            var nameY = groupY.Name?.ToString() ?? string.Empty;

            var indexX = Array.IndexOf(CategoryOrder, nameX);
            var indexY = Array.IndexOf(CategoryOrder, nameY);

            // Handle unknown categories (put them at the end)
            if (indexX == -1) indexX = int.MaxValue;
            if (indexY == -1) indexY = int.MaxValue;

            return indexX.CompareTo(indexY);
        }

        return 0;
    }
}
