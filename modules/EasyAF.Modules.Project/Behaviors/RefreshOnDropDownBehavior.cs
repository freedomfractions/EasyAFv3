using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace EasyAF.Modules.Project.Behaviors
{
    /// <summary>
    /// Behavior that refreshes a ViewModel's data when a ComboBox dropdown is opened.
    /// </summary>
    /// <remarks>
    /// This allows lazy-loading of ComboBox items without polling or background watchers.
    /// Useful for scenarios where the item source might change (e.g., files on disk).
    /// </remarks>
    public class RefreshOnDropDownBehavior : Behavior<ComboBox>
    {
        /// <summary>
        /// Dependency property for the refresh action to execute.
        /// </summary>
        public static readonly DependencyProperty RefreshActionProperty =
            DependencyProperty.Register(
                nameof(RefreshAction),
                typeof(System.Action),
                typeof(RefreshOnDropDownBehavior),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the action to execute when the dropdown opens.
        /// Typically bound to a ViewModel method like RefreshMappings.
        /// </summary>
        public System.Action? RefreshAction
        {
            get => (System.Action?)GetValue(RefreshActionProperty);
            set => SetValue(RefreshActionProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.DropDownOpened += OnDropDownOpened;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.DropDownOpened -= OnDropDownOpened;
        }

        private void OnDropDownOpened(object? sender, EventArgs e)
        {
            RefreshAction?.Invoke();
        }
    }
}
