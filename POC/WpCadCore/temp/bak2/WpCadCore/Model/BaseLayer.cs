using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using WpCadCore.Converters;

namespace WpCadCore.Model
{
    class BaseLayer : FrameworkElement
    {
        static BaseLayer()
        {
            DataContextProperty.OverrideMetadata(typeof(BaseLayer),
                new FrameworkPropertyMetadata(typeof(BaseLayer), FrameworkPropertyMetadataOptions.AffectsRender));
        }

        #region InversionScale

        public static readonly DependencyProperty InversionScaleProperty = DependencyProperty.Register("InversionScale", typeof(double),
            typeof(BaseLayer), new FrameworkPropertyMetadata((double)1.0, FrameworkPropertyMetadataOptions.None, InversionScalePropertyChanged));

        public double InversionScale
        {
            get
            {
                return (double)this.GetValue(InversionScaleProperty);
            }
            set
            {
                this.SetValue(InversionScaleProperty, value);
            }
        }

        static void InversionScalePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            BaseLayer This = target as BaseLayer;
            if (This != null) This.InversionScaleUpdate();
        }

        #endregion

        protected VisualCollection children;

        public BaseLayer()
            : base()
        {
            this.SetBindings();
            this.children = new VisualCollection(this);
        }

        private void SetBindings()
        {
            Binding binding = new Binding("RenderTransform")
            {
                Source = this,
                Mode = BindingMode.OneWay,
                Converter = new TransformToScaleConverter()
            };
            SetBinding(InversionScaleProperty, binding);
        }

        public virtual void InversionScaleUpdate()
        { }

        protected override int VisualChildrenCount
        {
            get { return this.children.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= this.children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            return this.children[index];
        }
    }
}
