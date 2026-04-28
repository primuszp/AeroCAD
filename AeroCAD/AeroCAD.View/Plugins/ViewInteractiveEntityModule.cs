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
                    () => new PolygonInteractiveShapeController(),
                    replaceExistingCommand: true);
                yield return new LineInteractiveShapeDefinition(() => new LineCommandController(), replaceExistingCommand: true);
                yield return new CircleInteractiveShapeDefinition(() => new CircleCommandController(), replaceExistingCommand: true);
                yield return new ArcInteractiveShapeDefinition(() => new ArcCommandController(), replaceExistingCommand: true);
                yield return new RectangleInteractiveShapeDefinition(() => new RectangleCommandController(), replaceExistingCommand: true);
                yield return new PolylineInteractiveShapeDefinition(() => new PolylineCommandController(), replaceExistingCommand: true);
            }
        }

        public override IEnumerable<InteractiveCommandRegistration> InteractiveCommands
        {
            get { yield break; }
        }
    }
}
