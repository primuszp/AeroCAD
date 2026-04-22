using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Editing.InteractiveShapes
{
    public class PolygonInteractiveShapeDefinitionTests
    {
        [Fact]
        public void PolygonDefinition_ExposesExpectedCommandMetadata()
        {
            var definition = new PolygonInteractiveShapeDefinition(() => null);

            Assert.Equal("AeroCAD.Polygon", definition.Name);
            Assert.Equal("POLYGON", definition.CommandName);
            Assert.Equal("Draw a regular polygon.", definition.Description);
            Assert.True(definition.AssignActiveLayer);
            Assert.Equal("Draw", definition.MenuGroup);
            Assert.Equal("_Polygon", definition.MenuLabel);
            Assert.NotNull(definition.InitialStep);
            Assert.Equal("Sides", definition.InitialStep.Id);
            Assert.Equal(6, definition.Steps.Count);
            Assert.Equal("Sides", definition.Steps[0].Id);
            Assert.Equal("Placement", definition.Steps[1].Id);
            Assert.Equal("CenterMode", definition.Steps[2].Id);
            Assert.Equal("Radius", definition.Steps[3].Id);
            Assert.Equal("FirstEdge", definition.Steps[4].Id);
            Assert.Equal("SecondEdge", definition.Steps[5].Id);
        }
    }
}
