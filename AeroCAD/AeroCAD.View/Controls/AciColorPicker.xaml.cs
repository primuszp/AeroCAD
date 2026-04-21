using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.View.Controls
{
    public partial class AciColorPicker : UserControl
    {
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(
                nameof(SelectedColor),
                typeof(Color),
                typeof(AciColorPicker),
                new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));

        public static readonly DependencyProperty SelectedColorIndexProperty =
            DependencyProperty.Register(
                nameof(SelectedColorIndex),
                typeof(byte),
                typeof(AciColorPicker),
                new FrameworkPropertyMetadata((byte)7, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorIndexChanged));

        private bool paletteLoaded;
        private bool updatingColor;

        public AciColorPicker()
        {
            InitializeComponent();
        }

        public Color SelectedColor
        {
            get => (Color)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public byte SelectedColorIndex
        {
            get => (byte)GetValue(SelectedColorIndexProperty);
            set => SetValue(SelectedColorIndexProperty, value);
        }

        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (AciColorPicker)d;
            if (picker.updatingColor) return;
            picker.updatingColor = true;
            try
            {
                var nextIndex = AciPalette.GetIndex((Color)e.NewValue);
                if (picker.SelectedColorIndex != nextIndex)
                    picker.SelectedColorIndex = nextIndex;
            }
            finally { picker.updatingColor = false; }
        }

        private static void OnSelectedColorIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (AciColorPicker)d;
            if (picker.updatingColor) return;
            picker.updatingColor = true;
            try
            {
                var nextColor = AciPalette.GetColor((byte)e.NewValue);
                if (picker.SelectedColor != nextColor)
                    picker.SelectedColor = nextColor;
            }
            finally { picker.updatingColor = false; }
        }

        private void OpenPicker_Click(object sender, RoutedEventArgs e)
        {
            if (!paletteLoaded)
            {
                BuildPalette();
                paletteLoaded = true;
            }

            PickerPopup.IsOpen = !PickerPopup.IsOpen;
            e.Handled = true;
        }

        private void BuildPalette()
        {
            var stackPanel = new StackPanel { Width = 340 };
            stackPanel.Children.Add(new TextBlock
            {
                Text = "ACI",
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 6)
            });

            stackPanel.Children.Add(BuildColorRow(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 22));

            var grayRow = BuildColorRow(new byte[] { 250, 251, 252, 253, 254, 255 }, 22);
            grayRow.Margin = new Thickness(0, 4, 0, 0);
            stackPanel.Children.Add(grayRow);

            var fullGrid = new StackPanel { Margin = new Thickness(0, 6, 0, 0) };
            for (int r = 0; r < 10; r++)
            {
                int[] starts = { 18, 16, 14, 12, 10, 11, 13, 15, 17, 19 };
                var row = new WrapPanel();
                for (int i = 0; i < 24; i++)
                {
                    byte idx = (byte)(starts[r] + i * 10);
                    row.Children.Add(CreateColorButton(idx, 16));
                }
                fullGrid.Children.Add(row);
            }
            stackPanel.Children.Add(fullGrid);

            PickerPopup.Child = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0x12, 0x16, 0x1C)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x3B, 0x46, 0x57)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(6),
                Child = stackPanel
            };
        }

        private WrapPanel BuildColorRow(byte[] indices, int size)
        {
            var row = new WrapPanel();
            foreach (var idx in indices)
                row.Children.Add(CreateColorButton(idx, size));
            return row;
        }

        private Button CreateColorButton(byte index, int size)
        {
            var color = AciPalette.GetColor(index);
            var brush = new SolidColorBrush(color);
            if (brush.CanFreeze) brush.Freeze();

            var btn = new Button
            {
                Width = size,
                Height = size,
                Margin = new Thickness(1),
                Padding = new Thickness(0),
                Background = brush,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x3B, 0x46, 0x57)),
                BorderThickness = new Thickness(size == 22 ? 1 : 0.5),
                Tag = index
            };
            btn.Click += PaletteColor_Click;
            return btn;
        }

        private void PaletteColor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is byte index)
            {
                SelectedColorIndex = index;
                PickerPopup.IsOpen = false;
                e.Handled = true;
            }
        }
    }
}
