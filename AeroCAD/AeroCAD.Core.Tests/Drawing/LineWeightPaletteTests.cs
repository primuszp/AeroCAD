using Primusz.AeroCAD.Core.Drawing.Layers;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Drawing
{
    public class LineWeightPaletteTests
    {
        [Fact]
        public void Default_Is0_25mm()
        {
            Assert.Equal(0.25, LineWeightPalette.Default);
        }

        [Fact]
        public void ToScreenPixels_DefaultLineWeight_Returns1Pixel()
        {
            // 0.25 mm × 4.0 px/mm = 1.0 px
            Assert.Equal(1.0, LineWeightPalette.ToScreenPixels(LineWeightPalette.Default));
        }

        [Theory]
        [InlineData(0.25, 1.0)]
        [InlineData(0.50, 2.0)]
        [InlineData(1.00, 4.0)]
        [InlineData(2.00, 8.0)]
        public void ToScreenPixels_VariousWeights_ScalesLinearly(double mm, double expectedPx)
        {
            Assert.Equal(expectedPx, LineWeightPalette.ToScreenPixels(mm), 6);
        }

        [Fact]
        public void ToScreenPixels_WithDisplayScale_AppliesMultiplier()
        {
            // 0.25 mm at display scale 2.0 = 2 pixels
            Assert.Equal(2.0, LineWeightPalette.ToScreenPixels(0.25, 2.0), 6);
        }

        [Fact]
        public void Snap_ExactStandardValue_ReturnsSameValue()
        {
            Assert.Equal(0.25, LineWeightPalette.Snap(0.25));
            Assert.Equal(1.00, LineWeightPalette.Snap(1.00));
        }

        [Fact]
        public void Snap_ValueBetweenStandards_ReturnsNearest()
        {
            // Between 0.25 and 0.30 — closer to 0.25
            Assert.Equal(0.25, LineWeightPalette.Snap(0.26));
            // Closer to 0.30
            Assert.Equal(0.30, LineWeightPalette.Snap(0.29));
        }

        [Fact]
        public void IsValid_StandardValues_ReturnsTrue()
        {
            foreach (var v in LineWeightPalette.StandardValues)
                Assert.True(LineWeightPalette.IsValid(v));
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(3.0)]
        [InlineData(-1.0)]
        public void IsValid_OutOfRange_ReturnsFalse(double value)
        {
            Assert.False(LineWeightPalette.IsValid(value));
        }

        [Fact]
        public void StandardValues_AreInAscendingOrder()
        {
            var values = LineWeightPalette.StandardValues;
            for (int i = 1; i < values.Length; i++)
                Assert.True(values[i] > values[i - 1]);
        }
    }
}
