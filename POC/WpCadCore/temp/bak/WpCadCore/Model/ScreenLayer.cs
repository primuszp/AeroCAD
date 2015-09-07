using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpCadCore.Controls;
using System.Windows;

namespace WpCadCore.Model
{
    class ScreenLayer : BaseLayer
    {
        static ScreenLayer()
        {
            DataContextProperty.OverrideMetadata(typeof(ScreenLayer),
                new FrameworkPropertyMetadata(typeof(ScreenLayer), FrameworkPropertyMetadataOptions.AffectsRender));
        }

        public RubberLine rubber = new RubberLine();

        public ScreenLayer()
            : base()
        {
            this.children.Add(rubber);
        }

        public override void InversionScaleUpdate()
        {
            this.rubber.ScaleUpdate(InversionScale);
        }

    }
}
