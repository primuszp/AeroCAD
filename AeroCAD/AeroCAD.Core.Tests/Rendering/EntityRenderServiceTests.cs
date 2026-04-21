using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Threading;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Rendering;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Rendering
{
    public class EntityRenderServiceTests
    {
        [Fact]
        public void Render_UsesLayerDashStyleAndLineWeight()
        {
            Exception failure = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var strategy = new CaptureLineStrategy();
                    var service = new EntityRenderService(new[] { strategy });
                    var layer = new Layer
                    {
                        Style =
                        {
                            Color = Colors.CornflowerBlue,
                            LineStyle = LineStyle.DotDash,
                            LineWeight = 2.5d
                        }
                    };
                    var line = new Line(new Point(0, 0), new Point(10, 0)) { Thickness = 1.2d };
                    var visual = new EntityVisual(line);

                    service.Render(line, layer, visual);

                    Assert.NotNull(strategy.Context);
                    Assert.Equal(DashStyles.DashDot, strategy.Context.Pen.DashStyle);
                    Assert.Equal(2.5d, strategy.Context.Pen.Thickness, 3);
                }
                catch (Exception ex)
                {
                    failure = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (failure != null)
                throw failure;
        }

        private sealed class CaptureLineStrategy : IEntityRenderStrategy
        {
            public EntityRenderContext Context { get; private set; }

            public bool CanHandle(Entity entity) => entity is Line;

            public void Render(Entity entity, DrawingContext drawingContext, EntityRenderContext context)
            {
                Context = context;
            }
        }
    }
}
