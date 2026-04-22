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
                    () => new PolygonInteractiveShapeController());
                yield return new LineInteractiveShapeDefinition(() => new LineCommandController());
                yield return new CircleInteractiveShapeDefinition(() => new CircleCommandController());
                yield return new ArcInteractiveShapeDefinition(() => new ArcCommandController());
                yield return new RectangleInteractiveShapeDefinition(() => new RectangleCommandController());
                yield return new PolylineInteractiveShapeDefinition(() => new PolylineCommandController());
            }
        }

        public override IEnumerable<InteractiveCommandRegistration> InteractiveCommands
        {
            get { yield break; }
        }
    }
}
