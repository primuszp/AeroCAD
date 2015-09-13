using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Data;
using WpfMapControlv2.Converters;

namespace WpfMapControlv2
{
    public class DrawingLayer : FrameworkElement
    {
        static DrawingLayer()
        {
            DataContextProperty.OverrideMetadata(typeof(DrawingLayer), new FrameworkPropertyMetadata(typeof(DrawingLayer), FrameworkPropertyMetadataOptions.AffectsRender));
        }

        #region InversionScale

        public static readonly DependencyProperty InversionScaleProperty = DependencyProperty.Register("InversionScale", typeof(double),
            typeof(DrawingLayer), new PropertyMetadata((double)1.0));

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

        #endregion

        #region ViewTransform

        public static readonly DependencyProperty ViewTransformProperty = DependencyProperty.Register("ViewTransform", typeof(MatrixTransform),
            typeof(DrawingLayer), new FrameworkPropertyMetadata(MatrixTransform.Identity, FrameworkPropertyMetadataOptions.AffectsRender));

        public MatrixTransform ViewTransform
        {
            get
            {
                return (MatrixTransform)this.GetValue(ViewTransformProperty);
            }
            set
            {
                this.SetValue(ViewTransformProperty, value);
            }
        }

        #endregion

        #region RenderedObjects

        public static readonly DependencyProperty RenderedObjectsProperty = DependencyProperty.Register("RenderedObjects", typeof(ObservableCollection<Drawing>),
          typeof(DrawingLayer), new FrameworkPropertyMetadata(default(ObservableCollection<Drawing>), FrameworkPropertyMetadataOptions.AffectsRender, RenderedObjectsPropertyChanged));

        public ObservableCollection<Drawing> RenderedObjects
        {
            get { return (ObservableCollection<Drawing>)GetValue(RenderedObjectsProperty); }
            set { SetValue(RenderedObjectsProperty, value); }
        }

        static void RenderedObjectsPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            DrawingLayer This = target as DrawingLayer;

            INotifyCollectionChanged newDrawingSource = e.NewValue as INotifyCollectionChanged;
            INotifyCollectionChanged oldDrawingSource = e.NewValue as INotifyCollectionChanged;

            if (oldDrawingSource != null)
                oldDrawingSource.CollectionChanged -= This.drawingsCollectionChanged;

            if (This != null && newDrawingSource != null)
                newDrawingSource.CollectionChanged += This.drawingsCollectionChanged;
        }

        void drawingsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.InvalidateVisual();
        }

        #endregion

        public DrawingLayer()
        {
            this.SetBindings();
        }

        protected override void OnRender(DrawingContext dc)
        {
            ObservableCollection<Drawing> renderedObject = DataContext as ObservableCollection<Drawing>;

            if (renderedObject == null) return;

            dc.PushTransform(ViewTransform);
            {
                foreach (Drawing drawing in renderedObject)
                {
                    GeometryDrawing gd = drawing as GeometryDrawing;

                    if (gd != null && gd.Pen != null)
                    {
                        Pen scaledPen = new Pen
                        {
                            Brush = gd.Pen.Brush
                            ,
                            Thickness = gd.Pen.Thickness * InversionScale
                            ,
                            StartLineCap = gd.Pen.StartLineCap
                            ,
                            EndLineCap = gd.Pen.EndLineCap
                            ,
                            DashCap = gd.Pen.DashCap
                            ,
                            LineJoin = gd.Pen.LineJoin
                            ,
                            MiterLimit = gd.Pen.MiterLimit
                            ,
                            DashStyle = gd.Pen.DashStyle
                        };
                        dc.DrawGeometry(gd.Brush, scaledPen, gd.Geometry);
                    }
                }
            }
            dc.Pop();
        }

        private void SetBindings()
        {
            Binding binding = new Binding(".") { Mode = BindingMode.OneWay };
            SetBinding(DrawingLayer.RenderedObjectsProperty, binding);

            binding = new Binding("ToViewTransform") { Mode = BindingMode.OneWay };
            SetBinding(ViewTransformProperty, binding);

            binding = new Binding("ToViewTransform")
            {
                Mode = BindingMode.OneWay,
                Converter = new TransformToScaleConverter()
            };
            SetBinding(InversionScaleProperty, binding);
        }
    }
}
