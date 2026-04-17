using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Layers;

namespace Primusz.AeroCAD.View.ViewModels
{
    public class LayerViewModel : ViewModelBase
    {
        private readonly Layer layer;
        private bool isActive;

        public LayerViewModel(Layer layer)
        {
            this.layer = layer;
        }

        public string Name
        {
            get => layer.LayerName;
            set { layer.LayerName = value; OnPropertyChanged(); }
        }

        public Color Color
        {
            get => layer.Color;
            set { layer.Color = value; OnPropertyChanged(); }
        }

        /// <summary>When true, new entities created by drawing tools go to this layer.</summary>
        public bool IsActive
        {
            get => isActive;
            set { isActive = value; OnPropertyChanged(); }
        }

        public Layer Layer => layer;
    }
}


