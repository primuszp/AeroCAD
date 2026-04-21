using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Rendering;
using Primusz.AeroCAD.Core.Documents;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Documents
{
    public class CadDocumentServiceLayerMoveTests
    {
        [Fact]
        public void AddEntity_MovesEntityBetweenLayers()
        {
            Exception failure = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var renderService = new CaptureRenderService();
                    var document = new CadDocumentService(renderService);
                    var source = document.CreateLayer("Source", Colors.Red);
                    var target = document.CreateLayer("Target", Colors.Cyan);
                    var line = new Line(new Point(0, 0), new Point(10, 10));

                    document.AddEntity(source.Id, line);
                    Assert.Same(source, document.GetLayerForEntity(line));

                    document.AddEntity(target.Id, line);
                    Assert.Same(target, document.GetLayerForEntity(line));
                    Assert.DoesNotContain(line, source.Entities);
                    Assert.Contains(line, target.Entities);
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

        private sealed class CaptureRenderService : IEntityRenderService
        {
            public void Render(Entity entity, Layer layer, EntityVisual visual)
            {
            }

            public void InvalidateLayerCache(Layer layer)
            {
            }
        }
    }
}
