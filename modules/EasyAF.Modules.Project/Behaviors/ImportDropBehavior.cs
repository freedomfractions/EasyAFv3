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
            // Save original state
            _originalBorderBrush = AssociatedObject.BorderBrush;
            _originalBorderThickness = AssociatedObject.BorderThickness;

            // Get glow brush from theme
            var glowBrushKey = IsNewData ? "NewDataGlowBrush" : "OldDataGlowBrush";
            var glowBrush = Application.Current.TryFindResource(glowBrushKey) as Brush;

            if (glowBrush != null)
            {
                AssociatedObject.BorderBrush = glowBrush;
                AssociatedObject.BorderThickness = new Thickness(3);
                AssociatedObject.Opacity = 0;

                // Animate glow in
                _glowInAnimation?.Begin(AssociatedObject);
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
            if (_originalBorderBrush == null)
                return; // Never showed glow

            // Animate glow out
            if (_glowOutAnimation != null)
            {
                _glowOutAnimation.Completed += (s, e) =>
                {
                    // Restore original appearance after animation
                    AssociatedObject.BorderBrush = _originalBorderBrush;
                    AssociatedObject.BorderThickness = _originalBorderThickness;
                    _originalBorderBrush = null;
                };
                _glowOutAnimation.Begin(AssociatedObject);
            }
            else
            {
                // No animation - restore immediately
                AssociatedObject.BorderBrush = _originalBorderBrush;
                AssociatedObject.BorderThickness = _originalBorderThickness;
                _originalBorderBrush = null;
            }
        }
    }
}
