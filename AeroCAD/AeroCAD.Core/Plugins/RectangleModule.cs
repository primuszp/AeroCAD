using System.Collections.Generic;

namespace Primusz.AeroCAD.Core.Plugins
{
    public class RectangleModule : CadModuleBase
    {
        public override string Name => "AeroCAD.Rectangle";
        public override string Version => "1.0.0";

        public override IEnumerable<IEntityPlugin> Plugins
        {
            get { yield return new RectangleEntityPlugin(); }
        }
    }
}
