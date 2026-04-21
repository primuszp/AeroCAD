using System;
using System.Windows;
using System.Windows.Data;
using Primusz.AeroCAD.Core.Converters;

namespace Primusz.AeroCAD.Core.Drawing.Layers
{
    public class ScaleFrameworkElement : FrameworkElement
    {
        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register("Scale", typeof(double),
            typeof(VisualHost), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.None, ScalePropertyChanged));

        public double Scale
        {
            get
            {
                return (double)GetValue(ScaleProperty);
            }
            set
            {
                SetValue(ScaleProperty, value);
            }
        }

        public ScaleFrameworkElement()
        {
            Binding binding = new Binding("RenderTransform")
            {
                Source = this,
                Mode = BindingMode.OneWay,
                Converter = new TransformToScaleConverter()
            };
            SetBinding(ScaleProperty, binding);
        }

        private static void ScalePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ScaleFrameworkElement element = target as ScaleFrameworkElement;

            if (element != null)
            {
                element.ScaleUpdate();
            }
        }

        protected virtual void ScaleUpdate()
        { }
    }
}
