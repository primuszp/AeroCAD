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
    class VectorLayer : BaseLayer, ISelectionService
    {
        static VectorLayer()
        {
            DataContextProperty.OverrideMetadata(typeof(VectorLayer),
                new FrameworkPropertyMetadata(typeof(VectorLayer), FrameworkPropertyMetadataOptions.AffectsRender));
        }

        #region Public properties

        public bool Editable { get; set; }

        public double Thickness { get; set; }

        public static Grip SelectedGrip { get; private set; }

        public IList<ISelectable> SelectedObjects { get; private set; }

        #endregion

        protected bool missed = true;

        public VectorLayer()
            : base()
        {
            this.Editable = true;
            this.Thickness = 1.5;
            this.SelectedObjects = new List<ISelectable>();
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
            foreach (var child in children)
            {
                if (child is EntityBase)
                    (child as EntityBase).ScaleUpdate(InversionScale);
            }
        }

        #region HitTest

        public virtual HitTestResultBehavior HitTestResult(HitTestResult result)
        {
            this.missed = false;

            if (result.VisualHit is ISelectable)
            {
                ISelectable sobj = result.VisualHit as ISelectable;

                if (!this.SelectedObjects.Contains(sobj))
                {
                    if (sobj is Grip)
                    {
                        if (SelectedGrip != null)
                            SelectedGrip.Unselect();

                        SelectedGrip = sobj as Grip;
                        SelectedGrip.Select();
                    }
                    else
                        this.SelectedObjects.Add(sobj);
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

        #endregion

        #region ISelectionService Members

        public void ClearSelection()
        {
            if (this.SelectedObjects.Count > 0)
            {
                SelectedGrip = null;
                foreach (ISelectable sobj in SelectedObjects)
                {
                    sobj.Unselect();
                }
            }
            this.SelectedObjects.Clear();
        }

        public bool HitTest(Point point)
        {
            Rect rectangle = new Rect(point.X - 6 * InversionScale, point.Y - 6 * InversionScale, 12 * InversionScale, 12 * InversionScale);
            return this.HitTest(rectangle);
        }

        public bool HitTest(Rect rect)
        {
            this.missed = true;
            Geometry rectangle = new RectangleGeometry(rect);

            VisualTreeHelper.HitTest(this,
                new HitTestFilterCallback(HitTestFilter),
                new HitTestResultCallback(HitTestResult),
                new GeometryHitTestParameters(rectangle));

            if (SelectedObjects.Count > 0)
            {
                foreach (ISelectable sobj in SelectedObjects)
                {
                    if (sobj.IsSelected == false)
                    {
                        sobj.Select();
                        return true;
                    }
                }
            }
            return !missed;
        }

        #endregion
    }
}
