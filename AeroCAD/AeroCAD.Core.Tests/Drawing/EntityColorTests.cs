using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Drawing
{
    public class EntityColorTests
    {
        [Fact]
        public void ByLayer_ResolvesToLayerColor()
        {
            var layerColor = Colors.Red;
            var result = EntityColor.ByLayer.Resolve(layerColor);
            Assert.Equal(layerColor, result);
        }

        [Fact]
        public void ByBlock_ResolvesToLayerColor()
        {
            var layerColor = Colors.Blue;
            var result = EntityColor.ByBlock.Resolve(layerColor);
            Assert.Equal(layerColor, result);
        }

        [Fact]
        public void FromRgb_ResolvesToExplicitColor_IgnoringLayerColor()
        {
            var explicitColor = Color.FromRgb(10, 20, 30);
            var result = EntityColor.FromRgb(explicitColor).Resolve(Colors.White);
            Assert.Equal(explicitColor, result);
        }

        [Fact]
        public void FromAci_ResolvesToPaletteColor_IgnoringLayerColor()
        {
            // ACI index 1 is Red (255,0,0) in the AutoCAD palette
            var result = EntityColor.FromAci(1).Resolve(Colors.Blue);
            Assert.Equal(Color.FromRgb(255, 0, 0), result);
        }

        [Fact]
        public void DefaultEntityColor_IsByLayer()
        {
            var entity = new Line(new System.Windows.Point(0, 0), new System.Windows.Point(1, 0));
            Assert.True(entity.Color.IsByLayer);
        }

        [Fact]
        public void IsByLayer_IsTrue_OnlyForByLayer()
        {
            Assert.True(EntityColor.ByLayer.IsByLayer);
            Assert.False(EntityColor.ByBlock.IsByLayer);
            Assert.False(EntityColor.FromAci(1).IsByLayer);
            Assert.False(EntityColor.FromRgb(Colors.Red).IsByLayer);
        }

        [Fact]
        public void Clone_PreservesEntityColor()
        {
            var line = new Line(new System.Windows.Point(0, 0), new System.Windows.Point(10, 0));
            line.Color = EntityColor.FromAci(3);

            var clone = line.Clone() as Line;

            Assert.NotNull(clone);
            Assert.Equal(EntityColorKind.Indexed, clone.Color.Kind);
            Assert.Equal((byte)3, clone.Color.AciIndex);
        }

        [Fact]
        public void RestoreState_RestoresEntityColor()
        {
            var line = new Line(new System.Windows.Point(0, 0), new System.Windows.Point(10, 0));
            line.Color = EntityColor.FromAci(5);
            var snapshot = line.Clone();

            line.Color = EntityColor.FromRgb(Colors.Green);
            line.RestoreState(snapshot);

            Assert.Equal(EntityColorKind.Indexed, line.Color.Kind);
            Assert.Equal((byte)5, line.Color.AciIndex);
        }
    }
}
