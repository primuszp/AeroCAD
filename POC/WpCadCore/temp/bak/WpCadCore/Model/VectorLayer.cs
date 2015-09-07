using System;
using System.Collections;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WpCadCore.Converters;
using System.Collections.Generic;

namespace WpCadCore.Model
{
    class VectorLayer : BaseLayer
    {
        static VectorLayer()
        {
            DataContextProperty.OverrideMetadata(typeof(VectorLayer),
                new FrameworkPropertyMetadata(typeof(VectorLayer), FrameworkPropertyMetadataOptions.AffectsRender));
        }

        #region Public properties

        public bool Editable { get; set; }

        public double Thickness { get; set; }

        public ArrayList SelectedObjects { get; private set; }

        #endregion

        protected List<Grip> grips = new List<Grip>();

        public VectorLayer()
            : base()
        {
            this.Editable = true;
            this.Thickness = 1.5;
            this.SelectedObjects = new ArrayList();
        }

        public void AddChild(EntityBase entity)
        {
            if (entity == null) return;
            {
                entity.Refresh();
            }
            this.children.Add(entity);
        }

        public void RemoveChild(EntityBase entity)
        {
            if (entity == null) return;

            if (this.children.Contains(entity))
                this.children.Remove(entity);
        }

        public override void InversionScaleUpdate()
        {
            foreach (EntityBase entity in children)
            {
                entity.ScaleUpdate(InversionScale);
            }
        }

        public void ClearSelectedObjects()
        {
            if (this.SelectedObjects.Count > 0)
            {
                foreach (EntityBase entity in SelectedObjects)
                {
                    entity.IsSelected = false;
                    entity.Refresh();
                }
            }

            this.SelectedObjects.Clear();
            this.grips.Clear();
        }

        #region HitTest

        public virtual HitTestResultBehavior HitTestResult(HitTestResult result)
        {
            if (result.VisualHit is EntityBase)
            {
                if ((result.VisualHit as EntityBase).IsSelected == false)
                    this.SelectedObjects.Add(result.VisualHit);
            }

            if (result.VisualHit is Grip)
            {
                Grip current = result.VisualHit as Grip;
                current.IsSelected = true;
                current.Refresh();

                foreach (Grip grip in grips)
                {
                    if (grip != current && grip.IsSelected == true)
                    {
                        grip.IsSelected = false;
                        grip.Refresh();
                    }
                }
            }

            return HitTestResultBehavior.Continue;
        }

        public virtual HitTestFilterBehavior HitTestFilter(DependencyObject dobj)
        {
            if (dobj.GetType() == typeof(VectorLayer))
                return HitTestFilterBehavior.ContinueSkipSelf;
            else
                return HitTestFilterBehavior.Continue;
        }

        public virtual void HitTest(Point point)
        {
            //Point point = e.GetPosition(this);

            Geometry rectangle = new RectangleGeometry(new Rect(point.X - 6 * InversionScale,
                point.Y - 6 * InversionScale, 12 * InversionScale, 12 * InversionScale));

            VisualTreeHelper.HitTest(this,
                new HitTestFilterCallback(HitTestFilter),
                new HitTestResultCallback(HitTestResult),
                new GeometryHitTestParameters(rectangle));

            //VisualTreeHelper.HitTest(this,
            //    new HitTestFilterCallback(HitTestFilter),
            //    new HitTestResultCallback(HitTestResult),
            //    new PointHitTestParameters(point));

            if (SelectedObjects.Count > 0)
            {
                foreach (EntityBase entity in SelectedObjects)
                {
                    entity.IsSelected = true;
                    entity.PutGrips(grips);
                }
            }
        }

        #endregion
    }
}
