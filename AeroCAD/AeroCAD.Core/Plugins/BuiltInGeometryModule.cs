using System.Collections.Generic;

namespace Primusz.AeroCAD.Core.Plugins
{
    public sealed class BuiltInGeometryModule : CadModuleBase
    {
        public override string Name => "AeroCAD.BuiltInGeometry";

        public override IEnumerable<IEntityPlugin> Plugins
        {
            get
            {
                yield return new LineEntityPlugin();
                yield return new PolylineEntityPlugin();
                yield return new CircleEntityPlugin();
                yield return new ArcEntityPlugin();
                yield return new PointEntityPlugin();
            }
        }
    }
}
