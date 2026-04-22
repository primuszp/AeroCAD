using System.Collections.Generic;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using Primusz.AeroCAD.Core.Plugins;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.View.Plugins
{
    public sealed class ViewInteractiveEntityModule : CadModuleBase
    {
        public override string Name => "AeroCAD.ViewInteractiveEntity";
        public override string Version => "1.0.0";

        public override IEnumerable<IEntityPlugin> Plugins
        {
            get { yield break; }
        }

        public override IEnumerable<IInteractiveShapeDefinition> Shapes
        {
            get
            {
                yield return new PolygonInteractiveShapeDefinition(
                    layerProvider => new PolygonInteractiveShapeController(layerProvider));
                yield return new LineInteractiveShapeDefinition(layerProvider => new LineCommandController(layerProvider));
                yield return new CircleInteractiveShapeDefinition(layerProvider => new CircleCommandController(layerProvider));
                yield return new ArcInteractiveShapeDefinition(layerProvider => new ArcCommandController(layerProvider));
                yield return new RectangleInteractiveShapeDefinition(layerProvider => new RectangleCommandController(layerProvider));
                yield return new PolylineInteractiveShapeDefinition(layerProvider => new PolylineCommandController(layerProvider));
            }
        }

        public override IEnumerable<InteractiveCommandRegistration> InteractiveCommands
        {
            get { yield break; }
        }
    }
}
