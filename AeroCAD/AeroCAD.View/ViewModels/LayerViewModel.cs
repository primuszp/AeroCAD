using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Layers;

namespace Primusz.AeroCAD.View.ViewModels
{
    public class LayerViewModel : ViewModelBase, IDisposable
    {
        private readonly Layer layer;
        private Brush colorBrush;
        private bool isActive;
        private bool disposed;

        public LayerViewModel(Layer layer)
        {
            this.layer = layer ?? throw new ArgumentNullException(nameof(layer));
            this.layer.Style.PropertyChanged += OnLayerStyleChanged;
            RefreshColorBrush();
        }

        public string Name
        {
            get => layer.LayerName;
            set
            {
                if (layer.LayerName == value)
                    return;

                layer.LayerName = value;
                OnPropertyChanged();
            }
        }

        public Color Color
        {
            get => layer.Color;
            set
            {
                if (layer.Color == value)
                    return;

                layer.Color = value;
                NotifyColorChanged();
            }
        }

        public Brush ColorBrush => colorBrush;

        public string ColorText
        {
            get => layer.Color.ToString();
            set
            {
                if (TryParseColor(value, out var color) && layer.Color != color)
                {
                    layer.Color = color;
                    NotifyColorChanged();
                }
            }
        }

        public LineStyle LineStyle
        {
            get => layer.Style.LineStyle;
            set
            {
                if (layer.Style.LineStyle == value)
                    return;

                layer.Style.LineStyle = value;
                OnPropertyChanged();
            }
        }

        public double LineWeight
        {
            get => layer.Style.LineWeight;
            set
            {
                if (Math.Abs(layer.Style.LineWeight - value) < 0.0001d)
                    return;

                layer.Style.LineWeight = value;
                OnPropertyChanged();
            }
        }

        public bool IsVisible
        {
            get => layer.Style.IsVisible;
            set
            {
                if (layer.Style.IsVisible == value)
                    return;

                layer.Style.IsVisible = value;
                NotifyCanBeActiveChanged();
            }
        }

        public bool IsFrozen
        {
            get => layer.Style.IsFrozen;
            set
            {
                if (layer.Style.IsFrozen == value)
                    return;

                layer.Style.IsFrozen = value;
                NotifyCanBeActiveChanged();
            }
        }

        public bool IsLocked
        {
            get => layer.Style.IsLocked;
            set
            {
                if (layer.Style.IsLocked == value)
                    return;

                layer.Style.IsLocked = value;
                NotifyCanBeActiveChanged();
            }
        }

        public bool CanBeActive => layer.IsRenderable;

        /// <summary>When true, new entities created by drawing tools go to this layer.</summary>
        public bool IsActive
        {
            get => isActive;
            set
            {
                if (isActive == value)
                    return;

                isActive = value;
                OnPropertyChanged();
            }
        }

        public Layer Layer => layer;

        public void SetActive(bool value)
        {
            IsActive = value;
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            layer.Style.PropertyChanged -= OnLayerStyleChanged;
        }

        private void OnLayerStyleChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(LayerStyle.Color):
                    NotifyColorChanged();
                    break;
                case nameof(LayerStyle.LineStyle):
                    OnPropertyChanged(nameof(LineStyle));
                    break;
                case nameof(LayerStyle.LineWeight):
                    OnPropertyChanged(nameof(LineWeight));
                    break;
                case nameof(LayerStyle.IsVisible):
                    NotifyCanBeActiveChanged();
                    break;
                case nameof(LayerStyle.IsFrozen):
                    NotifyCanBeActiveChanged();
                    break;
                case nameof(LayerStyle.IsLocked):
                    NotifyCanBeActiveChanged();
                    break;
            }
        }

        private static bool TryParseColor(string text, out Color color)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    color = Colors.Transparent;
                    return false;
                }

                object value = ColorConverter.ConvertFromString(text);
                if (value is Color parsed)
                {
                    color = parsed;
                    return true;
                }
            }
            catch
            {
                // Ignore invalid input and leave the current color unchanged.
            }

            color = Colors.Transparent;
            return false;
        }

        private void RefreshColorBrush()
        {
            var brush = new SolidColorBrush(layer.Color);
            if (brush.CanFreeze)
                brush.Freeze();

            colorBrush = brush;
            OnPropertyChanged(nameof(ColorBrush));
        }

        private void NotifyColorChanged()
        {
            OnPropertyChanged(nameof(Color));
            OnPropertyChanged(nameof(ColorText));
            RefreshColorBrush();
        }

        private void NotifyCanBeActiveChanged()
        {
            OnPropertyChanged(nameof(IsVisible));
            OnPropertyChanged(nameof(IsFrozen));
            OnPropertyChanged(nameof(IsLocked));
            OnPropertyChanged(nameof(CanBeActive));
        }
    }
}
