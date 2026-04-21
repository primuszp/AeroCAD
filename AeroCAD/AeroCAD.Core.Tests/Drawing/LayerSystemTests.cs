using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Rendering;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Drawing
{
    public class LayerSystemTests
    {
        [Fact]
        public void Layer_RenderEntity_RespectsVisibilityAndFrozenState()
        {
            Exception failure = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var renderService = new CaptureRenderService();
                    var layer = new Layer { RenderService = renderService };
                    var line = new Line(new Point(0, 0), new Point(100, 0));

                    layer.Add(line);
                    var baseline = renderService.RenderCount;

                    layer.RenderEntity(line);
                    Assert.Equal(baseline + 1, renderService.RenderCount);

                    layer.Style.IsVisible = false;
                    layer.RenderEntity(line);
                    Assert.Equal(baseline + 1, renderService.RenderCount);

                    layer.Style.IsVisible = true;
                    Assert.Equal(baseline + 2, renderService.RenderCount);

                    layer.Style.IsFrozen = true;
                    layer.RenderEntity(line);
                    Assert.Equal(baseline + 2, renderService.RenderCount);
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

        [Fact]
        public void CadDocumentService_RejectsEditsOnLockedLayer_ButAllowsLayerRemoval()
        {
            Exception failure = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var renderService = new CaptureRenderService();
                    var documentService = new CadDocumentService(renderService);
                    var layer = documentService.CreateLayer("Layer 1", Colors.White);
                    var line = new Line(new Point(0, 0), new Point(20, 20));

                    documentService.AddEntity(layer.Id, line);
                    layer.Style.IsLocked = true;

                    Assert.Throws<InvalidOperationException>(() =>
                        documentService.AddEntity(layer.Id, new Line(new Point(0, 0), new Point(10, 10))));

                    Assert.Throws<InvalidOperationException>(() => documentService.RemoveEntity(line));
                    Assert.True(documentService.RemoveLayer(layer.Id));
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
            public int RenderCount { get; private set; }

            public void Render(Entity entity, Layer layer, EntityVisual visual)
            {
                RenderCount++;
            }

            public void InvalidateLayerCache(Layer layer)
            {
            }
        }
    }
}
