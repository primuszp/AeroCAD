using System.Linq;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using Primusz.AeroCAD.Core.Plugins;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Editing.InteractiveShapes
{
    public class InteractiveShapeAliasTests
    {
        [Theory]
        [InlineData("LINE", "L")]
        [InlineData("PLINE", "PL")]
        [InlineData("CIRCLE", "C")]
        [InlineData("ARC", "A")]
        [InlineData("RECTANGLE", "REC")]
        [InlineData("RECTANGLE", "RECTANG")]
        [InlineData("POLYGON", "POL")]
        public void DrawShapeDefinitions_ExposeAutoCadStyleAliases(string commandName, string alias)
        {
            var definitions = new IInteractiveShapeDefinition[]
            {
                new LineInteractiveShapeDefinition(() => null),
                new PolylineInteractiveShapeDefinition(() => null),
                new CircleInteractiveShapeDefinition(() => null),
                new ArcInteractiveShapeDefinition(() => null),
                new RectangleInteractiveShapeDefinition(() => null),
                new PolygonInteractiveShapeDefinition(() => null)
            };

            var definition = definitions.Single(item => item.CommandName == commandName);
            var command = definition.CreateCommandRegistration().CreateCommandDefinition();

            Assert.Contains(alias, command.Aliases);
        }
    }
}
