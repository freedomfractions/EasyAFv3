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
        private Storyboard? _glowInAnimation;
        private Storyboard? _glowOutAnimation;
        private Border? _targetBorder; // The actual background border to glow

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

            // Create animations for glow effect
            CreateGlowAnimations();

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
        /// Creates glow-in and glow-out animations for drag feedback.
        /// </summary>
        private void CreateGlowAnimations()
        {
            // Glow-in animation (fade in border + increase thickness)
            _glowInAnimation = new Storyboard();
            var opacityIn = new DoubleAnimation
            {
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTargetProperty(opacityIn, new PropertyPath("Opacity"));
            _glowInAnimation.Children.Add(opacityIn);

            // Glow-out animation (fade out border + restore thickness)
            _glowOutAnimation = new Storyboard();
            var opacityOut = new DoubleAnimation
            {
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTargetProperty(opacityOut, new PropertyPath("Opacity"));
            _glowOutAnimation.Children.Add(opacityOut);
        }

        /// <summary>
        /// Handles drag enter - shows glow if file is valid.
        /// </summary>
        private void OnDragEnter(object sender, DragEventArgs e)
        {
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
        /// Handles drag leave - hides glow.
        /// </summary>
        private void OnDragLeave(object sender, DragEventArgs e)
        {
            HideGlow();
            e.Handled = true;

            Log.Verbose("Drag leave on {Zone} drop zone", IsNewData ? "New Data" : "Old Data");
        }

        /// <summary>
        /// Handles file drop - triggers import.
        /// </summary>
        private void OnDrop(object sender, DragEventArgs e)
        {
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

            // Save original state
            _originalBorderBrush = _targetBorder.BorderBrush;
            _originalBorderThickness = _targetBorder.BorderThickness;

            // Get glow brush from theme
            var glowBrushKey = IsNewData ? "NewDataGlowBrush" : "OldDataGlowBrush";
            var glowBrush = Application.Current.TryFindResource(glowBrushKey) as Brush;

            if (glowBrush != null)
            {
                _targetBorder.BorderBrush = glowBrush;
                _targetBorder.BorderThickness = new Thickness(3);
                
                // Animate glow in
                if (_glowInAnimation != null)
                {
                    _glowInAnimation.Begin(_targetBorder);
                }
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
            if (_targetBorder == null || _originalBorderBrush == null)
                return; // Never showed glow or no target

            // Animate glow out
            if (_glowOutAnimation != null)
            {
                _glowOutAnimation.Completed += (s, e) =>
                {
                    // Restore original appearance after animation
                    if (_targetBorder != null)
                    {
                        _targetBorder.BorderBrush = _originalBorderBrush;
                        _targetBorder.BorderThickness = _originalBorderThickness;
                    }
                    _originalBorderBrush = null;
                };
                _glowOutAnimation.Begin(_targetBorder);
            }
            else
            {
                // No animation - restore immediately
                _targetBorder.BorderBrush = _originalBorderBrush;
                _targetBorder.BorderThickness = _originalBorderThickness;
                _originalBorderBrush = null;
            }
        }
    }
}
