using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Drawing
{
    public class AciPaletteTests
    {
        [Theory]
        [InlineData(1, 255, 0, 0)]      // Red
        [InlineData(2, 255, 255, 0)]    // Yellow
        [InlineData(3, 0, 255, 0)]      // Green
        [InlineData(4, 0, 255, 255)]    // Cyan
        [InlineData(5, 0, 0, 255)]      // Blue
        [InlineData(6, 255, 0, 255)]    // Magenta
        [InlineData(7, 255, 255, 255)]  // White
        public void GetColor_StandardIndexes_ReturnExpectedColor(byte index, byte r, byte g, byte b)
        {
            var color = AciPalette.GetColor(index);
            Assert.Equal(Color.FromRgb(r, g, b), color);
        }

        [Fact]
        public void GetColor_Index10_IsFullRed()
        {
            // Hue group 0 shade 0: primary red at full brightness
            var color = AciPalette.GetColor(10);
            Assert.Equal(Color.FromRgb(255, 0, 0), color);
        }

        [Fact]
        public void GetColor_Index11_IsRedTint()
        {
            // Hue group 0 shade 1: tint of red (midpoint between red and white at same level)
            var color = AciPalette.GetColor(11);
            Assert.Equal(Color.FromRgb(255, 127, 127), color);
        }

        [Fact]
        public void GetColor_Index12_IsDarkRed()
        {
            // Hue group 0 shade 2: red at 165/255 brightness
            var color = AciPalette.GetColor(12);
            Assert.Equal(Color.FromRgb(165, 0, 0), color);
        }

        [Fact]
        public void GetColor_Index13_IsDarkRedTint()
        {
            var color = AciPalette.GetColor(13);
            Assert.Equal(Color.FromRgb(165, 82, 82), color);
        }

        [Fact]
        public void GetColor_Index170_IsBlue()
        {
            // Hue group 16 shade 0: primary blue
            var color = AciPalette.GetColor(170);
            Assert.Equal(Color.FromRgb(0, 0, 255), color);
        }

        [Theory]
        [InlineData(250, 51, 51, 51)]
        [InlineData(255, 255, 255, 255)]
        public void GetColor_GrayscaleRange_ReturnExpectedColor(byte index, byte r, byte g, byte b)
        {
            var color = AciPalette.GetColor(index);
            Assert.Equal(Color.FromRgb(r, g, b), color);
        }

        [Fact]
        public void GetColor_AllIndexes1To255_DoNotThrow()
        {
            for (int i = 1; i <= 255; i++)
            {
                var color = AciPalette.GetColor((byte)i);
                Assert.Equal(255, color.A); // all opaque
            }
        }
    }
}
