using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace Primusz.AeroCAD.Core.Drawing.Layers
{
    public sealed class LayerStyle : INotifyPropertyChanged
    {
        private Color color = Colors.White;
        private LineStyle lineStyle = LineStyle.Solid;
        private double lineWeight = LineWeightPalette.Default;
        private bool isVisible = true;
        private bool isFrozen;
        private bool isLocked;

        public event PropertyChangedEventHandler PropertyChanged;

        public Color Color
        {
            get => color;
            set => SetField(ref color, value);
        }

        public LineStyle LineStyle
        {
            get => lineStyle;
            set => SetField(ref lineStyle, value);
        }

        public double LineWeight
        {
            get => lineWeight;
            set => SetField(ref lineWeight, value);
        }

        public bool IsVisible
        {
            get => isVisible;
            set => SetField(ref isVisible, value);
        }

        public bool IsFrozen
        {
            get => isFrozen;
            set => SetField(ref isFrozen, value);
        }

        public bool IsLocked
        {
            get => isLocked;
            set => SetField(ref isLocked, value);
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
